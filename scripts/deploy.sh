#!/bin/bash

# TPOR Intranet Deployment Script
# This script builds and deploys both API and Worker services to Google Cloud Run

set -e

# Configuration
PROJECT_ID=${GOOGLE_CLOUD_PROJECT_ID:-"tpor-project"}
REGION=${GOOGLE_CLOUD_REGION:-"us-central1"}
API_SERVICE_NAME="tpor-intranet-api"
WORKER_SERVICE_NAME="tpor-intranet-worker"

echo "Starting deployment to Google Cloud Run..."
echo "Project ID: $PROJECT_ID"
echo "Region: $REGION"

# Build and push API service
echo "Building and pushing API service..."
docker build -f src/TPOR.Intranet.API/Dockerfile -t gcr.io/$PROJECT_ID/$API_SERVICE_NAME:latest .
docker push gcr.io/$PROJECT_ID/$API_SERVICE_NAME:latest

# Build and push Worker service
echo "Building and pushing Worker service..."
docker build -f src/TPOR.Intranet.Worker/Dockerfile -t gcr.io/$PROJECT_ID/$WORKER_SERVICE_NAME:latest .
docker push gcr.io/$PROJECT_ID/$WORKER_SERVICE_NAME:latest

# Deploy API service
echo "Deploying API service..."
gcloud run deploy $API_SERVICE_NAME \
  --image gcr.io/$PROJECT_ID/$API_SERVICE_NAME:latest \
  --platform managed \
  --region $REGION \
  --allow-unauthenticated \
  --memory 2Gi \
  --cpu 2 \
  --timeout 300 \
  --max-instances 10 \
  --set-env-vars ASPNETCORE_ENVIRONMENT=Production

# Deploy Worker service
echo "Deploying Worker service..."
gcloud run deploy $WORKER_SERVICE_NAME \
  --image gcr.io/$PROJECT_ID/$WORKER_SERVICE_NAME:latest \
  --platform managed \
  --region $REGION \
  --no-allow-unauthenticated \
  --memory 2Gi \
  --cpu 2 \
  --timeout 3600 \
  --max-instances 5 \
  --set-env-vars ASPNETCORE_ENVIRONMENT=Production

echo "Deployment completed successfully!"
echo "API Service URL: https://$API_SERVICE_NAME-$PROJECT_ID.a.run.app"
echo "Worker Service: Internal only"
