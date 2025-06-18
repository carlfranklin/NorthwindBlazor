namespace NorthwindBlazor.Data;

public class ReturnListType<T>
{
    public bool Success { get; set; }
    public List<T> Data { get; set; } = new List<T>();
    public List<string> ErrorMessages { get; set; } = new List<string>();
}