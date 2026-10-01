namespace NoP77svk.AspNetCore.BrowserAutoLaunch.Tests;

using Xunit;

[AttributeUsage(AttributeTargets.Method, AllowMultiple = false)]
internal sealed class BrowserSmokeFactAttribute : FactAttribute
{
    public BrowserSmokeFactAttribute()
    {
        if (!string.Equals(Environment.GetEnvironmentVariable("RUN_BROWSER_SMOKE_TEST"), "1", StringComparison.Ordinal))
        {
            Skip = "Set RUN_BROWSER_SMOKE_TEST=1 to launch a real browser.";
        }
    }
}
