# dotnet-webapi-k8s-boilerplate

[![.NET](https://github.com/osisdie/dotnet-webapi-k8s-boilerplate/actions/workflows/dotnet.yml/badge.svg)](https://github.com/osisdie/dotnet-webapi-k8s-boilerplate/actions/workflows/dotnet.yml)
[![License: MIT](https://img.shields.io/badge/License-MIT-yellow.svg)](https://opensource.org/licenses/MIT)
[![.NET 8](https://img.shields.io/badge/.NET-8.0-blue.svg)](https://dotnet.microsoft.com/download/dotnet/8.0)
[![.NET 10](https://img.shields.io/badge/.NET-10.0-purple.svg)](https://dotnet.microsoft.com/download/dotnet/10.0)
[![Docker](https://img.shields.io/badge/docker-ready-blue)](Dockerfile)
[![Kubernetes](https://img.shields.io/badge/kubernetes-minikube-blue)](minikube/)

> Production-ready .NET WebAPI framework with structured responses, request/response logging middleware, Redis caching (circuit-breaker), and Kubernetes deployment — supporting both .NET 8 and .NET 10. Clone and ship.

---

## What's Inside

This repo contains two layers:

### CoreFX Framework (Reusable Library)

A production-grade .NET framework designed to be reused across multiple WebAPI projects. All CoreFX libraries **multi-target net8.0 and net10.0**.

| Module | What it does |
|--------|-------------|
| **CoreFX.Abstractions** | `SvcResponse<T>` structured response pattern, `FxObject` base class with auto-logging, `LazySingleton<T>`, `SdkRuntime` configuration, `DefaultJsonSerializer`, failback/circuit-breaker scoring, network utilities |
| **CoreFX.Common** | `SvcContext` initialization, string extensions (masking, hashing, parsing), file utilities, environment detection (`IsDevelopment()`, `IsProduction()`, etc.) |
| **CoreFX.Hosting** | Request/response logging middleware with correlation IDs, uses `RecyclableMemoryStream` for efficiency |
| **CoreFX.Logging.Log4net** | `ILogger` adapter for log4net — bridges `Microsoft.Extensions.Logging` to log4net seamlessly |
| **CoreFX.Caching.Redis** | `IDistributedCache` extensions with typed get/set (`GetAsync<T>`), circuit-breaker pattern via `FailbackScore`, auto-reconnect |

### Hello8 Sample App (Reference Implementation)

A demo WebAPI that showcases CoreFX in action:
- API versioning (v1/v2) with Swagger UI
- Health check endpoints (app, database, cache)
- Environment-based configuration (Debug/Development/Production)
- Docker multi-stage build + Minikube K8s deployment
- xUnit integration and unit tests

---

## Quick Start

### Prerequisites
- [.NET 8.0](https://dotnet.microsoft.com/download/dotnet/8.0) or [.NET 10.0](https://dotnet.microsoft.com/download/dotnet/10.0) SDK
- [Docker](https://www.docker.com/get-started) (optional)
- [minikube](https://minikube.sigs.k8s.io/docs/start/) (optional, for K8s deployment)

### Run locally (30 seconds)
```bash
cd src/Endpoint/Hello8
dotnet run
# Open http://localhost:5000/swagger
```

### Run with Docker Compose (1 minute)
```bash
docker-compose up
# App at http://localhost:8080/swagger, Redis at localhost:16379
```

### Deploy to Minikube (5 minutes)
```bash
# Build image
export VERSION=$(cat src/Endpoint/Hello8/.version | head -n1)
docker build . -t hello8-api:$VERSION -f Dockerfile

# Deploy
envsubst < minikube/deployment.yaml | kubectl apply -f -
kubectl apply -f minikube/service.yaml
minikube service hello8-api-dev-svc --url
```

---

## Architecture

```
+---------------------------------------------------------+
|  Hello8.Domain.Endpoint (net10.0)                       |
|  ASP.NET Core WebAPI + Swagger + HealthChecks           |
+---------------------------------------------------------+
        |               |                |
+-------v------+ +-----v-------+ +------v--------+
| Hello8.SDK   | | Hello8.DB   | | Hello8.Common |
| (net8/10)    | | Dapper+SQL  | | Domain models |
+--------------+ +-------------+ +---------------+
        |               |                |
+---------------------------------------------------------+
|  CoreFX Framework (net8.0 + net10.0)                    |
|  Abstractions | Common | Hosting | Log4net | Redis      |
+---------------------------------------------------------+
        |
+---------------------------------------------------------+
|  Infrastructure                                         |
|  Docker | Minikube K8s | Azure Pipelines | GitHub CI    |
+---------------------------------------------------------+
```

---

## CoreFX Features

### Structured Response Pattern
Every API response uses `SvcResponse<T>` for consistency:
```json
{
  "data": { ... },
  "code": 1,
  "msg": "Success",
  "msgId": "uuid",
  "isSuccess": true,
  "subCode": "",
  "subMsg": "",
  "extMap": {}
}
```
Fluent API: `response.SetData(obj).Success()` or `response.SetMsg("error").Error()`

### Request/Response Logging Middleware
```csharp
app.UseRequestResponseLogging(); // one line in Startup.cs
```
Logs every request/response with method, path, body, headers, correlation ID, and timing.

### Redis Caching with Circuit-Breaker
```csharp
var result = await cache.GetAsync<MyDto>("key");   // typed deserialization
await cache.SetAsync("key", myObj, TimeSpan.FromMinutes(5));
```
Built-in `FailbackScore` pattern: after N consecutive failures, cache is marked unavailable and auto-retries on a decay schedule.

### Multi-Framework Targeting
All library projects compile for both `net8.0` and `net10.0` with TFM-conditional NuGet packages, ensuring compatibility across .NET versions.

---

## Projects

**Hello8.Domain.Endpoint** is the primary project wrapping all dependent projects:

- **CoreFX** (Framework)
  - CoreFX.Abstractions
  - CoreFX.Common
  - CoreFX.Hosting
  - CoreFX.Logging.Log4net
  - CoreFX.Caching.Redis

- **Hello8** (Domain)
  - Hello8.Domain.Common
  - Hello8.Domain.Contract
  - Hello8.Domain.DataAccess.Database
  - Hello8.Domain.SDK

## Versioning
- Version File: `./src/Endpoint/Hello8/.version`
- Version Format: `#.#.#-###` (e.g., `3.0.0-100`)
- CHANGELOG: `./CHANGELOG.md`
- Git Tags: `hello8-api/v3.0.0-100`

---

## Testing

### Integration Test
```powershell
$env:ASPNETCORE_ENVIRONMENT = 'Development'
$env:CI_TEST_ENDPOINT = 'http://+:18731'

dotnet test tests/IntegrationTest/Hello8/IntegrationTest.Hello8.csproj -c Release --filter FullyQualifiedName=IntegrationTest.Hello8.CI_Test.Integration_Test
# Passed! Duration: less than 5s
```

### Unit Test (Redis)
```powershell
$env:ASPNETCORE_ENVIRONMENT = 'Debug'
$env:HELLO_REDIS_CACHE_CONN = '127.0.0.1:6379'

dotnet test tests/UnitTest/CoreFX/Caching/Redis/UnitTest.CoreFX.Caching.Redis.csproj -c Release
# Passed! Duration: less than 1m
```

---

## K8s/Minikube Deployment

### Docker Build
```dockerfile
FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS base
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
# Multi-stage build: restore -> publish -> final
```

### Kubernetes Manifests
- `minikube/deployment.yaml` — 2 replicas, rolling update
- `minikube/service.yaml` — ClusterIP service
- `minikube/ingress.yaml` — Ingress configuration

### Environment Variables
| Variable | Example | Required |
|----------|---------|----------|
| `ASPNETCORE_ENVIRONMENT` | `Development` | Yes |
| `IMAGE_HOST` | `docker.io/[ACCOUNT-ID]` | For deploy |
| `HELLO_HELLODB_CONN` | `Data Source=...` | For DB |
| `HELLO_REDIS_CACHE_CONN` | `127.0.0.1:6379` | For cache |

---

## Health Checks

```bash
BASE_URL=http://localhost:5000

curl $BASE_URL/health                    # Healthy
curl $BASE_URL/api/echo/ver | jq '.data' # "3.0.0"
curl $BASE_URL/api/echo/config           # Config status
curl $BASE_URL/api/echo/db              # Database connectivity
curl $BASE_URL/api/echo/cache           # Redis connectivity
curl $BASE_URL/api/echo/dump            # System info (version, env, IP, uptime)
```
