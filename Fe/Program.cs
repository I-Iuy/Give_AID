
﻿using Fe.Services.Ngos;
using Fe.Services.Partners;
using Fe.Services.Purposes;
using Fe.Services.Comment;


var builder = WebApplication.CreateBuilder(args);

// Add services to the container (Razor View + MVC)
builder.Services.AddControllersWithViews();

// ✅ Đăng ký CommentService dùng HttpClient và cấu hình base URL từ appsettings.json
builder.Services.AddHttpClient<ICommentService, CommentService>(client =>
{
    client.BaseAddress = new Uri(builder.Configuration["ApiSettings:BaseUrl"]);
});
builder.Services.AddScoped<Fe.Services.Share.IShareService, Fe.Services.Share.ShareService>();
builder.Services.AddScoped<Fe.Services.Notification.INotificationService, Fe.Services.Notification.NotificationService>();

// ✅ Đăng ký PurposeService (nếu có)
builder.Services.AddScoped<IPurposeApiService, PurposeApiService>();
// Đăng ký Partner
builder.Services.AddScoped<IPartnerApiService, PartnerApiService>();
// Đăng ký Ngo
builder.Services.AddScoped<INgoApiService, NgoApiService>();

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
