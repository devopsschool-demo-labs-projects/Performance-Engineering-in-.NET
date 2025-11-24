# PerfLab Demo – ASP.NET Core Performance Lab

This solution is a minimal demo to support a performance engineering workshop.

It includes:
- ASP.NET Core Web API (`Orders.Api`)
- EF Core with SQL Server
- Naive vs optimized bulk insert endpoints
- Example services for circular dependency discussion
- TLS configuration example
- Dockerfile for containerization
- Kubernetes deployment YAML

## 1. Prerequisites

- .NET 8 SDK
- SQL Server (LocalDB or SQL Express)
- Visual Studio 2022 (recommended) or VS Code + C# extensions
- Docker Desktop (optional, for container demo)
- kubectl + local cluster (kind/minikube/Docker Desktop) for k8s demo (optional)

## 2. Restore & Run (Local)

1. From the `PerfLabDemo` folder:
   ```bash
   dotnet new sln -n PerfLab
   dotnet sln add Orders.Api/Orders.Api.csproj
   ```

2. Update `Orders.Api/appsettings.json` if needed to point to your local SQL Server.

3. Run the API:
   ```bash
   cd Orders.Api
   dotnet run
   ```

# For Errorrs
   ```bash
Go to:
C:\Users\rajes\OneDrive\Desktop\TRAINING-PERF\PerfLabDemo\

Then run:
dotnet restore


If You Created a New Solution, Do This:

If you used:
dotnet new sln -n PerfLab
dotnet sln add Orders.Api/Orders.Api.csproj

Then run:
dotnet restore PerfLab.sln

Both commands will restore missing dependencies.


Option B – Trigger It Automatically:

Build → Clean Solution
Build → Rebuild Solution

Visual Studio will restore missing NuGet packages before build.


✔️ After Restore, Build the Project:
dotnet build


1. Install the Swagger package

From a terminal in:
C:\Users\rajes\OneDrive\Desktop\TRAINING-PERF\PerfLabDemo\Orders.Api

run:
dotnet add package Swashbuckle.AspNetCore
dotnet restore

This adds the Swashbuckle.AspNetCore package which contains:
AddSwaggerGen
UseSwagger
UseSwaggerUI

   ```
   
4. Browse Swagger:
   - https://localhost:5001/swagger

## 3. Endpoints for the Demo

- Naive bulk insert (many SaveChanges):
  ```
  POST https://localhost:5001/api/orders/bulk-naive?count=1000
  ```

- Optimized bulk insert (single SaveChanges):
  ```
  POST https://localhost:5001/api/orders/bulk-optimized?count=1000
  ```

- Check total orders:
  ```
  GET https://localhost:5001/api/orders/count
  ```

- Call AService (for circular dependency discussion):
  ```
  GET https://localhost:5001/api/orders/ping-a-service
  ```

## 4. Circular Dependency Demo

By default, `BService` does not depend on `IAService`, so there is no circular dependency.

To demonstrate a circular dependency:

1. Modify `BService` constructor to take `IAService` and store it in a field.
2. Call back into `IAService` from `BService.DoWorkAsync()`.
3. Keep registrations in `Program.cs` as:
   ```csharp
   builder.Services.AddScoped<IAService, AService>();
   builder.Services.AddScoped<IBService, BService>();
   ```
4. Run the app and observe the DI error on startup.

Discuss why this is a design smell and how it can impact maintainability and indirect performance.

## 5. TLS Configuration Demo

`Program.cs` configures Kestrel to listen on HTTPS with TLS 1.2 and 1.3:

```csharp
builder.WebHost.ConfigureKestrel(options =>
{
    options.Listen(IPAddress.Loopback, 5001, opt =>
    {
        opt.UseHttps(https =>
        {
            https.SslProtocols = SslProtocols.Tls12 | SslProtocols.Tls13;
        });
    });
});
```

You can explain handshake cost, CPU overhead, and why TLS 1.3 is generally preferred for performance and security.

## 6. Profiling with Visual Studio Diagnostic Tools

1. Open the solution in Visual Studio.
2. Set `Orders.Api` as startup project.
3. Start debugging (F5).
4. Open **Debug > Windows > Show Diagnostic Tools**.
5. Trigger load from a terminal using `curl`/`hey`/`wrk` against `bulk-naive` and `bulk-optimized`.
6. Compare CPU, memory, and GC events between the two endpoints.

## 7. Docker Demo

From `Orders.Api`:

```bash
docker build -t orders-api:perf .
docker run -d -p 8080:8080 --name orders-api orders-api:perf
```

Then hit:

```bash
curl http://localhost:8080/api/orders/bulk-optimized?count=1000
```

Use `docker stats` to observe container CPU/memory usage.

## 8. Kubernetes Demo (Optional)

From `PerfLabDemo` root:

```bash
kubectl apply -f k8s-deployment.yaml
```

Then hit (NodePort example):

```bash
curl http://localhost:30080/api/orders/bulk-optimized?count=1000
```

Scale replicas and observe throughput/latency under load.

---

Use this repo as a teaching aid to show how .NET performance is **measured, profiled, and improved** in a realistic but minimal scenario.
