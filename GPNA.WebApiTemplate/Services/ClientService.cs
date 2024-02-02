
using GPNA.WebApiSender.Configuration;
using Grpc.Core;
using Grpc.Net.Client;
using GPNA.WebApiSender;

namespace GPNA.WebApiSender.Services
{
    public class ClientService : BackgroundService
    {
        private readonly ILogger<ClientService> _logger;
        private readonly string _url;

        public ClientService(ILogger<ClientService> logger, IConfiguration configuration, MessageConfiguration message)
        {
            _logger = logger;
            _url = configuration[key: "Kestrel:Endpoints:gRPC:Url"] ?? string.Empty;
        }
        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            //CancellationTokenSource source = new CancellationTokenSource();
            //CancellationToken token = source.Token;
            //var token = CancellationTokenSource.CreateLinkedTokenSource(stoppingToken);
            // создаем канал для обмена сообщениями с сервером
            // параметр - адрес сервера gRPC
            using var channel = GrpcChannel.ForAddress(_url);
            // создаем клиент
            var client = new GreeterGrpc.GreeterClient(channel);

            // {IDLE, CONNECTING, READY!} 
            //_logger.LogInformation($"{channel.State}");

            // посылаем  сообщение HelloRequest серверу
            using  var serverData = client.Transfer(new Request(), new CallOptions().WithWaitForReady(true).WithDeadline(DateTime.UtcNow.AddSeconds(800)).WithCancellationToken(stoppingToken));

            // получаем поток сервера
            var responseStream = serverData.ResponseStream;

            try
            {
                while (channel.State == ConnectivityState.Ready)
                {
                    //Для считывания данных из потока можно использовать разные стратегии. 
                    // здесь с помощью итераторов извлекаем каждое сообщение из потока
                    var i = 0;
                    await foreach (var response in serverData.ResponseStream.ReadAllAsync(stoppingToken))
                    {
                        _logger.LogInformation($"Server: {response.Message}");
                       if (i == 14)
                        {
                           await StopAsync(stoppingToken);
                        }
                        i++;
                    }
                    
                }
            }
            catch (RpcException e) when (e.Status.StatusCode == StatusCode.Cancelled)
            {
                _logger.LogWarning("Client: Streaming was cancelled from the client!");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.Message);
            }
        }
    }
}
