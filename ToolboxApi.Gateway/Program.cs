using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.OpenApi.Models;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Ocelot.DependencyInjection;
using Ocelot.Middleware;
using Ocelot.Provider.Polly;

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
                services
                    .AddOcelot()
                    .AddPolly();
                //.AddAdministration("/administration", "secret");

                //services.AddOcelotConfigEditor();

                //services.AddSwaggerGen(c =>
                //{
                //    c.SwaggerDoc("v1", new OpenApiInfo { Title = "API Gateway", Version = "v1" });
                //});

                services.AddSwaggerForOcelot(Configuration);

            })
            .Configure(app =>
            {
                app
                .UseSwaggerForOcelotUI(Configuration, opt =>
                {
                    //opt.ReConfigureUpstreamSwaggerJson = (HttpContext context, string swaggerJson) =>
                    //{
                    //    var swagger = JObject.Parse(swaggerJson);
                    //    // ... alter upstream json
                    //    return swagger.ToString(Formatting.Indented);
                    //};
                })
                //.UseSwagger()
                //.UseOcelotConfigEditor() // use only with 2.2 for moment
                .UseOcelot()
                .Wait();
            });
    }

}
