using FlowGuardMonitoring.BLL.Options;
using FlowGuardMonitoring.BLL.Services;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Resend;

namespace FlowGuardMonitoring.BLL;

public static class DependencyInjection
{
    public static IServiceCollection AddServices(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        services.Configure<EmailOptions>(configuration.GetSection("Email"));
        services.AddTransient<EmailSenderService>();

        return services;
    }
}