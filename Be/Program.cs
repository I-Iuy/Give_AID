using Be.Models;
using Be.Repositories.Accounts;
using Be.Repositories.Campaigns;
using Be.Repositories.CampaignsUsage;
using Be.Repositories.ContentPages;
using Be.Repositories.Donations;
using Be.Repositories.Ngos;
using Be.Repositories.Partners;
using Be.Repositories.Purposes;
using Be.Services;
using Be.Services.Campaigns;
using Be.Services.CampaignUsage;
using Be.Services.ContentPages;
using Be.Services.Donations;
using Be.Services.Ngos;
using Be.Services.Partners;
using Be.Services.Purposes;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using System.Security.Claims;
using Be.Repositories.CommentRepo;
using Be.Repositories.NotificationRepo;
using Be.Repositories.ShareRepo;
using Be.Services.Comment;
using Be.Services.EmailService;
using Be.Services.NotificationService;
using Be.Services.ShareService;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
// Kết nối DB
builder.Services.AddDbContext<DatabaseContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("MyConnection")));

// Add CORS
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowFrontend", policy =>
    {
        policy.WithOrigins("https://localhost:7108")
              .AllowAnyMethod()
              .AllowAnyHeader()
              .AllowCredentials();
    });
});

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
//Đăng ký CampaignUsage
builder.Services.AddScoped<ICampaignUsageRepository, CampaignUsageRepository>();
builder.Services.AddScoped<ICampaignUsageService, CampaignUsageService>();
//Đăng ký Donation
builder.Services.AddScoped<IDonationRepository, DonationRepository>();
builder.Services.AddScoped<IDonationService, DonationService>();
//Đăng ký ContentPage
builder.Services.AddScoped<IContentPageRepository, ContentPageRepository>();
builder.Services.AddScoped<IContentPageService, ContentPageService>();
//Đăng ký JWT
// JWT token generator service
builder.Services.AddScoped<JwtService>();
// Account repository (business logic for accounts)
builder.Services.AddScoped<IAccountRepository, AccountRepository>();
// Email service for sending reset emails
builder.Services.AddScoped<EmailServices>();
// Register Repositories
builder.Services.AddScoped<ICommentRepository, CommentRepository>();
builder.Services.AddScoped<IShareRepository, ShareRepository>();
builder.Services.AddScoped<INotificationRepository, NotificationRepository>();
// Register Services
builder.Services.AddScoped<ICommentService, CommentService>();
builder.Services.AddScoped<IShareService, ShareService>();
builder.Services.AddScoped<INotificationService, NotificationService>();
// Register Email Service
builder.Services.AddScoped<IEmailService, EmailService>();
// Register Campaign Usage services
builder.Services.AddScoped<ICampaignUsageRepository, CampaignUsageRepository>();
builder.Services.AddScoped<ICampaignUsageService, CampaignUsageService>();


// ========================================
// Configure JWT Authentication middleware
// ========================================

builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(options =>
{
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuer = true, // Validate the token issuer
        ValidateAudience = true, // Validate the token audience
        ValidateLifetime = true, // Validate token expiration
        ValidateIssuerSigningKey = true, // Validate the signing key
        ValidIssuer = builder.Configuration["Jwt:Issuer"],
        ValidAudience = builder.Configuration["Jwt:Audience"],
        IssuerSigningKey = new SymmetricSecurityKey(
            Encoding.UTF8.GetBytes(builder.Configuration["Jwt:Key"]))
    };
    // Maps the Role claim type for role-based authorization
    options.TokenValidationParameters.RoleClaimType = ClaimTypes.Role;
});
var app = builder.Build();

// Add request logging middleware
app.Use(async (context, next) =>
{
    var logger = context.RequestServices.GetRequiredService<ILogger<Program>>();
    logger.LogInformation("Incoming request: {Method} {Path}", context.Request.Method, context.Request.Path);
    await next();
});

// ===========================
// Seed Initial ContentPages
// ===========================
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<DatabaseContext>();

    // If no ContentPages exist, seed default pages
    if (!db.ContentPages.Any())
    {
        db.ContentPages.AddRange(new[]
        {
            new ContentPage { Title = "What We Do", Slug = "what-we-do", Content = "", Author = "System", UpdatedAt = DateTime.Now },
            new ContentPage { Title = "Our Mission", Slug = "our-mission", Content = "", Author = "System", UpdatedAt = DateTime.Now },
            new ContentPage { Title = "Our Team", Slug = "our-team", Content = "", Author = "System", UpdatedAt = DateTime.Now },
            new ContentPage { Title = "Career With Us", Slug = "career-with-us", Content = "", Author = "System", UpdatedAt = DateTime.Now },
            new ContentPage { Title = "Our Achievements", Slug = "our-achievements", Content = "", Author = "System", UpdatedAt = DateTime.Now },
            new ContentPage { Title = "Our Supporters", Slug = "our-supporters", Content = "", Author = "System", UpdatedAt = DateTime.Now },
            new ContentPage { Title = "Read About Us", Slug = "read-about-us", Content = "", Author = "System", UpdatedAt = DateTime.Now }
        });

        db.SaveChanges(); // Persist seed data
    }
}
// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

// Enable CORS
app.UseCors("AllowFrontend");

// Enable authentication and authorization middleware
app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();
