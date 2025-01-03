namespace FlowGuardMonitoring.BLL;

using System;
using FlowGuardMonitoring.BLL.Options;
using FlowGuardMonitoring.BLL.Services;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Resend;

public static class DependencyInjection
{
    public static IServiceCollection AddServices(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        services.Configure<EmailOptions>(configuration.GetSection("Email"));
        services.AddTransient<EmailSenderService>();
        services.Configure<ResendClientOptions>(o =>
        {
            o.ApiToken = configuration.GetValue<string>("Email:ApiKey")!;
        });
        services.AddTransient<IResend, ResendClient>();
        return services;
    }
}