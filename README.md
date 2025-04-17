# 🔗 Quick-share Tool

Simple cross-platform web tool for sharing clipboard text and files between multiple devices without relying on third-party services.

---

## 🚀 Why I Built This

As a developer, I often work across multiple devices: a Windows work laptop, a personal Linux machine, and an Android phone. I wanted a **simple** way to share clipboard text or small files between them — without syncing credentials or using external cloud services.

This project is my personal solution — and an opportunity to explore new tech, apply patterns, and build something useful for everyday workflows.

---

## 🛠️ Tech Stack

- **Backend:** .NET 8 Minimal API, Redis (ephemeral storage), Serilog (logging)
- **Frontend:** Vue.js 3
- **DevOps:** Docker (local deployment), Jenkins CI/CD (in progress)
- **Monitoring/Logging:** Serilog (Grafana/Prometheus planned)
- **Infrastructure:** Local homelab server (Kubernetes experimentation coming)

---

## 🔐 Key Features

- ✅ Share clipboard text across devices in real time
- ✅ Upload and transfer small files securely (file size limits enforced)
- ✅ No permanent data storage (Redis handles short-term sessions)
- 🚧 Cross-platform support (Windows, Linux, Android browser)
- 🚧 In development: User Frontend, Jenkins pipelines, Kubernetes deployment, user-friendly session expiration

---

## 🧰 Getting Started

### Prerequisites

- Docker & Docker Compose installed
- .NET 8 SDK (for manual local builds)

### Local Development

```bash
docker run -d --name local-redis -p 6379:6379 redis

git clone https://github.com/jorgepintordev/quick-share.git
cd quick-share/src
docker-compose up --build
```

---
## 🚧 MVP roadmap
### API
- Code Refactor, Service class into multiple services by functionality to have clear code
- remove hardcoded settings, use appsettings 
- Add Seq/Graylog/Grafana/Prometheus
- Unit test

### Frontend
- Create Vue project

### Infra
- Docker compose
