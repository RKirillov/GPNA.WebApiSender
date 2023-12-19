using Grpc.Core;
using static Google.Rpc.Context.AttributeContext.Types;

namespace GPNA.WebApiSender.Services;

//Greeter.GreeterBase - абстрактный класс, который автоматически генерируется
//по определению сервиса greeter в файле greeter.proto
//это сервис (сервер)
public class ServerGreeterService : GreeterServerStream.GreeterServerStreamBase
{
    private readonly ILogger<ServerGreeterService> _logger;
    string[] messages = { "Привет", "Как дела?", "Че молчишь?", "Ты че, спишь?", "Ну пока" };
    public ServerGreeterService(ILogger<ServerGreeterService> logger)
    {
        _logger = logger;
    }
    //ообщение от клиента в виде объекта request. 
    public override async Task SayHello1(HelloRequest request, IServerStreamWriter<HelloReply> responseStream, ServerCallContext context)
    {
        while (true)
        {
            foreach (var message in messages)
            {
                //Потоковая передача сервера завершается, когда происходит выход из метода.
                await responseStream.WriteAsync(new HelloReply
                {
                    Message = $"{message} {request.Name} {request.Value}"
                });
                // для имитации работы делаем задержку в 1 секунду
                await Task.Delay(TimeSpan.FromSeconds(1));
            }
        }
    }
}