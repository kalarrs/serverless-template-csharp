using Kalarrs.Serverless.NetCore.Util;
using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Logging;

namespace mongo.Local
{
    public static class LocalEntryPoint
    {
        public static void Main(string[] args)
        {
            Startup<Handler>.ServerlessProject = new ServerlessProject();
            BuildWebHost(args).Run();
        }

        private static IWebHost BuildWebHost(string[] args)
        {
            return WebHost.CreateDefaultBuilder(args)
                .ConfigureLogging((webhostContext, builder) =>
                {
                    builder.AddConfiguration(webhostContext.Configuration.GetSection("Logging"))
                        .AddConsole()
                        .AddDebug();
                })
                .UseStartup<Startup<Handler>>()
                .UseUrls($"http://localhost:{Startup<Handler>.ServerlessProject.Port}")
                .Build();
        }
    }
}