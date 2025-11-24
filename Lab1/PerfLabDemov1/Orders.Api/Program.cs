using System.Net;
using System.Security.Authentication;
using Microsoft.EntityFrameworkCore;
using Orders.Api.Data;
using Orders.Api.Services;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// EF Core DbContext
builder.Services.AddDbContext<OrdersDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

// Demo services (non-circular by default)
builder.Services.AddScoped<IAService, AService>();
builder.Services.AddScoped<IBService, BService>();

// NOTE: To demonstrate a circular dependency issue during training,
// you can temporarily create a circular graph like this:
// 1. Modify BService to depend on IAService in its constructor.
// 2. Store it in a field and call back into IAService from DoWorkAsync().
// 3. Keep the registrations above as-is.
// ASP.NET Core DI will then throw an InvalidOperationException at startup.

// Optional: Kestrel + TLS demo config
builder.WebHost.ConfigureKestrel(options =>
{
    // HTTP 5000
    options.Listen(IPAddress.Loopback, 5000);

    // HTTPS 5001 with TLS 1.2/1.3
    options.Listen(IPAddress.Loopback, 5001, opt =>
    {
        opt.UseHttps(https =>
        {
            https.SslProtocols = SslProtocols.Tls12 | SslProtocols.Tls13;
        });
    });
});

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

// 👇 ADD THIS BLOCK BEFORE app.Run();
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<OrdersDbContext>();
    db.Database.EnsureCreated();   // creates PerfLabOrders schema (Orders table) if needed
}

app.Run();
