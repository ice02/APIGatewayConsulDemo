using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Ocelot.Administration;
using Ocelot.DependencyInjection;
using Ocelot.Middleware;
using Ocelot.Provider.Eureka;
using Ocelot.Provider.Polly;
using System.Collections.Generic;

namespace ToolboxApi.Gateway
{
    public class Program
    {
        public static void Main(string[] args)
        {
            CreateWebHostBuilder(args).Build().Run();
        }

        public static IConfiguration Configuration { get; set; }

        public static IWebHostBuilder CreateWebHostBuilder(string[] args) =>
            WebHost.CreateDefaultBuilder(args)
             .UseUrls("http://*:9000")
             .ConfigureAppConfiguration((hostingContext, config) =>
             {
                 config
                     .SetBasePath(hostingContext.HostingEnvironment.ContentRootPath)
                     .AddJsonFile("ocelot.json")
                     .AddEnvironmentVariables();
                 Configuration = config.Build();
             })
            .ConfigureServices(services =>
            {
                services.AddOcelot()
                    .AddPolly()
                    .AddEureka();
                //.AddAdministration("/administration", "secret");
                services.AddSwaggerForOcelot(Configuration);
            })
            .Configure(app =>
            {
                app
                .UseSwaggerForOcelotUI(Configuration, opt =>
                {
                    opt.DownstreamSwaggerHeaders = new[]
                    {
                        new KeyValuePair<string, string>("Key", "Value"),
                        new KeyValuePair<string, string>("Key2", "Value2"),
                    };
                })
                .UseOcelot()
                .Wait();
            });
    }
}
