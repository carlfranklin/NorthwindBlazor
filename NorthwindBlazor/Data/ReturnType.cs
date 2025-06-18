namespace NorthwindBlazor.Data;

public class ReturnType<T>
{
    public bool Success { get; set; }
    public T? Data { get; set; }
    public List<string> ErrorMessages { get; set; } = new List<string>();
}