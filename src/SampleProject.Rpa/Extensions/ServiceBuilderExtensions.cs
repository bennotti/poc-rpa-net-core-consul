using Consul;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Localization;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.IdentityModel.Logging;
using SampleProject.Core.Config;
using SampleProject.Infra.HostedService;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;

namespace MeuAtelie.Rpa.Extensions
{
    public static class ServiceBuilderExtensions
    {
        public static IServiceCollection AddRpaInfra(this IServiceCollection services, IConfiguration configuration)
        {
            IdentityModelEventSource.ShowPII = true;

            services.AddSingleton(p => configuration);

            services.AddHostedService<Worker2>();
            services.AddHostedService<Worker1>();

            services.AddSingleton<IHostedService, ConsulRpaHostedService>();

            services.Configure<ConsulConfig>(configuration.GetSection("ConsulSettings"));
            services.AddSingleton<IConsulClient, ConsulClient>(p => new ConsulClient(consulConfig =>
            {
                var address = configuration["ConsulSettings:address"];
                consulConfig.Address = new Uri(address);
            }));

            services.AddMemoryCache();

            services.Configure<RequestLocalizationOptions>(opts => {
                var supportedCultures = new[] { new CultureInfo("pt-BR") };

                opts.DefaultRequestCulture = new RequestCulture("pt-BR");
                opts.SupportedCultures = supportedCultures;
                opts.SupportedUICultures = supportedCultures;
            });


            return services;
        }
    }
}
