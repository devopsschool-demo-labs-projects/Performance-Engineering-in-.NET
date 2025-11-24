// NOTE: This is implemented WITHOUT a circular dependency by default.
// To demonstrate circular dependency issues live:
// 1. Add IAService to this constructor.
// 2. Store it in a field and call back into IAService from DoWorkAsync().
// 3. Run the app and observe the DI error on startup.

namespace Orders.Api.Services;

public class BService : IBService
{
    public BService()
    {
    }

    public async Task DoWorkAsync()
    {
        // Simulate some lightweight async work
        await Task.Delay(10);
    }
}
