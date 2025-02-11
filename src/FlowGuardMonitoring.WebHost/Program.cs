using FlowGuardMonitoring.BLL;
using FlowGuardMonitoring.BLL.Contracts;
using FlowGuardMonitoring.BLL.Services;
using FlowGuardMonitoring.DAL.Data;
using FlowGuardMonitoring.DAL.Models;
using FlowGuardMonitoring.DAL.Repositories;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Identity;
using Resend;

namespace FlowGuardMonitoring.WebHost;

public class Program
{
    public static void Main(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);

        builder.Services.AddFlowGuardMonitoringContext(builder.Configuration.GetConnectionString("DefaultConnection"));
        builder.Services.AddHttpClient<ResendClient>();
        builder.Services.AddServices(builder.Configuration);
        builder.Services.AddHttpContextAccessor();
        builder.Services.AddScoped<ICurrentUser, CurrentUserService>();
        builder.Services.AddScoped<IRepository<Sensor>, SensorRepository>();
        builder.Services.AddScoped<IRepository<Measurement>, MeasurementRepository>();
        builder.Services.AddScoped<IRepository<Site>, SiteRepository>();
        builder.Services.AddScoped<IPaginationService<Site>, PaginationService<Site>>();
        builder.Services.AddScoped<IPaginationService<Sensor>, PaginationService<Sensor>>();
        builder.Services.AddScoped<IPaginationService<Measurement>, PaginationService<Measurement>>();
        builder.Services.AddScoped<StatsService>();

        builder.Services.AddHttpClient("MeasurementApi", client =>
        {
            client.BaseAddress = new Uri("http://localhost:5000");
            client.Timeout = TimeSpan.FromSeconds(30);
        });
        builder.Services.AddHostedService<FetchDataBackgroundService>();

        builder.Services.AddControllersWithViews();

        builder.Services.AddAuthentication()
            .AddCookie(CookieAuthenticationDefaults.AuthenticationScheme, o =>
            {
                o.LoginPath = "/login";
                o.LogoutPath = "/logout";
            });

        builder.Services.AddIdentity<User, IdentityRole>(options =>
            {
                options.SignIn.RequireConfirmedEmail = true;
                options.Password.RequiredLength = 8;
                options.Password.RequireDigit = true;
                options.Password.RequireLowercase = true;
                options.Password.RequireUppercase = true;
                options.Password.RequireNonAlphanumeric = true;
            })
            .AddEntityFrameworkStores<FlowGuardMonitoringContext>()
            .AddDefaultTokenProviders();

        var app = builder.Build();

        using (var scope = app.Services.CreateScope())
        {
            var dbContext = scope.ServiceProvider.GetRequiredService<FlowGuardMonitoringContext>();
            dbContext.Database.EnsureCreated();
        }

        if (!app.Environment.IsDevelopment())
        {
            app.UseExceptionHandler("/Home/Error");
            app.UseHsts();
        }

        app.UseHttpsRedirection();
        app.UseStaticFiles();

        app.UseRouting();

        app.UseAuthentication();
        app.UseAuthorization();

        app.MapControllerRoute(
            name: "default",
            pattern: "{controller=Home}/{action=Index}/{id?}");

        app.Run();
    }
}