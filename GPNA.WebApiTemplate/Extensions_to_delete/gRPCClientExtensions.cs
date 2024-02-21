using GPNA.gRPCClient.ServiceTagBool;
using GPNA.gRPCClient.ServiceTagDouble;
using gRPCClient;
using gRPCClient.Configuration;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace GPNA.gRPCClient.Extensions
{
    public static class gRPCClientExtensions
    {
        private static readonly ILogger<gRPCClientConfiguration> _producerlogger;
        static gRPCClientExtensions()
        {
            _producerlogger = LoggerFactory.Create(cfg => { }).CreateLogger<gRPCClientConfiguration>();
        }

        /// <summary>
        /// Конфигурация передачи TagValueDouble
        /// </summary>
        /// <param name="self"></param>
        /// <param name="configuration">Конфигурация HttpClientConfiguration</param>
        /// <returns></returns>
        public static IServiceCollection gRPCConfigureDouble_(this IServiceCollection self, HttpClientConfiguration configuration, gRPCClientConfiguration gRPCConfiguration)
        {
            try
            {
                var clientConfiguration = new gRPCClientConfiguration
                {
                    BatchCount = gRPCConfiguration.BatchCount,
                    DeadLineSec = gRPCConfiguration.DeadLineSec,
                    WithWaitForReady = gRPCConfiguration.WithWaitForReady
                };
                self.TryAddSingleton(clientConfiguration);

                var handler = new SocketsHttpHandler
                {
                    PooledConnectionIdleTimeout = Timeout.InfiniteTimeSpan,
                    KeepAlivePingDelay = TimeSpan.FromSeconds(configuration.KeepAlivePingDelay),
                    KeepAlivePingTimeout = TimeSpan.FromSeconds(configuration.KeepAlivePingTimeout),
                    EnableMultipleHttp2Connections = configuration.EnableMultipleHttp2Connections
                };

                var loggerFactory = LoggerFactory.Create(logging =>
                {
                    logging.AddConsole();
                    logging.SetMinimumLevel(LogLevel.Trace);
                });

                self.AddGrpcClient<GreeterGrpcDouble.GreeterGrpcDoubleClient>(o =>
                    {
                        string connectionPort = $"http://localhost:{configuration.PortNumber}";
                        o.Address = new Uri(connectionPort);
                    }).ConfigureChannel(o =>
                    {
                        o.HttpHandler = handler;
                        o.LoggerFactory = loggerFactory;
                    }
                 );
            }
            catch (Exception ex)
            {
                _producerlogger.LogError($"Client: {ex.Message}");
            }
            return self;
        }

        /// <summary>
        /// Конфигурация передачи TagValueBool
        /// </summary>
        /// <param name="self"></param>
        /// <param name="httpConfiguration">Конфигурация HttpClientConfiguration</param>
        /// <returns></returns>
        public static IServiceCollection gRPCConfigureBool_(this IServiceCollection self, HttpClientConfiguration httpConfiguration, gRPCClientConfiguration gRPCConfiguration)
        {
            try
            {
                var clientConfiguration = new gRPCClientConfiguration
                {
                    BatchCount = gRPCConfiguration.BatchCount,
                    DeadLineSec = gRPCConfiguration.DeadLineSec,
                    WithWaitForReady = gRPCConfiguration.WithWaitForReady
                };
                self.TryAddSingleton(clientConfiguration);
                {
                    var handler = new SocketsHttpHandler
                    {
                        PooledConnectionIdleTimeout = Timeout.InfiniteTimeSpan,
                        KeepAlivePingDelay = TimeSpan.FromSeconds(httpConfiguration.KeepAlivePingDelay),
                        KeepAlivePingTimeout = TimeSpan.FromSeconds(httpConfiguration.KeepAlivePingTimeout),
                        EnableMultipleHttp2Connections = httpConfiguration.EnableMultipleHttp2Connections
                    };

                    var loggerFactory = LoggerFactory.Create(logging =>
                    {
                        logging.AddConsole();
                        logging.SetMinimumLevel(LogLevel.Trace);
                    });

                    self.AddGrpcClient<GreeterGrpcBool.GreeterGrpcBoolClient>(o =>
                    {
                        string connectionPort = $"http://localhost:{httpConfiguration.PortNumber}";
                        o.Address = new Uri(connectionPort);
                    }).ConfigureChannel(o =>
                    {
                        o.HttpHandler = handler;
                        o.LoggerFactory = loggerFactory;
                    }
                     );
                }
            }
            catch (Exception ex)
            {
                _producerlogger.LogError($"Client: {ex.Message}");
            }
            return self;
        }

        public static IHostBuilder gRPCHostBuilderBool_(this IHostBuilder self)
        {
            self.ConfigureServices(svc =>
                             {
                                 svc.AddHostedService<ClientServiceBool_>();
                             });
            return self;
        }

        public static IHostBuilder gRPCHostBuilderDouble_(this IHostBuilder self)
        {
            self.ConfigureServices(svc =>
            {
                svc.AddHostedService<ClientServiceDouble_>();
            });
            return self;
        }

    }
}
