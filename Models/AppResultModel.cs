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
            Messages = messages.ToList(),
            Data = data
        };
    }

    public static AppResultModel<T> Fail(params string[] messages)
    {
        return new AppResultModel<T>
        {
            Success = false,
            Messages = messages.ToList(),
            Data = default
        };
    }
}
