namespace Mango.Contracts.Responses;

public record ApiResponse<T>(
    bool IsSuccess,
    T? Result,
    int StatusCode,
    string Message
)
{
    public static ApiResponse<T> Success(T result, int statusCode = 200, string message = "")
        => new(true, result, statusCode, message);

    public static ApiResponse<T> Fail(string message, int statusCode = 400)
        => new(false, default, statusCode, message);
}

public record ApiResponse(
    bool IsSuccess,
    int StatusCode,
    string Message
)
{
    public static ApiResponse Success(int statusCode = 200, string message = "")
        => new(true, statusCode, message);

    public static ApiResponse Fail(string message, int statusCode = 400)
        => new(false, statusCode, message);
}
