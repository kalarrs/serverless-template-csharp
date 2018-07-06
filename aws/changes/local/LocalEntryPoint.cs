using Kalarrs.NetCore.Util;
using Kalarrs.Sreverless.NetCore;
using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Logging;

namespace changes.Local
{
    /// <summary>
    /// The Main function can be used to run the ASP.NET Core application locally using the Kestrel webserver.
    /// </summary>
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
