using System.Security.Cryptography;

namespace Application.Common;

public static class AppErrorLog
{
    public static string Log(ILogger logger, Exception exception, string operation)
    {
        var now = DateTimeOffset.UtcNow;
        var errorId = $"{now:yyyyMMdd}-{now:HHmmss}-{RandomNumberGenerator.GetInt32(0, 10000):D4}";
        logger.LogError(exception, "{Operation} failed. Error ID: {ErrorId}", operation, errorId);
        return errorId;
    }
}
