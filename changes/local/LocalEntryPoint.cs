using Kalarrs.Serverless.NetCore.Yaml;
using Kalarrs.Sreverless.NetCore;
using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Logging;

namespace changes.Local
{
    /// <summary>
    /// The Main function can be used to run the ASP.NET Core application locally using the Kestrel webserver.
    /// </summary>
    public class LocalEntryPoint
    {
        public static void Main(string[] args)
        {
            BuildWebHost(args).Run();
        }

        public static IWebHost BuildWebHost(string[] args)
        {
            var parser = new Parser();
            var port = parser.GetPort();
            
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
