using Serilog;

namespace DupeBuster.Tests;

public class TestBase
{
    static TestBase()
    {
        Log.Logger = new LoggerConfiguration()
            .WriteTo.Debug()
            .MinimumLevel.Verbose()
            .CreateLogger();
    }
}
