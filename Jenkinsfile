pipeline {

    agent any

    triggers {
        githubPush()
    }

    options {
        timestamps()
        disableConcurrentBuilds()
        skipDefaultCheckout(true)
        timeout(time: 20, unit: 'MINUTES')
        buildDiscarder(
            logRotator(
                numToKeepStr: '20',
                artifactNumToKeepStr: '10'
            )
        )
    }

    environment {
        APP_NAME           = "HRMS-DEV-API"
        DEPLOY_PATH        = "/var/www/HRMS/DEV/API"
        PUBLISH_PATH       = "${WORKSPACE}/publish"
        SERVICE_NAME       = "hrms-dev-api"
        DOTNET_ENVIRONMENT = "Production"
        API_PORT           = "6000"
        BACKUP_ROOT        = "/var/www/HRMS/DEV"
        MAIN_PROJECT       = "EMS.API.csproj"
        APP_DLL            = "EMS.API.dll"
    }

    stages {

        // ============================================================
        // 1. PRE-CHECK
        // ============================================================
        stage('Pre-Check') {
            steps {
                sh '''#!/bin/bash
                    set -e
                    echo "=========================================="
                    echo " HRMS DEV API Deployment Pre-Check"
                    echo "=========================================="

                    if ! command -v dotnet >/dev/null 2>&1; then
                        echo "ERROR: dotnet is not installed."
                        exit 1
                    fi

                    DOTNET_VERSION=$(dotnet --version)
                    echo "Installed .NET Version: $DOTNET_VERSION"

                    if ! systemctl cat "${SERVICE_NAME}.service" >/dev/null 2>&1; then
                        echo "ERROR: Service ${SERVICE_NAME}.service not found."
                        exit 1
                    fi
                    echo "Service found: ${SERVICE_NAME}.service"

                    sudo mkdir -p "$DEPLOY_PATH"
                    df -h "$DEPLOY_PATH"
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
            }
        }

        // ============================================================
        // 3. CLEAN & RESTORE
        // ============================================================
       stage('Restore') {
    steps {
        sh '''#!/bin/bash
            set -e

            echo "=========================================="
            echo " CLEAN & RESTORE"
            echo "=========================================="

            echo "Removing generated build artifacts..."

            rm -rf bin
            rm -rf obj
            rm -rf "$PUBLISH_PATH"

            mkdir -p "$PUBLISH_PATH"

            echo "Restoring NuGet packages..."

            dotnet restore "$MAIN_PROJECT" \
                --configfile nuget.config \
                --nologo

            echo "Restore completed successfully."
        '''
    }
}

stage('Build & Publish') {
    steps {
        sh '''#!/bin/bash
            set -e

            echo "=========================================="
            echo " BUILD & PUBLISH"
            echo "=========================================="

            echo "Building and Publishing $MAIN_PROJECT..."

            if [ -f "$MAIN_PROJECT" ]; then
                BUILD_TARGET="$MAIN_PROJECT"
            else
                BUILD_TARGET=$(find . -name "EMS.API.csproj" | head -n 1)
            fi

            if [ -z "$BUILD_TARGET" ]; then
                echo "ERROR: EMS.API.csproj not found."
                exit 1
            fi

            echo "Build Target: $BUILD_TARGET"

            dotnet publish "$BUILD_TARGET" \
                -c Release \
                -o "$PUBLISH_PATH" \
                --no-restore \
                --nologo

            if [ ! -f "$PUBLISH_PATH/$APP_DLL" ]; then
                echo "ERROR: $APP_DLL was not generated."
                echo "Contents of publish directory:"
                ls -la "$PUBLISH_PATH"
                exit 1
            fi

            rm -f "$PUBLISH_PATH"/*.staticwebassets.runtime.json

            mkdir -p "$PUBLISH_PATH/wwwroot"

            echo "Publish verified successfully."
        '''
    }
}

        // ============================================================
        // 5. BACKUP
        // ============================================================
        // ============================================================
// 5. BACKUP
// ============================================================
stage('Backup') {
    steps {
        sh '''#!/bin/bash
            set -e

            BACKUP_DIR="${BACKUP_ROOT}/API_Backup_$(date +%Y%m%d_%H%M%S)"

            echo "Creating backup at:"
            echo "$BACKUP_DIR"

            if [ -d "$DEPLOY_PATH" ] && [ "$(sudo -n ls -A "$DEPLOY_PATH" 2>/dev/null)" ]; then

                sudo -n mkdir -p "$BACKUP_DIR"

                sudo -n rsync -r \
                    --omit-dir-times \
                    "$DEPLOY_PATH"/ \
                    "$BACKUP_DIR"/

                echo "$BACKUP_DIR" | sudo -n tee "${BACKUP_ROOT}/.last_api_backup" > /dev/null

                echo "Backup completed successfully."

            else
                echo "No existing deployment found."
                echo "Skipping backup."
            fi
        '''
    }
}
        // ============================================================
        // 6. STOP SERVICE
        // ============================================================
        stage('Stop API') {
            steps {
                sh '''#!/bin/bash
                    echo "Stopping $SERVICE_NAME..."
                    sudo -n /usr/bin/systemctl stop "$SERVICE_NAME" || true
                    sleep 2
                '''
            }
        }

        // ============================================================
        // 7. DEPLOY
        // ============================================================
        // ============================================================
// 7. DEPLOY
// ============================================================
stage('Deploy') {
    steps {
        sh '''#!/bin/bash
            set -e

            echo "=========================================="
            echo " DEPLOYING HRMS DEV API"
            echo "=========================================="

            echo "Deploy Path  : $DEPLOY_PATH"
            echo "Publish Path : $PUBLISH_PATH"

            # Make sure deployment directory exists
            sudo -n mkdir -p "$DEPLOY_PATH"

            echo "Cleaning existing deployment files..."

            sudo -n rm -rf "$DEPLOY_PATH"/*

            echo "Deploying new binaries..."

            # IMPORTANT:
            # Do not use cp -a or cp -r here.
            # rsync avoids the directory timestamp preservation issue.
            sudo -n rsync -r \
                --delete \
                --omit-dir-times \
                "$PUBLISH_PATH"/ \
                "$DEPLOY_PATH"/

            echo "Setting ownership..."

            sudo -n chown -R www-data:www-data "$DEPLOY_PATH"

            echo "Setting permissions..."

            sudo -n find "$DEPLOY_PATH" -type d -exec chmod 755 {} \\;
            sudo -n find "$DEPLOY_PATH" -type f -exec chmod 644 {} \\;

            echo "Verifying deployed application..."

            if [ ! -f "$DEPLOY_PATH/$APP_DLL" ]; then
                echo "ERROR: $APP_DLL missing after deployment."
                echo "Deployment directory contents:"
                sudo -n ls -la "$DEPLOY_PATH"
                exit 1
            fi

            echo "Deployment files:"
            sudo -n ls -lah "$DEPLOY_PATH"

            echo "Deployment completed successfully."
        '''
    }
}

        // ============================================================
        // 8. START & VERIFY SERVICE
        // ============================================================
        stage('Start & Verify API') {
            steps {
                sh '''#!/bin/bash
                    set -e
                    echo "Starting $SERVICE_NAME..."
                    sudo -n /usr/bin/systemctl daemon-reload
                    sudo -n /usr/bin/systemctl start "$SERVICE_NAME"

                    echo "Waiting for port $API_PORT to become active..."
                    PORT_READY=false
                    for i in {1..15}; do
                        if ss -lnt | grep -q ":${API_PORT} "; then
                            echo "Service listening on port $API_PORT."
                            PORT_READY=true
                            break
                        fi
                        sleep 2
                    done

                    if [ "$PORT_READY" != "true" ]; then
                        echo "ERROR: Port $API_PORT is not listening."
                        sudo -n /usr/bin/journalctl -u "$SERVICE_NAME" -n 50 --no-pager
                        exit 1
                    fi
                '''
            }
        }
    }

    // ================================================================
    // POST ACTIONS
    // ================================================================
    post {
        success {
            echo """
==========================================
DEPLOYMENT SUCCESSFUL
==========================================
Application : ${APP_NAME}
Service     : ${SERVICE_NAME}
Port        : ${API_PORT}
Path        : ${DEPLOY_PATH}
==========================================
"""
        }

        failure {
    echo "Deployment failed. Rolling back..."

    sh '''#!/bin/bash
        set +e

        BACKUP_POINTER="${BACKUP_ROOT}/.last_api_backup"

        if [ -f "$BACKUP_POINTER" ]; then

            RESTORE_PATH=$(cat "$BACKUP_POINTER")

            if [ -d "$RESTORE_PATH" ]; then

                echo "=========================================="
                echo " ROLLING BACK"
                echo "=========================================="

                echo "Restore Path: $RESTORE_PATH"

                sudo -n /usr/bin/systemctl stop "$SERVICE_NAME" || true

                sudo -n mkdir -p "$DEPLOY_PATH"

                sudo -n rm -rf "$DEPLOY_PATH"/*

                sudo -n rsync -r \
                    --delete \
                    --omit-dir-times \
                    "$RESTORE_PATH"/ \
                    "$DEPLOY_PATH"/

                sudo -n chown -R www-data:www-data "$DEPLOY_PATH"

                sudo -n find "$DEPLOY_PATH" -type d -exec chmod 755 {} \\;
                sudo -n find "$DEPLOY_PATH" -type f -exec chmod 644 {} \\;

                sudo -n /usr/bin/systemctl start "$SERVICE_NAME" || true

                echo "Rollback completed."

            else
                echo "Backup directory does not exist:"
                echo "$RESTORE_PATH"
            fi

        else
            echo "No backup pointer found."
            echo "Rollback cannot be performed."
        fi
    '''
}
        always {
            cleanWs(deleteDirs: true, disableDeferredWipeout: true, notFailBuild: true)
        }
    }
}