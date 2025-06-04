using Be.Models;
using Be.Repositories.Campaigns;
using Be.Repositories.CampaignsUsage;
using Be.Repositories.ContentPages;
using Be.Repositories.Donations;
using Be.Repositories.Ngos;
using Be.Repositories.Partners;
using Be.Repositories.Purposes;
using Be.Services.Campaigns;
using Be.Services.CampaignUsage;
using Be.Services.ContentPages;
using Be.Services.Donations;
using Be.Services.Ngos;
using Be.Services.Partners;
using Be.Services.Purposes;
using Microsoft.EntityFrameworkCore;

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
//Đăng ký CampaignUsage
builder.Services.AddScoped<ICampaignUsageRepository, CampaignUsageRepository>();
builder.Services.AddScoped<ICampaignUsageService, CampaignUsageService>();
//Đăng ký Donation
builder.Services.AddScoped<IDonationRepository, DonationRepository>();
builder.Services.AddScoped<IDonationService, DonationService>();
//Đăng ký ContentPage
builder.Services.AddScoped<IContentPageRepository, ContentPageRepository>();
builder.Services.AddScoped<IContentPageService, ContentPageService>();

var app = builder.Build();

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

app.UseAuthorization();

app.MapControllers();

app.Run();
