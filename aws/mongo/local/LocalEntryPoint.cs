using Kalarrs.Serverless.NetCore.Core;
using Kalarrs.Sreverless.NetCore;
using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Logging;

namespace mongo.Local
{
    public class LocalEntryPoint
    {
        public static ServerlessProject ServerlessProject { get; private set; }

        public static void Main(string[] args)
        {
            ServerlessProject = new ServerlessProject();
            BuildWebHost(args).Run();
        }

        public static IWebHost BuildWebHost(string[] args)
        {
            return WebHost.CreateDefaultBuilder(args)
                .ConfigureLogging((webhostContext, builder) =>
                {
                    builder.AddConfiguration(webhostContext.Configuration.GetSection("Logging"))
                        .AddConsole()
                        .AddDebug();
                })
                .UseStartup<Startup<Handler>>()
                .UseUrls($"http://localhost:{ServerlessProject.GetPort()}")
                .Build();
        }
    }
}