using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Orders.Api.Data;
using Orders.Api.Models;
using Orders.Api.Services;

namespace Orders.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class OrdersController : ControllerBase
{
    private readonly OrdersDbContext _db;
    private readonly IAService _aService;

    public OrdersController(OrdersDbContext db, IAService aService)
    {
        _db = db;
        _aService = aService;
    }

    // Naive bulk insert - intentionally inefficient for demo
    [HttpPost("bulk-naive")]
    public async Task<IActionResult> BulkNaive(int count = 1000)
    {
        for (int i = 0; i < count; i++)
        {
            _db.Orders.Add(new Order
            {
                Customer = $"Customer-{i}",
                Amount = i,
                CreatedAt = DateTime.UtcNow
            });

            // NAIVE: saving per iteration (many round-trips)
            await _db.SaveChangesAsync();
        }

        return Ok(new { Inserted = count, Mode = "Naive" });
    }

    // Optimized bulk insert - fewer DB round-trips
    [HttpPost("bulk-optimized")]
    public async Task<IActionResult> BulkOptimized(int count = 1000)
    {
        var orders = new List<Order>(capacity: count);

        for (int i = 0; i < count; i++)
        {
            orders.Add(new Order
            {
                Customer = $"Customer-{i}",
                Amount = i,
                CreatedAt = DateTime.UtcNow
            });
        }

        await _db.Orders.AddRangeAsync(orders);
        await _db.SaveChangesAsync();

        return Ok(new { Inserted = count, Mode = "Optimized" });
    }

    [HttpGet("count")]
    public async Task<IActionResult> GetCount()
    {
        var total = await _db.Orders.CountAsync();
        return Ok(new { Total = total });
    }

    [HttpGet("ping-a-service")]
    public async Task<IActionResult> PingAService()
    {
        await _aService.DoWorkAsync();
        return Ok(new { Message = "AService executed" });
    }
}
