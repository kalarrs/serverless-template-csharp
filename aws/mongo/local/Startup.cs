using System;
using Kalarrs.Serverless.NetCore.Core;
using Kalarrs.Serverless.NetCore.Util;
using mongo.Local;
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
            services.AddSingleton(LocalEntryPoint.ServerlessProject);
            services.AddRouting();
        }

        public void Configure(IApplicationBuilder app, IHostingEnvironment env, ServerlessProject serverlessProject)
        {
            var routeBuilder = new RouteBuilder(app);
            
            var httpEvents = serverlessProject.GetHttpEvents();
            var port = serverlessProject.Port;
            routeBuilder.AddRoutes<T>(serverlessProject.DefaultEnvironmentVariables, serverlessProject.EnvironmentVariables, httpEvents, port);

            var routes = routeBuilder.Build();
            app.UseRouter(routes);
        }
    }
}