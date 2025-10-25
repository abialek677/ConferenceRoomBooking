
# Conference Room Booking – Kubernetes Deployment (Minikube + Docker)

This repository provides a complete local Kubernetes deployment setup for the **Conference Room Booking** application using **Minikube**, **Docker**, and **PostgreSQL**.

The setup lets you build and run everything locally using the included PowerShell script.

---

## Prerequisites

You need the following tools installed before running the setup:

| Tool | Description |
|------|-------------|
| [Docker Desktop](https://www.docker.com/products/docker-desktop/) | Used to build images |
| [Minikube](https://minikube.sigs.k8s.io/docs/start/) | Local Kubernetes cluster |
| [kubectl](https://kubernetes.io/docs/tasks/tools/install-kubectl/) | Kubernetes CLI |
| [PowerShell](https://learn.microsoft.com/en-us/powershell/) | To execute the deployment script |

---

##  Setup Instructions

1. Open PowerShell in the project root directory.
2. Start minikube container
```powershell
minikube start --driver=docker
```
3. Run the deployment script:

```powershell
.\start.ps1
```

This script will:

- Set the Docker environment for Minikube
- Build the Docker image for the app
- Apply Kubernetes manifests for PostgreSQL and the app
- Wait until all pods are ready

Once the script completes, you can access the app using:

```powershell
minikube service conference-app
```

---

## Running Locally Without Docker or Minikube

You can run the **Conference Room Booking** application directly on your machine by connecting to a local PostgreSQL database.

### 1. Make sure PostgreSQL is running

- Have a local PostgreSQL server running.
- Note your database, username, and password.

---

### 2. Update the Database Connection String

- Open `appsettings.json` in the root of your project.
- Set the DefaultConnection string for your setup:

```json
"ConnectionStrings": {
  "DefaultConnection": "Host=localhost;Port=5432;Database=ConferenceRoomBookingDb;Username=postgres;Password=your_password"
}
```

Update `Database`, `Username`, and `Password` with your own values.

---

### 3. Build and Run the App

You can use your IDE, or run from the command line:


---

## Troubleshooting

**Pods not starting:**  
Run:

```powershell
kubectl get pods
kubectl describe pod <pod-name>
```

to view logs or events for details.

**Service not reachable:**  
List all running services and confirm the correct name and port:

```powershell
minikube service list
```
