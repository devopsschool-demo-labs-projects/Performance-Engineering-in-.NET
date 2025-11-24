namespace Orders.Api.Services;

public class AService : IAService
{
    private readonly IBService _b;

    public AService(IBService b)
    {
        _b = b;
    }

    public async Task DoWorkAsync()
    {
        // Simulate some lightweight async work
        await Task.Delay(10);
        await _b.DoWorkAsync();
    }
}
