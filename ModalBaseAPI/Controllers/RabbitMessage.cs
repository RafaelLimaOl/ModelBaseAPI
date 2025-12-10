using Microsoft.AspNetCore.Mvc;
using ModelBaseAPI.Services;
using ModelBaseAPI.Utilities;
using Polly.Registry;

namespace ModelBaseAPI.Controllers
{
    [ApiController]
    [Route("api/rabbit-messages")]
    public class MessagesController(RabbitMQService rabbitMQService, ResiliencePipelineProvider<string> pipeline) : ControllerBase
    {
        private readonly RabbitMQService _rabbitMQService = rabbitMQService;
        private readonly ResiliencePipelineProvider<string> _pipeline = pipeline;

        /// <summary>
        /// Sends a message to the RabbitMQ queue.
        /// </summary>
        /// <param name="message">The message to be sent.</param>
        /// <response code="200">Message sent successfully!</response>
        /// <response code="400">Error sending the message.</response>
        [HttpPost]
        public async Task<IActionResult> SendMessageAsync([FromBody] string message)
        {
 
            var pipelineProvider = _pipeline.GetPipeline("default");
            try
            {
                await pipelineProvider.ExecuteAsync(async ct => await _rabbitMQService.SendMessageAsync(message));
            }
            catch (Exception ex)
            {
                throw new ProblemExeption("Bad Request", string.Concat("Errors." + ex.Message), StatusCodes.Status400BadRequest);
            }
            return Ok(new { Message = "Message sent successfully!" });
        }

        /// <summary>
        /// Receive a message from the RabbitMQ queue.
        /// </summary>
        /// <response code="200">Message sent successfully!</response>
        /// <response code="204">No message available in queue.</response>
        [HttpGet]
        public IActionResult ReceiveMessage()
        {
            var message = _rabbitMQService.ReceiveMessageAsync() ?? throw new ProblemExeption("No Data", string.Concat("No message available in queue."), StatusCodes.Status204NoContent);
            return Ok(new { Message = message });
        }
    }
}
