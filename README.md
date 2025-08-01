# k8s-restart

A lightweight REST API for restarting, stopping, and starting Kubernetes workloads via HTTP. Built with .NET 8 and deployable to **any Kubernetes cluster** — cloud or on-prem.

---

## 🚀 Features

- Restart all pods in a namespace
- Scale deployments to 0 (stop)
- Restore deployments to their previous replica count (start)
- Restart individual deployments or pods
- Protected with optional Basic Authentication
- Swagger UI built-in

---

## 📘 Works With Any Kubernetes Cluster

This service is compatible with:

- ✅ **AKS** (Azure Kubernetes Service)
- ✅ **EKS** (Amazon Elastic Kubernetes Service)
- ✅ **GKE** (Google Kubernetes Engine)
- ✅ **On-prem clusters** (e.g., kubeadm, Rancher)
- ✅ **kind / minikube / k3s**

> As long as:
> - The cluster supports **standard Kubernetes RBAC**
> - You are running **Kubernetes v1.21+**
> - You apply the **ClusterRole + ServiceAccount + Secret** setup correctly

---

## 🔐 Authentication

Set the following environment variables to enable Basic Auth:

```bash
ENABLE_BASIC_AUTH=true
BASIC_AUTH_USER=admin
BASIC_AUTH_PASSWORD=secret
```

When enabled, Swagger will show an "Authorize" button. Without it, the API is unauthenticated (for internal/test use only).

---

## 📘 API Endpoints (Swagger)

| Method | Endpoint                                                      | Description                                       |
|--------|---------------------------------------------------------------|---------------------------------------------------|
| POST   | `/namespace/{ns}/restart`                                     | Restart all pods in the namespace                 |
| POST   | `/namespace/{ns}/stop`                                        | Scale all deployments to 0 replicas              |
| POST   | `/namespace/{ns}/start`                                       | Restore deployments to their previous replica counts |
| POST   | `/namespace/{ns}/deployment/{deployment}/restart`            | Restart all pods in the deployment               |
| POST   | `/namespace/{ns}/deployment/{deployment}/stop`               | Scale deployment to 0 replicas                   |
| POST   | `/namespace/{ns}/deployment/{deployment}/start`              | Scale deployment to last known replica count     |
| POST   | `/namespace/{ns}/pod/{pod}/restart`                          | Restart a single pod                             |

---

## 🔐 Required ClusterRole and Binding

The service must run with a `ClusterRole` bound to a `ServiceAccount` with access to pods and deployments in **all namespaces**.

### ClusterRole

```yaml
apiVersion: rbac.authorization.k8s.io/v1
kind: ClusterRole
metadata:
  name: k8s-restart-role
rules:
- apiGroups: [""]
  resources: ["pods"]
  verbs: ["get", "list", "delete"]
- apiGroups: ["apps"]
  resources: ["deployments", "replicasets"]
  verbs: ["get", "list", "patch", "update"]
```

### ClusterRoleBinding

```yaml
apiVersion: rbac.authorization.k8s.io/v1
kind: ClusterRoleBinding
metadata:
  name: k8s-restart-binding
subjects:
- kind: ServiceAccount
  name: k8s-restart
  namespace: kube-system
roleRef:
  kind: ClusterRole
  name: k8s-restart-role
  apiGroup: rbac.authorization.k8s.io
```

### ServiceAccount

```yaml
apiVersion: v1
kind: ServiceAccount
metadata:
  name: k8s-restart
  namespace: kube-system
```

---

## 📦 Deployment

### 1. Create the basic auth secret

```bash
kubectl create secret generic k8s-restart-auth \
  --namespace kube-system \
  --from-literal=user=admin \
  --from-literal=password=secret
```

### 2. Apply with Kustomize

```bash
kubectl apply -k k8s/base
```

This deploys:
- The API service
- Ingress
- ServiceAccount
- ClusterRole + Binding

---

## 🔎 Access

Once deployed, access the Swagger UI at:

```
https://<your-domain>/swagger
```

Log in with Basic Auth if enabled, and interact with the API directly from the browser.

---

## © Ownership

This software is © 2025 Affinity7 Consulting Ltd.  
It is licensed under the [MIT License](https://opensource.org/licenses/MIT), allowing commercial and non-commercial use, modification, and distribution, with attribution.

**Disclaimer:** This software is provided "as is", without warranty of any kind. Use it at your own risk.

For support or licensing inquiries, visit [Affinity7 Software](https://www.affinity7software.com).

---

## 📝 License

This project is licensed under the MIT License:

```text
MIT License

Copyright (c) 2025 Affinity7 Consulting Ltd.

Permission is hereby granted, free of charge, to any person obtaining a copy
of this software and associated documentation files (the “Software”), to deal
in the Software without restriction, including without limitation the rights
to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
copies of the Software, and to permit persons to whom the Software is
furnished to do so, subject to the following conditions:

The above copyright notice and this permission notice shall be included in all
copies or substantial portions of the Software.

THE SOFTWARE IS PROVIDED “AS IS”, WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
SOFTWARE.
```
