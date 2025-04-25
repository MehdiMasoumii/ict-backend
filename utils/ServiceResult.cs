namespace API.Utils;

public class APIResult<T>(string Message, T? Data)
{
    public string Message { get; set; } = Message;
    public T Data { get; set; } = Data!;
}

public class ServiceResult<T>(bool IsSuccess, string Message, T? Data) : APIResult<T>(Message, Data)
{
    public bool IsSuccess { get; set; } = IsSuccess;

    public static ServiceResult<T> Success(string Message, T Data)
    {
        return new ServiceResult<T>(true, Message, Data);
    }
    public static ServiceResult<T> Failure(string Message, T Data)
    {
        return new ServiceResult<T>(false, Message, Data);
    }
    public static ServiceResult<T> Failure(string Message)
    {
        return new ServiceResult<T>(false, Message, default);
    }
}