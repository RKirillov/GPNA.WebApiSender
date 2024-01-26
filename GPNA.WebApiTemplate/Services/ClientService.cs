using GPNA.WebApiSender.Configuration;
using Grpc.Core;
using Grpc.Net.Client;

namespace GPNA.WebApiSender.Services
{
    public class ClientService : BackgroundService
    {
        private readonly ILogger<ClientService> _logger;
        private readonly string _url;
        private readonly MessageConfiguration _message;
        string[] _messages = { "Вася", "Шварц", "Анна", "Сучка", "Google" };
        public ClientService(ILogger<ClientService> logger, IConfiguration configuration, MessageConfiguration message)
        {
            _logger = logger;
            _url = configuration[key: "Kestrel:Endpoints:gRPC:Url"]??string.Empty;
            _message = message;
        }
        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            // создаем канал для обмена сообщениями с сервером
            // параметр - адрес сервера gRPC
            using var channel = GrpcChannel.ForAddress(_url);
            // создаем клиент
            var client = new GreeterGrpc.GreeterGrpcClient(channel);

            // посылаем  сообщение HelloRequest серверу
            var serverData = client.SayHello1();

            // получаем поток сервера
            var responseStream = serverData.ResponseStream;

            Task readTask;
            while (!stoppingToken.IsCancellationRequested)
            {


                readTask = Task.Run(async () =>
                {
                    await foreach (var response in serverData.ResponseStream.ReadAllAsync())
                    {
                        _logger.LogInformation($"Server: {response.Message}");
                    }
                });

                // посылаем каждое сообщение
                foreach (var message in _messages)
                {
                    await serverData.RequestStream.WriteAsync(new HelloRequest { Name = message, Value = _message.Value });
                    await Task.Delay(2000);
                }

                // завершаем отправку сообщений на сервер
                await serverData.RequestStream.CompleteAsync();
                await readTask;
            }
        }
    }
}
