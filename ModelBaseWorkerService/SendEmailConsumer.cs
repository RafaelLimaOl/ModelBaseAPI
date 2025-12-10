using MassTransit;
using Quartz;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ModelBaseWorkerService
{
    public record SendEmailCommand(string To, string Subject, string Body);

    public class SendEmailConsumer : IConsumer<SendEmailCommand>
    {
        private readonly IEmailService _emailService;

        public SendEmailConsumer(IEmailService emailService)
        {
            _emailService = emailService;
        }

        public async Task Consume(ConsumeContext<SendEmailCommand> context)
        {
            var cmd = context.Message;
            await _emailService.SendAsync(cmd.To, cmd.Subject, cmd.Body);
        }
    }

    public class DailyEmailJob : IJob
    {
        private readonly IPublishEndpoint _publish;

        public DailyEmailJob(IPublishEndpoint publish)
        {
            _publish = publish;
        }

        public async Task Execute(IJobContext context)
        {
            // você consegue usar o Quartz pra inserir mensagens na fila
            await _publish.Publish(new SendEmailCommand("foo@example.com", "Relatório", "Seu relatório diário."));
        }
    }

}
