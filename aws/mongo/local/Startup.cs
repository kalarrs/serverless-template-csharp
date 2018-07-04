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
        public static ServerlessProject ServerlessProject { get; set; }
        
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public static IConfiguration Configuration { get; private set; }

        public void ConfigureServices(IServiceCollection services)
        {
            if (ServerlessProject == null) throw new Exception("Startup Failed! Please create and store an instance of ServerlessProject on Startup<T>.ServerlessProject");
            services.AddSingleton(ServerlessProject);
            services.AddRouting();
        }

        public void Configure(IApplicationBuilder app, IHostingEnvironment env, ServerlessProject serverlessProject)
        {
            var routeBuilder = new RouteBuilder(app);
            routeBuilder.AddRoutes<T>(serverlessProject);

            var routes = routeBuilder.Build();
            app.UseRouter(routes);
        }
    }
}