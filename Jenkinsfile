```groovy
pipeline {

    agent any

    triggers {
        githubPush()
    }

    options {
        timestamps()
        disableConcurrentBuilds()
        skipDefaultCheckout(true)

        timeout(
            time: 20,
            unit: 'MINUTES'
        )

        buildDiscarder(
            logRotator(
                numToKeepStr: '20',
                artifactNumToKeepStr: '10'
            )
        )
    }

    environment {

        // ============================================================
        // APPLICATION
        // ============================================================

        APP_NAME           = "HRMS-DEV-API"

        MAIN_PROJECT       = "EMS.API.csproj"
        APP_DLL            = "EMS.API.dll"

        DOTNET_ENVIRONMENT = "Production"

        // ============================================================
        // DEPLOYMENT
        // ============================================================

        DEPLOY_PATH        = "/var/www/HRMS/DEV/API"
        PUBLISH_PATH       = "${WORKSPACE}/publish"

        // ============================================================
        // SYSTEMD
        // ============================================================

        SERVICE_NAME       = "hrms-dev-api"

        // ============================================================
        // API
        // ============================================================

        API_PORT           = "6000"

        // ============================================================
        // BACKUP
        // ============================================================

        BACKUP_ROOT        = "/var/www/HRMS/DEV"
    }


    stages {

        // ============================================================
        // 1. PRE-CHECK
        // ============================================================

        stage('Pre-Check') {

            steps {

                sh '''#!/bin/bash

                    set -e

                    echo ""
                    echo "=========================================="
                    echo " HRMS DEV API - PRE CHECK"
                    echo "=========================================="

                    echo ""
                    echo "Checking .NET installation..."

                    if ! command -v dotnet >/dev/null 2>&1; then
                        echo "ERROR: dotnet is not installed."
                        exit 1
                    fi

                    DOTNET_VERSION=$(dotnet --version)

                    echo "Installed .NET Version: $DOTNET_VERSION"

                    echo ""
                    echo "Checking .NET SDK..."

                    dotnet --list-sdks

                    echo ""
                    echo "Checking systemd service..."

                    if ! systemctl cat "${SERVICE_NAME}.service" >/dev/null 2>&1; then
                        echo "ERROR: Service ${SERVICE_NAME}.service not found."
                        exit 1
                    fi

                    echo "Service found: ${SERVICE_NAME}.service"

                    echo ""
                    echo "Checking deployment directory..."

                    sudo mkdir -p "$DEPLOY_PATH"
                    sudo mkdir -p "$BACKUP_ROOT"

                    df -h "$DEPLOY_PATH"

                    echo ""
                    echo "Pre-check completed successfully."

                '''
            }
        }


        // ============================================================
        // 2. CHECKOUT
        // ============================================================

        stage('Checkout') {

            steps {

                echo "Checking out GitHub source..."

                checkout scm

                sh '''#!/bin/bash

                    set -e

                    echo ""
                    echo "=========================================="
                    echo " CHECKOUT VERIFICATION"
                    echo "=========================================="

                    echo ""
                    echo "Workspace:"
                    pwd

                    echo ""
                    echo "Workspace contents:"
                    ls -la

                    echo ""
                    echo "Checking project..."

                    if [ ! -f "$MAIN_PROJECT" ]; then

                        echo "WARNING: $MAIN_PROJECT not found in root."

                        BUILD_TARGET=$(find . -name "$MAIN_PROJECT" -type f | head -n 1)

                        if [ -z "$BUILD_TARGET" ]; then
                            echo "ERROR: $MAIN_PROJECT not found."
                            exit 1
                        fi

                        echo "Project found at:"
                        echo "$BUILD_TARGET"

                    else

                        echo "Project found:"
                        echo "$MAIN_PROJECT"

                    fi

                    echo ""
                    echo "Checking NuGet configuration..."

                    if [ ! -f "nuget.config" ]; then

                        echo "WARNING: nuget.config not found in workspace."

                    else

                        echo "nuget.config found."

                        echo ""
                        echo "NuGet configuration:"
                        cat nuget.config

                    fi

                '''
            }
        }


        // ============================================================
        // 3. CLEAN & RESTORE
        // ============================================================

        stage('Restore') {

            steps {

                sh '''#!/bin/bash

                    set -e

                    echo ""
                    echo "=========================================="
                    echo " CLEAN & RESTORE"
                    echo "=========================================="

                    echo ""
                    echo "Removing generated build artifacts..."

                    # IMPORTANT:
                    # Do NOT use "dotnet clean" here.
                    #
                    # dotnet clean was trying to process the old
                    # Windows-generated project.assets.json and causing:
                    #
                    # C:\\Program Files (x86)\\Microsoft Visual Studio\\...
                    #
                    # Jenkins is running on Linux.
                    #
                    # Therefore remove bin/obj directly.

                    rm -rf bin
                    rm -rf obj

                    # Remove publish directory if it exists.

                    rm -rf "$PUBLISH_PATH"

                    mkdir -p "$PUBLISH_PATH"

                    echo ""
                    echo "Build artifacts removed."

                    echo ""
                    echo "Checking for stale Windows paths..."

                    if grep -R \
                        "C:\\\\Program Files (x86)\\\\Microsoft Visual Studio\\\\Shared\\\\NuGetPackages" \
                        . \
                        --exclude-dir=.git \
                        --exclude-dir=bin \
                        --exclude-dir=obj \
                        2>/dev/null; then

                        echo ""
                        echo "ERROR: Windows-specific NuGet path found."
                        echo "Please remove it from the repository configuration."
                        exit 1
                    fi

                    echo "No stale Windows NuGet path found."

                    echo ""
                    echo "Starting NuGet restore..."

                    if [ -f "$MAIN_PROJECT" ]; then

                        BUILD_TARGET="$MAIN_PROJECT"

                    else

                        BUILD_TARGET=$(find . \
                            -name "$MAIN_PROJECT" \
                            -type f \
                            | head -n 1)

                    fi

                    if [ -z "$BUILD_TARGET" ]; then
                        echo "ERROR: $MAIN_PROJECT not found."
                        exit 1
                    fi

                    echo "Restore project:"
                    echo "$BUILD_TARGET"

                    echo ""

                    if [ -f "nuget.config" ]; then

                        dotnet restore "$BUILD_TARGET" \
                            --configfile nuget.config \
                            --nologo

                    else

                        echo "nuget.config not found."
                        echo "Using default NuGet configuration."

                        dotnet restore "$BUILD_TARGET" \
                            --nologo

                    fi

                    echo ""
                    echo "=========================================="
                    echo " NUGET RESTORE SUCCESSFUL"
                    echo "=========================================="

                '''
            }
        }


        // ============================================================
        // 4. VERIFY RESTORE
        // ============================================================

        stage('Verify Restore') {

            steps {

                sh '''#!/bin/bash

                    set -e

                    echo ""
                    echo "=========================================="
                    echo " VERIFY RESTORE"
                    echo "=========================================="

                    if [ ! -d "obj" ]; then
                        echo "ERROR: obj directory was not created."
                        exit 1
                    fi

                    echo "obj directory created successfully."

                    ASSETS_FILE=$(find . \
                        -path "*/obj/project.assets.json" \
                        -type f \
                        | head -n 1)

                    if [ -z "$ASSETS_FILE" ]; then
                        echo "ERROR: project.assets.json not found."
                        exit 1
                    fi

                    echo ""
                    echo "Assets file:"
                    echo "$ASSETS_FILE"

                    echo ""
                    echo "Checking generated assets for Windows paths..."

                    if grep -q \
                        'C:\\\\Program Files (x86)\\\\Microsoft Visual Studio\\\\Shared\\\\NuGetPackages' \
                        "$ASSETS_FILE" 2>/dev/null; then

                        echo ""
                        echo "ERROR: Windows NuGet fallback path exists in:"
                        echo "$ASSETS_FILE"

                        echo ""
                        echo "This means stale Windows restore information is still being used."

                        exit 1
                    fi

                    echo ""
                    echo "No Windows fallback path found."

                    echo ""
                    echo "Restore verification completed successfully."

                '''
            }
        }


        // ============================================================
        // 5. BUILD & PUBLISH
        // ============================================================

        stage('Build & Publish') {

            steps {

                sh '''#!/bin/bash

                    set -e

                    echo ""
                    echo "=========================================="
                    echo " BUILD & PUBLISH"
                    echo "=========================================="

                    echo ""
                    echo "Finding project..."

                    if [ -f "$MAIN_PROJECT" ]; then

                        BUILD_TARGET="$MAIN_PROJECT"

                    else

                        BUILD_TARGET=$(find . \
                            -name "$MAIN_PROJECT" \
                            -type f \
                            | head -n 1)

                    fi

                    if [ -z "$BUILD_TARGET" ]; then
                        echo "ERROR: $MAIN_PROJECT not found."
                        exit 1
                    fi

                    echo "Build Target:"
                    echo "$BUILD_TARGET"

                    echo ""
                    echo "Starting dotnet publish..."

                    dotnet publish "$BUILD_TARGET" \
                        -c Release \
                        -o "$PUBLISH_PATH" \
                        --no-restore \
                        --nologo

                    echo ""
                    echo "Publish command completed."

                    echo ""
                    echo "Checking published DLL..."

                    if [ ! -f "$PUBLISH_PATH/$APP_DLL" ]; then

                        echo "ERROR: $APP_DLL was not generated."

                        echo ""
                        echo "Publish directory contents:"

                        ls -la "$PUBLISH_PATH"

                        exit 1

                    fi

                    echo ""
                    echo "Application DLL found:"
                    echo "$PUBLISH_PATH/$APP_DLL"

                    # Remove static web asset runtime manifest.
                    # This avoids unnecessary environment-specific
                    # static web asset path issues.

                    rm -f "$PUBLISH_PATH"/*.staticwebassets.runtime.json

                    # Ensure wwwroot exists.

                    mkdir -p "$PUBLISH_PATH/wwwroot"

                    echo ""
                    echo "Published files:"

                    ls -lah "$PUBLISH_PATH"

                    echo ""
                    echo "=========================================="
                    echo " BUILD & PUBLISH SUCCESSFUL"
                    echo "=========================================="

                '''
            }
        }


        // ============================================================
        // 6. BACKUP
        // ============================================================

        stage('Backup') {

            steps {

                sh '''#!/bin/bash

                    set -e

                    echo ""
                    echo "=========================================="
                    echo " BACKUP CURRENT APPLICATION"
                    echo "=========================================="

                    BACKUP_DIR="${BACKUP_ROOT}/API_Backup_$(date +%Y%m%d_%H%M%S)"

                    echo "Backup location:"
                    echo "$BACKUP_DIR"

                    if [ -d "$DEPLOY_PATH" ] && \
                       [ "$(ls -A "$DEPLOY_PATH" 2>/dev/null)" ]; then

                        echo ""
                        echo "Existing deployment found."

                        sudo mkdir -p "$BACKUP_DIR"

                        sudo cp -a \
                            "$DEPLOY_PATH"/. \
                            "$BACKUP_DIR"/

                        echo "$BACKUP_DIR" \
                            | sudo tee "${BACKUP_ROOT}/.last_api_backup" \
                            >/dev/null

                        echo ""
                        echo "Backup completed successfully."

                    else

                        echo ""
                        echo "No existing deployment found."
                        echo "Backup skipped."

                    fi

                '''
            }
        }


        // ============================================================
        // 7. STOP API
        // ============================================================

        stage('Stop API') {

            steps {

                sh '''#!/bin/bash

                    set -e

                    echo ""
                    echo "=========================================="
                    echo " STOP API SERVICE"
                    echo "=========================================="

                    echo "Stopping service:"
                    echo "$SERVICE_NAME"

                    sudo -n /usr/bin/systemctl stop "$SERVICE_NAME" || true

                    sleep 2

                    echo ""
                    echo "Service stop command completed."

                '''
            }
        }


        // ============================================================
        // 8. DEPLOY
        // ============================================================

        stage('Deploy') {

            steps {

                sh '''#!/bin/bash

                    set -e

                    echo ""
                    echo "=========================================="
                    echo " DEPLOY APPLICATION"
                    echo "=========================================="

                    echo "Deployment path:"
                    echo "$DEPLOY_PATH"

                    echo ""
                    echo "Cleaning existing deployment files..."

                    sudo rm -rf "$DEPLOY_PATH"/*

                    echo ""
                    echo "Copying published files..."

                    sudo cp -a \
                        "$PUBLISH_PATH"/. \
                        "$DEPLOY_PATH"/

                    echo ""
                    echo "Setting ownership..."

                    sudo chown -R \
                        www-data:www-data \
                        "$DEPLOY_PATH"

                    echo ""
                    echo "Setting permissions..."

                    sudo chmod -R 755 "$DEPLOY_PATH"

                    echo ""
                    echo "Verifying deployment..."

                    if [ ! -f "$DEPLOY_PATH/$APP_DLL" ]; then

                        echo "ERROR: $APP_DLL missing after deployment."

                        echo ""
                        echo "Deployment directory contents:"

                        sudo ls -la "$DEPLOY_PATH"

                        exit 1

                    fi

                    echo ""
                    echo "Application deployed successfully."

                    echo ""
                    echo "Deployed files:"

                    sudo ls -lah "$DEPLOY_PATH"

                '''
            }
        }


        // ============================================================
        // 9. START & VERIFY API
        // ============================================================

        stage('Start & Verify API') {

            steps {

                sh '''#!/bin/bash

                    set -e

                    echo ""
                    echo "=========================================="
                    echo " START & VERIFY API"
                    echo "=========================================="

                    echo "Reloading systemd..."

                    sudo -n /usr/bin/systemctl daemon-reload

                    echo ""
                    echo "Starting service:"
                    echo "$SERVICE_NAME"

                    sudo -n /usr/bin/systemctl start "$SERVICE_NAME"

                    echo ""
                    echo "Waiting for service..."

                    sleep 2

                    echo ""
                    echo "Checking systemd service status..."

                    if ! sudo -n /usr/bin/systemctl is-active \
                        --quiet "$SERVICE_NAME"; then

                        echo "ERROR: Service is not active."

                        echo ""
                        echo "Service status:"

                        sudo -n /usr/bin/systemctl status \
                            "$SERVICE_NAME" \
                            --no-pager \
                            -l || true

                        echo ""
                        echo "Recent logs:"

                        sudo -n /usr/bin/journalctl \
                            -u "$SERVICE_NAME" \
                            -n 100 \
                            --no-pager || true

                        exit 1

                    fi

                    echo ""
                    echo "Service is active."

                    echo ""
                    echo "Waiting for port $API_PORT..."

                    PORT_READY=false

                    for i in {1..20}; do

                        if ss -lnt | grep -q ":${API_PORT} "; then

                            echo ""
                            echo "Service is listening on port $API_PORT."

                            PORT_READY=true

                            break

                        fi

                        echo "Waiting... attempt $i/20"

                        sleep 2

                    done

                    if [ "$PORT_READY" != "true" ]; then

                        echo ""
                        echo "ERROR: Port $API_PORT is not listening."

                        echo ""
                        echo "=========================================="
                        echo " SERVICE STATUS"
                        echo "=========================================="

                        sudo -n /usr/bin/systemctl status \
                            "$SERVICE_NAME" \
                            --no-pager \
                            -l || true

                        echo ""
                        echo "=========================================="
                        echo " SERVICE LOGS"
                        echo "=========================================="

                        sudo -n /usr/bin/journalctl \
                            -u "$SERVICE_NAME" \
                            -n 100 \
                            --no-pager || true

                        echo ""
                        echo "=========================================="
                        echo " LISTENING PORTS"
                        echo "=========================================="

                        ss -lnt || true

                        exit 1

                    fi

                    echo ""
                    echo "=========================================="
                    echo " API VERIFICATION SUCCESSFUL"
                    echo "=========================================="

                '''
            }
        }
    }


    // ================================================================
    // POST ACTIONS
    // ================================================================

    post {

        // ============================================================
        // SUCCESS
        // ============================================================

        success {

            echo """

==========================================
 DEPLOYMENT SUCCESSFUL
==========================================

Application : ${APP_NAME}
Service     : ${SERVICE_NAME}
Environment : ${DOTNET_ENVIRONMENT}
Port        : ${API_PORT}
Path        : ${DEPLOY_PATH}

==========================================

"""
        }


        // ============================================================
        // FAILURE / ROLLBACK
        // ============================================================

        failure {

            echo """
==========================================
 DEPLOYMENT FAILED
==========================================

Starting rollback process...

==========================================
"""

            sh '''#!/bin/bash

                set +e

                BACKUP_POINTER="${BACKUP_ROOT}/.last_api_backup"

                echo ""
                echo "Checking backup pointer..."

                if [ -f "$BACKUP_POINTER" ]; then

                    RESTORE_PATH=$(cat "$BACKUP_POINTER")

                    echo "Backup found:"
                    echo "$RESTORE_PATH"

                    if [ -d "$RESTORE_PATH" ]; then

                        echo ""
                        echo "=========================================="
                        echo " STARTING ROLLBACK"
                        echo "=========================================="

                        echo ""
                        echo "Stopping service..."

                        sudo -n /usr/bin/systemctl stop \
                            "$SERVICE_NAME" || true

                        echo ""
                        echo "Removing failed deployment..."

                        sudo rm -rf "$DEPLOY_PATH"/*

                        echo ""
                        echo "Restoring previous version..."

                        sudo cp -a \
                            "$RESTORE_PATH"/. \
                            "$DEPLOY_PATH"/

                        echo ""
                        echo "Restoring ownership..."

                        sudo chown -R \
                            www-data:www-data \
                            "$DEPLOY_PATH"

                        echo ""
                        echo "Restoring permissions..."

                        sudo chmod -R 755 "$DEPLOY_PATH"

                        echo ""
                        echo "Starting previous version..."

                        sudo -n /usr/bin/systemctl start \
                            "$SERVICE_NAME" || true

                        echo ""
                        echo "Checking rollback service..."

                        sleep 3

                        if sudo -n /usr/bin/systemctl is-active \
                            --quiet "$SERVICE_NAME"; then

                            echo ""
                            echo "=========================================="
                            echo " ROLLBACK SUCCESSFUL"
                            echo "=========================================="

                        else

                            echo ""
                            echo "WARNING: Rollback completed but service is not active."

                            sudo -n /usr/bin/systemctl status \
                                "$SERVICE_NAME" \
                                --no-pager \
                                -l || true

                        fi

                    else

                        echo ""
                        echo "WARNING: Backup directory does not exist:"
                        echo "$RESTORE_PATH"

                    fi

                else

                    echo ""
                    echo "WARNING: No previous backup found."
                    echo "Rollback skipped."

                fi

            '''
        }


        // ============================================================
        // ALWAYS
        // ============================================================

        always {

            echo "Cleaning Jenkins workspace..."

            cleanWs(
                deleteDirs: true,
                disableDeferredWipeout: true,
                notFailBuild: true
            )

        }
    }
}
```
