using System;
using Kalarrs.Serverless.NetCore.Util;
using Kalarrs.Serverless.NetCore.Yaml;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Kalarrs.Sreverless.NetCore
{
    public class Startup<T> where T : new()
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public static IConfiguration Configuration { get; private set; }

        public void ConfigureServices(IServiceCollection services)
        {
            services.AddRouting();
        }

        public void Configure(IApplicationBuilder app, IHostingEnvironment env)
        {
            var routeBuilder = new RouteBuilder(app);

            var parser = new Parser();
            
            // TODO: parser.GetEnvironmentVariables();
            Environment.SetEnvironmentVariable("MONGODB_URI", "<uri>");
            
            var httpEvents = parser.GetHttpEvents();
            var port = parser.GetPort();
            routeBuilder.AddRoutes<T>(httpEvents, port);

            var routes = routeBuilder.Build();
            app.UseRouter(routes);
        }
    }
}