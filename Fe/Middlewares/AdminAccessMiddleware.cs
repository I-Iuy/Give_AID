using Microsoft.AspNetCore.Http;
using System.Threading.Tasks;

namespace Fe.Middlewares
{
    public class AdminAccessMiddleware
    {
        private readonly RequestDelegate _next;

        public AdminAccessMiddleware(RequestDelegate next)
        {
            _next = next;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            var path = context.Request.Path.ToString().ToLower();

            if (path.StartsWith("/admin"))
            {
                var role = context.Session.GetString("Role");
                if (string.IsNullOrEmpty(role) || role != "Admin")
                {
                    context.Response.Redirect("/Web/Account/Login");
                    return;
                }
            }

            await _next(context);
        }
    }
}
