
using AutoMapper;
using gRPCClient.Configuration;
using gRPCClient.Extensions;
using Hellang.Middleware.ProblemDetails;
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
            //var clientConfiguration = _configuration.GetSection<ClientConfiguration>();
            services.AddSingleton(s => config.CreateMapper());

            //services.AddSingleton<gRPCClient.ServiceTagDouble.IClientServiceDouble, gRPCClient.ServiceTagDouble.ClientServiceDouble>();
            //services.AddSingleton(clientConfiguration);
            //services.AddSingleton<gRPCClient.Configuration.gRPCClientConfiguration>();
            //services.AddSingleton<gRPCClient.Configuration.gRPCClientConfiguration>();


            services.AddProblemDetails(ConfigureProblemDetails);
            services.AddControllers();
            services.gRPCConfigureDouble(
                new HttpClientConfiguration
                {
                    KeepAlivePingDelay = 10,
                    KeepAlivePingTimeout = 10,
                    PortNumber = 5000,
                    EnableMultipleHttp2Connections = true
                },
                new gRPCClientConfiguration
                {
                    BatchCount = 10000,
                    DeadLineSec = 60,
                    WithWaitForReady = true
                }
            );

            services.gRPCConfigureBool(
                new HttpClientConfiguration
                {
                    KeepAlivePingDelay = 10,
                    KeepAlivePingTimeout = 10,
                    PortNumber = 5000,
                    EnableMultipleHttp2Connections = true
                },
                new gRPCClientConfiguration
                {
                    BatchCount = 10000,
                    DeadLineSec = 60,
                    WithWaitForReady = true
                }
            );
            var handler = new SocketsHttpHandler
            {
                PooledConnectionIdleTimeout = Timeout.InfiniteTimeSpan,
                KeepAlivePingDelay = TimeSpan.FromSeconds(10),
                KeepAlivePingTimeout = TimeSpan.FromSeconds(10),
                EnableMultipleHttp2Connections = true
            };

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
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }
            else
            {
                app.UseExceptionHandler("/Error");
            }

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
