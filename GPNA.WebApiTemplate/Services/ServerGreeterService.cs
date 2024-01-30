using Grpc.Core;

namespace GPNA.WebApiSender.Services;

//Greeter.GreeterBase - абстрактный класс, который автоматически генерируется
//по определению сервиса greeter в файле greeter.proto
//это сервис (сервер)
public class ServerGreeterService : GreeterGrpc.GreeterGrpcBase
{
    private readonly ILogger<ServerGreeterService> _logger;
    string[] _messages = { "Привет", "Как дела?" };
    //string[] _messages = { "Привет", "Как дела?", "Че молчишь?", "Ты че, спишь?", "Ну пока" };
    public ServerGreeterService(ILogger<ServerGreeterService> logger)
    {
        _logger = logger;
    }
    //ообщение от клиента в виде объекта request. 
    public override async Task SayHello1(IAsyncStreamReader<HelloRequest> requestStream, IServerStreamWriter<HelloReply> responseStream, ServerCallContext context)
    {
        CancellationTokenSource cancellationTokenSource = new CancellationTokenSource();
        CancellationToken token = cancellationTokenSource.Token;
        //!context.CancellationToken.IsCancellationRequested

        // считываем входящие сообщения в фоновой задаче
        var readTask = Task.Run(async () =>
        {
            await foreach (var helloRequest in requestStream.ReadAllAsync(context.CancellationToken))
            {
                _logger.LogInformation($"Client: {helloRequest.Name}");
            }
        });
        //!readTask.IsCompleted
        //!context.CancellationToken.IsCancellationRequested
        var i = 0;//имитация передачи тегов
        while (!context.CancellationToken.IsCancellationRequested && i<10)
        {
            i++;
            try
            {
                foreach (var message in _messages)
                {
                    await responseStream.WriteAsync(new HelloReply { Message = message }, context.CancellationToken);
                    _logger.LogInformation(i.ToString());
                    await Task.Delay(TimeSpan.FromSeconds(1), context.CancellationToken);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError($"Srever: {ex.Message}");
            }
        }
        await readTask; // ожидаем завершения задачи чтения
    }
}