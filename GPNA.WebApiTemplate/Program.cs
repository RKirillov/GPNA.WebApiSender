using Google.Protobuf.WellKnownTypes;

namespace GPNA.WebApiSender
{
    public class Program
    {
        public static void Main(string[] args)
        {

            //HACK почему тут
            //var builder = WebApplication.CreateBuilder(args);
            CreateHostBuilder(args).Build().Run();
            //// добавляем сервисы для работы с gRPC
            //builder.Services.AddGrpc();

            //var app = builder.Build();

            //// настраиваем обработку HTTP-запросов
            //app.MapGrpcService<GreeterService>();
            //app.MapGet("/", () => "Communication with gRPC endpoints must be made through a gRPC client...");

            //app.Run();
        }

        public static IHostBuilder CreateHostBuilder(string[] args) =>
            Host.CreateDefaultBuilder(args)
                .ConfigureWebHostDefaults(webBuilder =>
                {
                    webBuilder.UseStartup<Startup>();
                });
    }
}
