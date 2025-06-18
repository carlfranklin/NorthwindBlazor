using System.Collections.Generic;

namespace NorthwindBlazor.Data;

public class ReturnType<T>
{
    public bool Success { get; set; }
    public T? Data { get; set; }
    public List<string> ErrorMessages { get; set; } = new List<string>();

    public ReturnType()
    {
    }

    public ReturnType(bool success, T? data, List<string>? errorMessages = null)
    {
        Success = success;
        Data = data;
        ErrorMessages = errorMessages ?? new List<string>();
    }
}