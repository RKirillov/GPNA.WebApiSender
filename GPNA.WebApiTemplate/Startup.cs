
using AutoMapper;
using Google.Protobuf.WellKnownTypes;
using GPNA.Extensions.Configurations;
using GPNA.WebApiSender.Configuration;
using GPNA.WebApiSender.Services;
using Hellang.Middleware.ProblemDetails;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.OpenApi.Models;
using System;
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

            services.AddProblemDetails(ConfigureProblemDetails);
            services.AddControllers();
            services.AddSwaggerGen(c =>
            {
                c.SwaggerDoc("v1", new OpenApiInfo { Title = "GPNA.WebApiSender", Version = "v1.0" });
            });
            services.AddSingleton(_configuration.GetSection<JsonConfiguration>());
            services.AddGrpc();
            // добавляем сервисы для работы с gRPC
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
                app.UseSwagger();
                app.UseSwaggerUI(c => c.SwaggerEndpoint("/swagger/v1/swagger.json", "GPNA.WebApiSender v1"));
            }
            app.UseProblemDetails();
            app.UseCors(builder =>
                builder.WithOrigins()
                    .AllowAnyOrigin()
                    .AllowAnyHeader()
                    .AllowAnyMethod());

            app.UseRouting();
            app.UseAuthorization();
            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
                endpoints.MapGet("/", () => "Communication with gRPC endpoints must be made through a gRPC client...");
                endpoints.MapGrpcService<GreeterService>();
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
