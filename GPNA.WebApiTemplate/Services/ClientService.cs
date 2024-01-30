using GPNA.WebApiSender.Configuration;
using Grpc.Core;
using Grpc.Net.Client;
using Microsoft.Extensions.Logging;
using static Google.Rpc.Context.AttributeContext.Types;

namespace GPNA.WebApiSender.Services
{
    public class ClientService : BackgroundService
    {
        private readonly ILogger<ClientService> _logger;
        private readonly string _url;
        private readonly MessageConfiguration _message;
        //string[] _messages = { "Вася", "Шварц", "Анна", "Сучка", "Google" };
        string[] _messages = { "Вася", "Шварц" };
        private Task readTask = new Task(delegate { });
        public ClientService(ILogger<ClientService> logger, IConfiguration configuration, MessageConfiguration message)
        {
            _logger = logger;
            _url = configuration[key: "Kestrel:Endpoints:gRPC:Url"] ?? string.Empty;
            _message = message;
        }
        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            // создаем канал для обмена сообщениями с сервером
            // параметр - адрес сервера gRPC
            var channel = GrpcChannel.ForAddress(_url);
            // создаем клиент
            var client = new GreeterGrpc.GreeterGrpcClient(channel);


            // {IDLE, CONNECTING, READY!} 
            _logger.LogInformation($"{channel.State}");

            //!stoppingToken.IsCancellationRequested

            // посылаем  сообщение HelloRequest серверу
            using var serverData = client.SayHello1(new CallOptions().WithWaitForReady(true).WithDeadline(DateTime.UtcNow.AddSeconds(8)));

            // получаем поток сервера
            var responseStream = serverData.ResponseStream;


            try
            {
                while (channel.State== ConnectivityState.Ready)
                {
                    readTask = Task.Run(async () =>
                    {
                        // посылаем каждое сообщение
                        foreach (var message in _messages)
                        {
                            await Task.Delay(2000);
                            await serverData.RequestStream.WriteAsync(new HelloRequest { Name = message, Value = _message.Value }, stoppingToken);
                        }
                        await serverData.RequestStream.CompleteAsync();
                    });


                    await foreach (var response in serverData.ResponseStream.ReadAllAsync(stoppingToken))
                    {
                        _logger.LogInformation($"Server: {response.Message}");
                    }
                    /*while (await responseStream.MoveNext())
                    {
                           Console.WriteLine("Greeting: " + responseStream.Current.Message);
                    }*/
                    //await serverData.RequestStream.CompleteAsync();
                    //await readTask;
                }
            }
            catch (RpcException e) when (e.Status.StatusCode == StatusCode.Cancelled)
            {
                Console.WriteLine("Streaming was cancelled from the client!");
            }
            finally
            {
                await serverData.RequestStream.CompleteAsync();
                await readTask;
            }
        }
    }
}
