namespace NoP77svk.AspNetBrowserAutoLaunch;

public class BrowserAutoLaunchException : Exception
{
    public BrowserAutoLaunchException()
    {
    }

    public BrowserAutoLaunchException(string message)
        : base(message)
    {
    }

    public BrowserAutoLaunchException(string message, Exception inner)
        : base(message, inner)
    {
    }
}
