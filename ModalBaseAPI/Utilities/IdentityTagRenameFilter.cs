using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace ModelBaseAPI.Utilities
{
    public class IdentityTagRenameFilter : IOperationFilter
    {
        public void Apply(OpenApiOperation operation, OperationFilterContext context)
        {
            var path = context.ApiDescription.RelativePath;

            if (path!.StartsWith("api/login") || path.StartsWith("api/register") || path.StartsWith("api/refresh") ||
                path.StartsWith("api/confirmEmail") || path.StartsWith("api/resendConfirmationEmail") || path.StartsWith("api/forgotPassword") ||
                path.StartsWith("api/resetPassword") || path.StartsWith("api/manage/2fa") || path.StartsWith("api/manage/info"))
            {
                operation.Tags.Clear();
                operation.Tags.Add(new OpenApiTag { Name = "Authentication" });
            }
        }
    }
}
