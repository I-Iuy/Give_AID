// ==============================
// AdminAccessMiddleware
// Restricts access to /admin routes unless the user has Admin role
// ==============================

using Microsoft.AspNetCore.Http;
using System.Threading.Tasks;

namespace Fe.Middlewares
{
    public class AdminAccessMiddleware
    {
        private readonly RequestDelegate _next;

        // Constructor to initialize the next middleware in the pipeline
        public AdminAccessMiddleware(RequestDelegate next)
        {
            _next = next;
        }

        // Middleware logic to check for admin access
        public async Task InvokeAsync(HttpContext context)
        {
            // Get the request path and convert to lowercase for comparison
            var path = context.Request.Path.ToString().ToLower();

            // Check if the path starts with /admin (admin area)
            if (path.StartsWith("/admin"))
            {
                // Retrieve the user's role from the session
                var role = context.Session.GetString("Role");

                // Redirect to login if user is not an admin
                if (string.IsNullOrEmpty(role) || role != "Admin")
                {
                    context.Response.Redirect("/Web/Account/Login");
                    return;
                }
            }

            // Call the next middleware in the pipeline
            await _next(context);
        }
    }
}
