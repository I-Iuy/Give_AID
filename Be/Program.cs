using Be.Models;
using Be.Repositories.Campaigns;
using Be.Repositories.Ngos;
using Be.Repositories.Partners;
using Be.Repositories.Purposes;
using Be.Services.Campaigns;
using Be.Services.Ngos;
using Be.Services.Partners;
using Be.Repositories.CommentRepo;
using Be.Repositories.NotificationRepo;
using Be.Repositories.Purposes;
using Be.Repositories.ShareRepo;
using Be.Services.Comment;
using Be.Services.EmailService;

using Be.Services.Purposes;
using Microsoft.EntityFrameworkCore;
using Be.Services.NotificationService;
using Be.Services.ShareService;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
// Kết nối DB
builder.Services.AddDbContext<DatabaseContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("MyConnection")));
builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
//Đăng ký Purpose
builder.Services.AddScoped<IPurposeRepository, PurposeRepository>();
builder.Services.AddScoped<IPurposeService, PurposeService>();
//Đăng ký Partner
builder.Services.AddScoped<IPartnerRepository, PartnerRepository>();
builder.Services.AddScoped<IPartnerService, PartnerService>();
//Đăng ký NGO
builder.Services.AddScoped<INgoRepository, NgoRepository>();
builder.Services.AddScoped<INgoService, NgoService>();
//Đăng ký Campaign
builder.Services.AddScoped<ICampaignRepository, CampaignRepository>();
builder.Services.AddScoped<ICampaignService, CampaignService>();

//  ĐĂNG KÝ REPOSITORIES

builder.Services.AddScoped<IPurposeRepository, PurposeRepository>();
builder.Services.AddScoped<ICommentRepository, CommentRepository>();
builder.Services.AddScoped<IShareRepository, ShareRepository>();
builder.Services.AddScoped<INotificationRepository, NotificationRepository>();

//ĐĂNG KÝ SERVICES
builder.Services.AddScoped<IPurposeService, PurposeService>();
builder.Services.AddScoped<ICommentService, CommentService>();
builder.Services.AddScoped<IShareService, ShareService>();
builder.Services.AddScoped<INotificationService, NotificationService>();

//EMAIL SERVICE
builder.Services.AddScoped<IEmailService, EmailService>();
var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();
