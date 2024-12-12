using FlowGuardMonitoring.BLL;
using FlowGuardMonitoring.DAL.Data;
using Microsoft.AspNetCore.Authentication.Cookies;

namespace FlowGuardMonitoring.WebHost;

public class Program
{
    public static void Main(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);

        builder.Services.AddFlowGuardMonitoringContext(builder.Configuration.GetConnectionString("DefaultConnection"));
        builder.Services.AddServices(builder.Configuration);

        builder.Services.AddControllersWithViews();

        builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
            .AddCookie(CookieAuthenticationDefaults.AuthenticationScheme, o =>
            {
                o.LoginPath = "/login";
                o.LogoutPath = "/logout";
            });

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