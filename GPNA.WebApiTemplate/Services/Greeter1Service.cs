using Grpc.Core;

namespace GPNA.WebApiSender.Services;

//Greeter.GreeterBase - абстрактный класс, который автоматически генерируется
//по определению сервиса greeter в файле greeter.proto
//это сервис (сервер)
public class Greeter1Service : GreeterRoman.GreeterRomanBase
{
    private readonly ILogger<Greeter1Service> _logger;
    public Greeter1Service(ILogger<Greeter1Service> logger)
    {
        _logger = logger;
    }
    //ообщение от клиента в виде объекта request. 
    public override Task<HelloReply> SayHello1(HelloRequest request, ServerCallContext context)
    {
        return Task.FromResult(new HelloReply
        {
            Message = $"Hello {request.Name} {request.Value}"
        });
    }
}