namespace Application.Models;

public class AppResultModel<T>
{
    public bool Success { get; set; }
    public List<string> Messages { get; set; } = new();
    public T? Data { get; set; }

    public static AppResultModel<T> Ok(T data, params string[] messages)
    {
        return new AppResultModel<T>
        {
            Success = true,
            Messages = messages.Length > 0 ? messages.ToList() : new List<string> { "Operation completed successfully." },
            Data = data
        };
    }

    public static AppResultModel<T> Fail(params string[] messages)
    {
        return new AppResultModel<T>
        {
            Success = false,
            Messages = messages.Length > 0 ? messages.ToList() : new List<string> { "Operation failed." },
            Data = default
        };
    }
}
