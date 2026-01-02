namespace Accounting.Shared.Models;

public class Result<T>
{
    public T? Data { get; set; }
    public bool IsSuccess { get; set; }
    public string? Message { get; set; }
    public List<string>? Errors { get; set; }

    public Result() { }

 
    public static Result<T> Success(T data, string? message = null)
    {
        return new Result<T> { IsSuccess = true, Data = data, Message = message };
    }

   
    public static Result<T> Failure(string message, List<string>? errors = null)
    {
        return new Result<T> { IsSuccess = false, Message = message, Errors = errors };
    }
}

public class Result
{
    public bool IsSuccess { get; set; }
    public string? Message { get; set; }
    public List<string>? Errors { get; set; }

    public static Result Success(string? message = null)
    {
        return new Result { IsSuccess = true, Message = message };
    }

    public static Result Failure(string message)
    {
        return new Result { IsSuccess = false, Message = message };
    }

    public static Result Failure(string message, List<string> validationErrors)
    {
        return new Result { IsSuccess = false, Message = message, Errors = validationErrors};
    }
}