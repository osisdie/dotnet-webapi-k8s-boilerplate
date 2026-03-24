# Contributing to dotnet-webapi-k8s-boilerplate

Thank you for your interest in contributing! Here's how to get started.

## Prerequisites

- [.NET 10.0 SDK](https://dotnet.microsoft.com/download/dotnet/10.0)
- [Docker](https://www.docker.com/get-started)
- [minikube](https://minikube.sigs.k8s.io/docs/start/) (optional, for K8s testing)
- A Redis instance (optional, for cache testing)

## Getting Started

1. Fork the repository
2. Clone your fork: `git clone https://github.com/<your-username>/dotnet-webapi-k8s-boilerplate.git`
3. Create a branch: `git checkout -b feature/my-feature`
4. Restore and build: `dotnet build`
5. Run tests: `dotnet test`

## Code Style

This project uses `.editorconfig` for consistent code formatting. Please ensure your editor respects it.

## Running Locally

```bash
# Via dotnet CLI
cd src/Endpoint/Hello8
dotnet run

# Via Docker Compose
docker-compose up
```

## Submitting Changes

1. Ensure all tests pass: `dotnet test -c Release`
2. Update `CHANGELOG.md` if applicable
3. Commit with a descriptive message following [Conventional Commits](https://www.conventionalcommits.org/)
4. Push to your fork and open a Pull Request

## Reporting Issues

- Use the [Bug Report](.github/ISSUE_TEMPLATE/bug_report.md) template for bugs
- Use the [Feature Request](.github/ISSUE_TEMPLATE/feature_request.md) template for new ideas

## License

By contributing, you agree that your contributions will be licensed under the [MIT License](LICENSE).
