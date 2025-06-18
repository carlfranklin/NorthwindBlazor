using System.Collections.Generic;

namespace NorthwindBlazor.Data;

public class ReturnListType<T>
{
    public bool Success { get; set; }
    public List<T> Data { get; set; } = new List<T>();
    public List<string> ErrorMessages { get; set; } = new List<string>();

    public ReturnListType()
    {
    }

    public ReturnListType(bool success, List<T>? data = null, List<string>? errorMessages = null)
    {
        Success = success;
        Data = data ?? new List<T>();
        ErrorMessages = errorMessages ?? new List<string>();
    }
}