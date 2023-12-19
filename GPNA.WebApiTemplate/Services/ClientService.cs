
using GPNA.WebApiSender.Configuration;
using Grpc.Net.Client;
using static Google.Rpc.Context.AttributeContext.Types;

namespace GPNA.WebApiSender.Services
{
    public class ClientService : BackgroundService
    {
        private readonly ILogger<ClientService> _logger;
        private readonly string _url;
        private readonly MessageConfiguration _message;
        string[] messages = { "Вася", "Шварц", "Анна", "Лена", "Петя" };
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
            var client = new GreeterServerSream.GreeterServerSreamClient(channel);

            // посылаем  сообщение HelloRequest серверу
            var serverData = client.SayHello1();

            // посылаем каждое сообщение
            foreach (var message in messages)
            {
                await serverData.RequestStream.WriteAsync(new HelloRequest { Name = message , Value= _message.Value});
            }

            // получаем поток сервера
            /*            var responseStream = serverData.ResponseStream;


                        while (!stoppingToken.IsCancellationRequested)
                        {
                            //Для считывания данных из потока можно использовать разные стратегии. 
                            // здесь с помощью итераторов извлекаем каждое сообщение из потока
                            while (await responseStream.MoveNext(stoppingToken))
                            {
                                HelloReply response = responseStream.Current;
                                _logger.LogInformation($"Ответ сервера: {response.Message} -- {DateTime.Now}");
                                await Task.Delay(1000, stoppingToken);
                            }
                        }*/
        }
    }
}
