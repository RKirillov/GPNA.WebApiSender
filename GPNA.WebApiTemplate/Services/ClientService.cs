using System;
using System.Threading;
using System.Threading.Tasks;
using GPNA.WebApiSender.Configuration;
using Grpc.Net.Client;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace GPNA.WebApiSender.Services
{
    public class ClientService : BackgroundService
    {
        private readonly ILogger<ClientService> _logger;
        private readonly string _url;
        private readonly MessageConfiguration _message;
        public ClientService(ILogger<ClientService> logger, IConfiguration configuration, MessageConfiguration message)
        {
            _logger = logger;
            _url = configuration[key: "Kestrel:Endpoints:gRPC:Url"];
            _message = message;
        }
        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            using var channel = GrpcChannel.ForAddress(_url);
            var client = new GreeterRoman.GreeterRomanClient(channel);
            while (!stoppingToken.IsCancellationRequested)
            {
                var reply = await client.SayHello1Async(new HelloRequest
                {
                    Name = _message.Name,
                    Value = _message.Value
                });
                _logger.LogInformation($"Greeting: {reply.Message} -- {DateTime.Now}");
                await Task.Delay(1000, stoppingToken);
            }
        }
    }
}
