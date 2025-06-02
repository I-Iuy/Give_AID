using Fe.Middlewares;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllersWithViews();
builder.Services.AddHttpClient();
builder.Services.AddSession();

var app = builder.Build();

app.UseStatusCodePages(async context =>
{
    var response = context.HttpContext.Response;

    if (response.StatusCode == 401 || response.StatusCode == 403)
    {
        response.Redirect("/Web/Account/Login"); 
    }
});


app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseSession();
app.UseAdminAccessControl();
app.UseMiddleware<Fe.Middlewares.AdminAccessMiddleware>();
app.UseRouting();

app.UseAuthorization();

// Area-based routing
app.MapControllerRoute(
    name: "areas",
    pattern: "{area:exists}/{controller=Dashboard}/{action=Index}/{id?}");

// Default route 
app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}",
    defaults: new { area = "Web" });


app.Run();
