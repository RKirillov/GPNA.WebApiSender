
using NLog.Web;
using gRPCClient.Extensions;

namespace GPNA.gRPCClient
{
    public class Program
    {
        private static IConfiguration Configuration { get; set; } = null!;

        public static void Main(string[] args)
        {
            var logger = NLogBuilder.ConfigureNLog("nlog.config").GetCurrentClassLogger();
            logger.Info("init main");
            var builder = new ConfigurationBuilder()
                .SetBasePath(AppDomain.CurrentDomain.BaseDirectory)
                .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true);
            Configuration = builder.Build();
            CreateHostBuilder(args).Build().Run();
        }

        public static IHostBuilder CreateHostBuilder(string[] args) =>
            Host.CreateDefaultBuilder(args)
                .ConfigureWebHostDefaults(webBuilder =>
                {
                    webBuilder.UseKestrel()
                        .UseConfiguration(Configuration)
                        .UseStaticWebAssets()
                        .UseStartup<Startup>()
/*                        .ConfigureKestrel(options =>
                        {
                            // Setup a HTTP/2 endpoint without TLS.
                            options.ListenLocalhost(5000, o => o.Protocols = HttpProtocols.Http2);
                        }); ;*/
                        .ConfigureLogging(logging =>
                        {
                            logging.ClearProviders();
                            logging.SetMinimumLevel(LogLevel.Trace);
                        })
                        .UseNLog();
                })
        /*                .ConfigureServices(svc =>
                        {
                            svc.AddHostedService<gRPCClient.ServiceTagDouble.ClientServiceDouble>();
                            svc.AddHostedService<gRPCClient.ServiceTagBool.ClientServiceBool>();
                        });*/
                      .gRPCHostBuilderBool()
                      .gRPCHostBuilderDouble();
    }
}
