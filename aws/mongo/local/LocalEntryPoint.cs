using Kalarrs.Serverless.NetCore.Core;
using Kalarrs.Sreverless.NetCore;
using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Logging;

namespace mongo.Local
{
    public class LocalEntryPoint
    {
        public static void Main(string[] args)
        {
            BuildWebHost(args).Run();
        }

        public static IWebHost BuildWebHost(string[] args)
        {
            var serverlessProject = new ServerlessProject();
            var port = serverlessProject.GetPort();
            
            return WebHost.CreateDefaultBuilder(args)
                .ConfigureLogging((webhostContext, builder) =>
                {
                    builder.AddConfiguration(webhostContext.Configuration.GetSection("Logging"))
                        .AddConsole()
                        .AddDebug();
                })
                .UseStartup<Startup<Handler>>()
                .UseUrls($"http://localhost:{port}")
                .Build();
        }
    }
}
