using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using ModelBaseAPI.Data;

namespace ModelBaseAPI.Utilities
{
    public class CustomHealthCheck(DataContext dataContext) : IHealthCheck
    {
        private readonly DataContext _dataContext = dataContext;

        public async Task<HealthCheckResult> CheckHealthAsync(HealthCheckContext context, CancellationToken cancellationToken = default)
        {
            var connection = _dataContext.Database.GetDbConnection();
            var command = connection.CreateCommand();
            try
            {
                await connection.OpenAsync(cancellationToken);
                command.CommandText = "SELECT 1";
                await command.ExecuteScalarAsync(cancellationToken);

                return HealthCheckResult.Healthy(
                    data: new Dictionary<string, object>
                    {
                        { "Mensagem", "Banco de dados conectado e funcional" }
                    }
                );
            }
            catch (Exception ex)
            {
                return HealthCheckResult.Unhealthy(exception: ex);
            }
            finally
            {
                await connection.CloseAsync();
            }
        }

        // Add Health Checks to external Services and API's 
    }
}
