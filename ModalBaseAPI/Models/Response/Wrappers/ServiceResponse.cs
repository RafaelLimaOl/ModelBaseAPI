namespace ModelBaseAPI.Models.Response.Wrappers
{
    public class ServiceResponse<T>
    {
        public bool Success { get; set; }
        public string? Message { get; set; }
        public T? Data { get; set; }

        public static ServiceResponse<T> Ok(T data) => new() { Success = true, Data = data };
        public static ServiceResponse<T> Ok(string message) => new() { Success = true, Message = message };
        public static ServiceResponse<T> Fail(string message) => new() { Success = false, Message = message };
    }
}
