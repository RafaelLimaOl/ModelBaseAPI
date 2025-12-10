namespace ModelBaseAPI.Interfaces.Service
{
    public interface IRabbitMQService
    {
        Task SendMessageAsync(string message);
        Task<string?> ReceiveMessage();
    }
}
