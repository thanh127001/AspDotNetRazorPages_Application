namespace Application.Models;

public class AppResult<T>
{
    public bool Success { get; set; }
    public string Code { get; set; } = string.Empty;
    public List<string> Message { get; set; } = new();
    public T? Data { get; set; }

    public static AppResult<T> Ok(T data, params string[] messages)
    {
        return new AppResult<T>
        {
            Success = true,
            Code = "SUCCESS",
            Message = messages.ToList(),
            Data = data
        };
    }

    public static AppResult<T> Fail(string code, params string[] messages)
    {
        return new AppResult<T>
        {
            Success = false,
            Code = code,
            Message = messages.ToList(),
            Data = default
        };
    }
}
