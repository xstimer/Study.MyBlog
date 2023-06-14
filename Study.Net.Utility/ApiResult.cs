namespace Study.Net.Utility;

public record ApiResult
{
    public int code { get; set; }
    public string? message { get; set; }
    public int Total { get; set; }
    public dynamic? Data { get; set; }
}