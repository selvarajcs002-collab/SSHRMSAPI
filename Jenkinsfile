pipeline {

    agent any

    triggers {
        githubPush()
    }

    options {
        timestamps()
        disableConcurrentBuilds()
        timeout(time: 20, unit: 'MINUTES')
        buildDiscarder(
            logRotator(
                numToKeepStr: '20',
                artifactNumToKeepStr: '10'
            )
        )
    }

    environment {

        // Application
        APP_NAME = "HRMS-DEV-API"
        SERVICE_NAME = "hrms-dev-api"
        API_PORT = "6000"

        // Project
        PROJECT_FILE = "EMS.API.csproj"
        APP_DLL = "EMS.API.dll"

        // Build
        PUBLISH_PATH = "${WORKSPACE}/publish"

        // Server
        DEPLOY_ROOT = "/var/www/HRMS/DEV"
        DEPLOY_PATH = "/var/www/HRMS/DEV/API"
    }

    stages {

        // =========================================================
        // 1. CHECKOUT
        // =========================================================
        stage('Checkout') {
            steps {
                echo "Checking out source code..."

                checkout scm
            }
        }


        // =========================================================
        // 2. RESTORE
        // =========================================================
        stage('Restore') {
            steps {
                sh '''
                    set -e

                    echo "Restoring NuGet packages..."

                    dotnet restore "$PROJECT_FILE"

                    echo "Restore completed."
                '''
            }
        }


        // =========================================================
        // 3. BUILD & PUBLISH
        // =========================================================
        stage('Build & Publish') {
            steps {
                sh '''
                    set -e

                    echo "Building application..."

                    rm -rf "$PUBLISH_PATH"

                    mkdir -p "$PUBLISH_PATH"

                    dotnet publish "$PROJECT_FILE" \
                        -c Release \
                        -o "$PUBLISH_PATH" \
                        --no-restore

                    echo "Publish completed."

                    if [ ! -f "$PUBLISH_PATH/$APP_DLL" ]; then
                        echo "ERROR: $APP_DLL not found."
                        exit 1
                    fi

                    echo "Build verified successfully."
                '''
            }
        }


        // =========================================================
        // 4. STOP SERVICE
        // =========================================================
        stage('Stop Service') {
            steps {
                sh '''
                    echo "Stopping $SERVICE_NAME..."

                    sudo -n systemctl stop "$SERVICE_NAME" || true

                    sleep 2
                '''
            }
        }


        // =========================================================
        // 5. BACKUP OLD API
        // =========================================================
        stage('Backup Current API') {
            steps {
                sh '''
                    set -e

                    echo "Checking existing deployment..."

                    if [ -d "$DEPLOY_PATH" ]; then

                        BACKUP_NAME="API_$(date +%Y%m%d_%H%M%S)"
                        BACKUP_PATH="$DEPLOY_ROOT/$BACKUP_NAME"

                        echo "Moving current API to:"
                        echo "$BACKUP_PATH"

                        sudo -n mv "$DEPLOY_PATH" "$BACKUP_PATH"

                        echo "Backup completed."

                    else

                        echo "No existing API deployment found."
                    fi
                '''
            }
        }


        // =========================================================
        // 6. DEPLOY NEW BUILD
        // =========================================================
        stage('Deploy') {
            steps {
                sh '''
                    set -e

                    echo "Deploying new build..."

                    sudo -n mkdir -p "$DEPLOY_PATH"

                    sudo -n rsync -r \
                        --omit-dir-times \
                        "$PUBLISH_PATH"/ \
                        "$DEPLOY_PATH"/

                    sudo -n chown -R www-data:www-data "$DEPLOY_PATH"

                    sudo -n find "$DEPLOY_PATH" \
                        -type d \
                        -exec chmod 755 {} \\;

                    sudo -n find "$DEPLOY_PATH" \
                        -type f \
                        -exec chmod 644 {} \\;

                    if [ ! -f "$DEPLOY_PATH/$APP_DLL" ]; then
                        echo "ERROR: Deployment failed."
                        exit 1
                    fi

                    echo "Deployment completed."
                '''
            }
        }


        // =========================================================
        // 7. START & VERIFY
        // =========================================================
        stage('Start & Verify') {
            steps {
                sh '''
                    set -e

                    echo "Starting $SERVICE_NAME..."

                    sudo -n systemctl daemon-reload

                    sudo -n systemctl start "$SERVICE_NAME"

                    echo "Waiting for API port $API_PORT..."

                    for i in {1..15}; do

                        if ss -lnt | grep -q ":${API_PORT} "; then

                            echo "API is running."
                            echo "Port $API_PORT is listening."

                            exit 0
                        fi

                        sleep 2
                    done

                    echo "ERROR: API did not start."

                    sudo -n journalctl \
                        -u "$SERVICE_NAME" \
                        -n 50 \
                        --no-pager

                    exit 1
                '''
            }
        }
    }


    // =============================================================
    // POST ACTIONS
    // =============================================================
    post {

        success {
            echo """
==========================================
 HRMS DEV API DEPLOYMENT SUCCESSFUL
==========================================

Application : ${APP_NAME}
Service     : ${SERVICE_NAME}
Port        : ${API_PORT}
Path        : ${DEPLOY_PATH}

==========================================
"""
        }

        failure {
            echo """
==========================================
 HRMS DEV API DEPLOYMENT FAILED
==========================================
Please check the Jenkins console log.
==========================================
"""
        }

        always {
            cleanWs(
                deleteDirs: true,
                disableDeferredWipeout: true,
                notFailBuild: true
            )
        }
    }
}