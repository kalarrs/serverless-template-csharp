using System;
using Kalarrs.Serverless.NetCore.Core;
using Kalarrs.Serverless.NetCore.Util;
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
            var serverlessProject = new ServerlessProject();

            var environmentVariables = serverlessProject.GetEnvironmentVariables();
            foreach (var keyValuePair in environmentVariables) Environment.SetEnvironmentVariable(keyValuePair.Key, keyValuePair.Value.ToString());                    
            
            var httpEvents = serverlessProject.GetHttpEvents();
            var port = serverlessProject.GetPort();
            routeBuilder.AddRoutes<T>(httpEvents, port);

            var routes = routeBuilder.Build();
            app.UseRouter(routes);
        }
    }
}