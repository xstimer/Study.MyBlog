namespace Study.Net.Utility;

public static class ApiResultHelper
{
    public static ApiResult Success(dynamic data)
    {
        return new ApiResult
        {
            code = 200,
            Data = data,
            message = "成功",
            Total = 0
        };
    }

    public static ApiResult Error(string msg)
    {
        return new ApiResult
        {
            code = 200,
            Data = null,
            message = msg,
            Total = 0
        };
    }
}
