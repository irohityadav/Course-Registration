# Setup and Run Instructions

## Prerequisites
* [.NET 8 SDK](https://dotnet.microsoft.com/download/dotnet/8.0) or later.
* Verify installation in your terminal:
  ```bash
  dotnet --version
  ```

## Getting Started

1. **Clone the Repository**
   ```bash
   git clone <your-github-repo-url>
   cd CourseRegistration
   ```

2. **Restore Dependencies & Build the Solution:**
   ```bash
   dotnet restore
   dotnet build --configuration Release
   ```

## Executing Automated Tests

Run the full xUnit test suite from the solution root directory:

```bash
dotnet test --logger "console;verbosity=normal"
```

### Additional Test Commands
* **Run tests without rebuilding (after a prior build):**
  ```bash
  dotnet test --no-build --logger "console;verbosity=normal"
  ```
* **Run a specific test case by name (e.g., concurrency stress test):**
  ```bash
  dotnet test --filter "FullyQualifiedName~Concurrent_Enrollments_Respect_Capacity_Using_Barrier"
  ```