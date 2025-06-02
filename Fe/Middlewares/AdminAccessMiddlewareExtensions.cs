using Microsoft.AspNetCore.Builder;

namespace Fe.Middlewares
{
    public static class AdminAccessMiddlewareExtensions
    {
        public static IApplicationBuilder UseAdminAccessControl(this IApplicationBuilder builder)
        {
            return builder.UseMiddleware<AdminAccessMiddleware>();
        }
    }
}
