namespace ClearLogicalDirectoriesFIleSystem.Extensions;

public static class ILoggerExtensions
{
    public static void ThrowIfNull(this ILogger logger, string paramName)
    {
        if (logger == null)
        {
            throw new ArgumentNullException(paramName);
        }
    }
}