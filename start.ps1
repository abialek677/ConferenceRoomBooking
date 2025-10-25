# Set Docker environment for Minikube
& minikube -p minikube docker-env | Invoke-Expression

# Build the Docker image
docker build -t conference_room_booking:latest .

# Apply Kubernetes manifests
kubectl apply -f k8s/postgres-deployment.yaml
kubectl apply -f k8s/app-deployment.yaml

Write-Host "Waiting for pods to be ready..."
kubectl wait --for=condition=ready pod -l app=conference-app --timeout=120s
kubectl wait --for=condition=ready pod -l app=postgres --timeout=120s

Write-Host "Deployment done. To access the app try:"
Write-Host "minikube service conference-app"
