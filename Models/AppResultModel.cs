namespace Application.Models;

public class AppResultModel<T>
{
    public bool Success { get; set; }
    public string Code { get; set; } = string.Empty;
    public List<string> Message { get; set; } = new();
    public T? Data { get; set; }

    public static AppResultModel<T> Ok(T data, params string[] messages)
    {
        return new AppResultModel<T>
        {
            Success = true,
            Code = "SUCCESS",
            Message = messages.ToList(),
            Data = data
        };
    }

    public static AppResultModel<T> Fail(string code, params string[] messages)
    {
        return new AppResultModel<T>
        {
            Success = false,
            Code = code,
            Message = messages.ToList(),
            Data = default
        };
    }
}
