
using AutoMapper;
using GPNA.Extensions.Configurations;
using GPNA.WebApiSender.Configuration;
using GPNA.WebApiSender.Services;
using Grpc.Core;
using Hellang.Middleware.ProblemDetails;
using Microsoft.Extensions.Logging;
using Microsoft.OpenApi.Models;
using System.Reflection;

namespace GPNA.WebApiSender
{
    public class Startup
    {
        #region Fields
        private ILogger<Startup>? _logger;

        private readonly IConfiguration _configuration;
        #endregion Fields

        public Startup(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            var config = new MapperConfiguration(cfg => cfg.AddMaps(Assembly.GetExecutingAssembly()));
            services.AddSingleton(s => config.CreateMapper());
            var messageConfiguration = _configuration.GetSection<MessageConfiguration>();

            services.AddProblemDetails(ConfigureProblemDetails);
            services.AddControllers();
            //https://learn.microsoft.com/ru-ru/aspnet/core/grpc/performance?view=aspnetcore-5.0
            var handler = new SocketsHttpHandler
            {
                PooledConnectionIdleTimeout = Timeout.InfiniteTimeSpan,
                KeepAlivePingDelay = TimeSpan.FromSeconds(10),
                KeepAlivePingTimeout = TimeSpan.FromSeconds(10),
                EnableMultipleHttp2Connections = true
            };

            var loggerFactory = LoggerFactory.Create(logging =>
            {
                logging.AddConsole();
                logging.SetMinimumLevel(LogLevel.Debug);
            });

            services.AddGrpcClient<GreeterGrpc.GreeterGrpcClient>(o =>
             {
                 o.Address = new Uri("http://localhost:5000");
             }).ConfigureChannel(o =>
             {
                 o.HttpHandler = handler;
                 o.LoggerFactory = loggerFactory;
             }
             );
            /*             .ConfigureChannel(o =>
                      {
                          o.Credentials = ChannelCredentials.Insecure;
                      });*/

            services.AddSwaggerGen(c =>
            {
                c.SwaggerDoc("v1",
                    new OpenApiInfo
                    {
                        Title = "GPNA.WebApiSender",
                        Version = "v1.0",
                        Contact = new OpenApiContact
                        {
                            Name = "Example Contact",
                            Url = new Uri("https://example.com/contact")
                        },
                        License = new OpenApiLicense
                        {
                            Name = "Example License",
                            Url = new Uri("https://example.com/license")
                        }
                    });

                var filePath = Path.Combine(AppContext.BaseDirectory, "GPNA.WebApiSender.xml");
                c.IncludeXmlComments(filePath);
                //c.IncludeGrpcXmlComments(filePath, includeControllerXmlComments: true);
            });

        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {

            app.UseDeveloperExceptionPage();
            app.UseSwagger();
            app.UseSwaggerUI(c =>
            {
                c.SwaggerEndpoint("/swagger/v1/swagger.json", "GPNA.WebApiSender v1");
            }
            );

            app.UseStaticFiles();
            app.UseProblemDetails();
            app.UseCors(builder =>
                builder.WithOrigins()
                    .AllowAnyOrigin()
                    .AllowAnyHeader()
                    .AllowAnyMethod());

            app.UseRouting();
            app.UseEndpoints(endpoints =>
            {

                endpoints.MapGet("/", async context => { await context.Response.WriteAsync("Communication with gRPC endpoints must be made through a gRPC client..."); });
                endpoints.MapControllers();
            });

        }

        private void ConfigureProblemDetails(Hellang.Middleware.ProblemDetails.ProblemDetailsOptions options)
        {
            options.OnBeforeWriteDetails = (ctx, problem) =>
            {
                problem.Extensions["traceId"] = ctx.TraceIdentifier;
            };
            options.MapToStatusCode<Exception>(StatusCodes.Status500InternalServerError);
        }
    }
}
