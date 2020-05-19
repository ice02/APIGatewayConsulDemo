namespace Common
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using Consul;
    using Microsoft.AspNetCore.Builder;
    using Microsoft.AspNetCore.Hosting;
    using Microsoft.AspNetCore.Hosting.Server.Features;
    using Microsoft.AspNetCore.Http.Features;
    using Microsoft.Extensions.Configuration;
    using Microsoft.Extensions.DependencyInjection;
    using Microsoft.Extensions.Logging;

    public static class AppExtensions
    {           
        public static IServiceCollection AddConsulConfig(this IServiceCollection services, IConfiguration configuration)
        {
            services.AddSingleton<IConsulClient, ConsulClient>(p => new ConsulClient(consulConfig =>
            {
                var address = configuration.GetValue<string>("Consul:Host");
                consulConfig.Address = new Uri(address);
            }));
            return services;
        }

        public static IApplicationBuilder UseConsul(this IApplicationBuilder app, IConfiguration configuration)
        {
            var consulClient = app.ApplicationServices.GetRequiredService<IConsulClient>();
            var logger = app.ApplicationServices.GetRequiredService<ILoggerFactory>().CreateLogger("AppExtensions");
            var lifetime = app.ApplicationServices.GetRequiredService<IApplicationLifetime>();

            // TODO : Compatibility with httpsys/iis - now work only for kestrel
            if (!(app.Properties["server.Features"] is FeatureCollection features)) return app;

            var addresses = features.Get<IServerAddressesFeature>();
            var address = addresses.Addresses.First();

            logger.LogDebug($"address={address}");

            var uri = new Uri(address);
            var serviceName = configuration.GetValue<string>("Service:Name");
            var metas = new Dictionary<string, string>();
            metas.Add("Controller", "Apps"); // TODO : Replace by controller kind
            var registration = new AgentServiceRegistration()
            {
                ID = $"{serviceName}-{ uri.Host }-{uri.Port}",
                Name = serviceName,
                Address = $"{uri.Host}",
                Port = uri.Port,
                Meta = metas,
                Tags = new string[] { "version-1", "version-2" } // TODO : replace by versions managed by this host, based on version discovered
            };

            logger.LogInformation($"Registering with Consul with ID = { serviceName}-{ uri.Host }-{ uri.Port}, Name = {serviceName}, Address = {uri.Host}, Port = {uri.Port}");
            consulClient.Agent.ServiceDeregister(registration.ID).ConfigureAwait(true);
            consulClient.Agent.ServiceRegister(registration).ConfigureAwait(true);

            lifetime.ApplicationStopping.Register(() =>
            {
                logger.LogInformation("Unregistering from Consul");
                consulClient.Agent.ServiceDeregister(registration.ID).ConfigureAwait(true);
            });

            return app;
        }
    }
}
