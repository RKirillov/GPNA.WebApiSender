
using Grpc.Core;
using System.Diagnostics;

namespace GPNA.WebApiSender.Services
{
    public class ClientService : BackgroundService
    {
        private readonly GreeterGrpc.GreeterGrpcClient _client;
        private readonly ILogger<ClientService> _logger;
        private const int MS_IN_SECOND = 1000;

        public ClientService(ILogger<ClientService> logger, GreeterGrpc.GreeterGrpcClient client)
        {
            _logger = logger;
            _client = client;
        }
        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {

            // создаем канал для обмена сообщениями с сервером
            // параметр - адрес сервера gRPC
            Stopwatch stopwatch = new();
            stopwatch.Start();


            // {IDLE, CONNECTING, READY!} 
            //_logger.LogInformation($"{channel.State}");

            // посылаем  сообщение HelloRequest серверу
            using  var serverData = _client.Transfer(new Request(), new CallOptions().WithWaitForReady(true).WithDeadline(DateTime.UtcNow.AddSeconds(100)).WithCancellationToken(stoppingToken));

            // получаем поток сервера
            var responseStream = serverData.ResponseStream;
            var butchCounter = 0;
    
            try
            {
                while (!stoppingToken.IsCancellationRequested && butchCounter<100000)
                {
                    //Для считывания данных из потока можно использовать разные стратегии. 
                    // здесь с помощью итераторов извлекаем каждое сообщение из потока
                    //var i = 0;
                    await foreach (var response in serverData.ResponseStream.ReadAllAsync(stoppingToken))
                    {
                        butchCounter+= response.Items.Count();
                        _logger.LogInformation($"Transfer count: {butchCounter}");
/*                        foreach (var item in response.Items)
                        {
                            _logger.LogInformation($"Item: {item.Tagname} {item.DateTime}");
                        }*/
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
            finally 
            {

                await StopAsync(stoppingToken);
                stopwatch.Stop();
                _logger.LogInformation($"Transfer speed: {butchCounter/((double)stopwatch.ElapsedMilliseconds / MS_IN_SECOND)} msg/sec.");
                _logger.LogInformation("Client is stopped");

            }
        }
    }
}
