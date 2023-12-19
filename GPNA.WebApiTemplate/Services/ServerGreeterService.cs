using Grpc.Core;

namespace GPNA.WebApiSender.Services;

//Greeter.GreeterBase - абстрактный класс, который автоматически генерируется
//по определению сервиса greeter в файле greeter.proto
//это сервис (сервер)
public class ServerGreeterService : GreeterServerStream.GreeterServerStreamBase
{
    private readonly ILogger<ServerGreeterService> _logger;
    string[] _messages = { "Привет", "Как дела?", "Че молчишь?", "Ты че, спишь?", "Ну пока" };
    public ServerGreeterService(ILogger<ServerGreeterService> logger)
    {
        _logger = logger;
    }
    //ообщение от клиента в виде объекта request. 
    public override async Task SayHello1(IAsyncStreamReader<HelloRequest> requestStream, IServerStreamWriter<HelloReply> responseStream, ServerCallContext context)
    {
        CancellationTokenSource cancellationTokenSource = new CancellationTokenSource();
        CancellationToken token = cancellationTokenSource.Token;
        while (!token.IsCancellationRequested)
        {
            // считываем входящие сообщения в фоновой задаче
            var readTask = Task.Run(async () =>
            {
                await foreach (var helloRequest in requestStream.ReadAllAsync())
                {
                    _logger.LogInformation($"Client: {helloRequest.Name} {helloRequest.Value}");
                }
            });


            foreach (var message in _messages)
            {
                // Посылаем ответ, пока клиент не закроет поток
                if (!readTask.IsCompleted)
                {
                    await responseStream.WriteAsync(new HelloReply { Message = message });
                    //_logger.LogInformation(message);
                    await Task.Delay(2000);
                }
            }
            await readTask; // ожидаем завершения задачи чтения
            /*            foreach (var message in messages)
                        {
                            //Потоковая передача сервера завершается, когда происходит выход из метода.
                            await responseStream.WriteAsync(new HelloReply
                            {
                                Message = $"{message} {request.Name} {request.Value}"
                            });
                            // для имитации работы делаем задержку в 1 секунду
                            await Task.Delay(TimeSpan.FromSeconds(1));
                        }*/
        }
    }
}