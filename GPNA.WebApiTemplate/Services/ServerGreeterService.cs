using Google.Protobuf;
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
        //CancellationTokenSource cancellationTokenSource = new CancellationTokenSource();
        //CancellationToken token = cancellationTokenSource.Token;

        try
        {
            var i = 0;//имитация передачи тегов
        while (!context.CancellationToken.IsCancellationRequested && i < 10)
        {
            i++;
            foreach (var message in messages)
            {
                await responseStream.WriteAsync(new HelloReply { Message = message });//, context.CancellationToken
                    _logger.LogInformation(i.ToString());
                await Task.Delay(TimeSpan.FromSeconds(1), context.CancellationToken);
            }
        }
        }
        catch (Exception ex)
        {
            _logger.LogError($"Srever: {ex.Message}");
        }
    }
}