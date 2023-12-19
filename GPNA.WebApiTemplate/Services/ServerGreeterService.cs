using Grpc.Core;
using static Google.Rpc.Context.AttributeContext.Types;

namespace GPNA.WebApiSender.Services;

//Greeter.GreeterBase - абстрактный класс, который автоматически генерируется
//по определению сервиса greeter в файле greeter.proto
//это сервис (сервер)
public class ServerGreeterService : GreeterServerSream.GreeterServerSreamBase
{
    private readonly ILogger<ServerGreeterService> _logger;
    public ServerGreeterService(ILogger<ServerGreeterService> logger)
    {
        _logger = logger;
    }
    //ообщение от клиента в виде объекта request. 
    public override async Task<HelloReply> SayHello1(IAsyncStreamReader<HelloRequest> requestStream, ServerCallContext context)
    {
        await foreach (var request in requestStream.ReadAllAsync())
        {
            //try to replace with request
            _logger.LogInformation(request.Name," ", request.Value);
        }
        _logger.LogInformation("Все данные получены...");
        return new HelloReply { Message = "все данные получены" };
        /*        while (true)
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
                }*/
    }
}