using System.Security.Cryptography;
using Application.Models;

namespace Application.Common;

public static class AppErrorLog
{
    public static AppResultModel<T> Fail<T>(ILogger logger, Exception exception, string operation)
    {
        var now = DateTimeOffset.UtcNow;
        var errorId = $"{now:yyyyMMdd}-{now.ToUnixTimeMilliseconds()}-{RandomNumberGenerator.GetInt32(0, 10000):D4}";
        logger.LogError(exception, "{Operation} failed. Error ID: {ErrorId}", operation, errorId);
        return AppResultModel<T>.Fail($"An error occurred while processing the request. Error ID: {errorId}");
    }
}
