using Fe.Services.Campaigns;
using Fe.Services.Getdata;
using Fe.Services.Ngos;
using Fe.Services.Partners;
using Fe.Services.Purposes;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container (Razor View + MVC)
builder.Services.AddControllersWithViews();

// Add HttpClient cấu hình để gọi API (dùng base URL từ appsettings.json)
builder.Services.AddHttpClient("API", client =>
{
    client.BaseAddress = new Uri(builder.Configuration["ApiSettings:BaseUrl"]);
});
// Đăng ký Purpose
builder.Services.AddScoped<IPurposeApiService, PurposeApiService>();
// Đăng ký Partner
builder.Services.AddScoped<IPartnerApiService, PartnerApiService>();
// Đăng ký Ngo
builder.Services.AddScoped<INgoApiService, NgoApiService>();
// Đăng ký Campaign
builder.Services.AddScoped<ICampaignApiService, CampaignApiService>();
// Đăng ký Getdata
builder.Services.AddScoped<IGetdataApiService, GetdataApiService>();
var app = builder.Build();
// Middleware xử lý HTTPS và file tĩnh (CSS, JS, ảnh, ...)
app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();
app.UseAuthorization();

// Routing cho khu vực có Area (ví dụ: /Admin/Purpose/Create)
app.MapControllerRoute(
    name: "areas",
    pattern: "{area:exists}/{controller=Dashboard}/{action=Index}/{id?}");

// Routing mặc định (không Area → về Web)
app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}",
    defaults: new { area = "Web" });

app.Run();
