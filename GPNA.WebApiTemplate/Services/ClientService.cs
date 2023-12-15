
using GPNA.WebApiSender.Configuration;
using Grpc.Net.Client;

namespace GPNA.WebApiSender.Services
{
    public class ClientService : BackgroundService
    {
        private readonly ILogger<ClientService> _logger;
        private readonly string _url;
        private readonly MessageConfiguration _message;
        public ClientService(ILogger<ClientService> logger, IConfiguration configuration, MessageConfiguration message)
        {
            _logger = logger;
            _url = configuration[key: "Kestrel:Endpoints:gRPC:Url"];
            _message = message;
        }
        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            using var channel = GrpcChannel.ForAddress(_url);
            var client = new GreeterUnary.GreeterUnaryClient(channel);

            // посылаем пустое сообщение и получаем набор сообщений
            var serverData = client.SayHello1(new HelloRequest
            {
                Name = _message.Name,
                Value = _message.Value
            }); 

            // получаем поток сервера
            var responseStream = serverData.ResponseStream;


            while (!stoppingToken.IsCancellationRequested)
            {
                // с помощью итераторов извлекаем каждое сообщение из потока
                while (await responseStream.MoveNext(stoppingToken))
                {
                    HelloReply response = responseStream.Current;
                    _logger.LogInformation($"Greeting: {response.Message} -- {DateTime.Now}");
                    await Task.Delay(1000, stoppingToken);
                }
            }
        }
    }
}
