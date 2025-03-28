namespace chess.Response
{
    public class ApiResponse<T>(String message, T data)
    {
        public string Message { get; set; } = message;
        public T Data { get; set; } = data;
    }
}
