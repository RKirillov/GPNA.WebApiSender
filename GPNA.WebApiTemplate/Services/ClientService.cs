﻿
using GPNA.Converters.TagValues;
using Grpc.Core;
using System.Collections.Concurrent;
using System.Diagnostics;

namespace GPNA.WebApiSender.Services
{
    public class ClientService : BackgroundService, IClientService
    {
        private readonly GreeterGrpc.GreeterGrpcClient _client;
        private readonly ILogger<ClientService> _logger;
        private readonly ConcurrentQueue<TagValueDouble?> _storage = new();
        private const int MS_IN_SECOND = 1000;
        private const int BATCH_COUNT = 100000;
        private const int DEADLINE_SEC = 100;
        public ClientService(ILogger<ClientService> logger, GreeterGrpc.GreeterGrpcClient client)
        {
            _logger = logger;
            _client = client;
        }

        public TagValueDouble? GetTag()
        {
            _storage.TryDequeue(out var parameter);
             return parameter;
        }

        public IEnumerable<TagValueDouble?> GetTags (int chunkSize)
        {
            for (int i = 0; i < chunkSize && !_storage.IsEmpty; i++)
            {
                _storage.TryDequeue(out var parameter);
                yield return parameter;
            }
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {

            // создаем канал для обмена сообщениями с сервером
            // параметр - адрес сервера gRPC
            Stopwatch stopwatch = new();
            stopwatch.Start();


            // {IDLE, CONNECTING, READY!} 
            //_logger.LogInformation($"{channel.State}");

            // посылаем  пустое сообщение Request серверу
            using var serverData = _client.Transfer(new Request(), new CallOptions().WithWaitForReady(true).WithDeadline(DateTime.UtcNow.AddSeconds(DEADLINE_SEC)).WithCancellationToken(stoppingToken));

            // получаем поток сервера
            var responseStream = serverData.ResponseStream;
            var batchCounter = 0;
    
            try
            {
                while (!stoppingToken.IsCancellationRequested && batchCounter< BATCH_COUNT)
                {
                    await foreach (var response in serverData.ResponseStream.ReadAllAsync(stoppingToken))
                    {
                        batchCounter+= response.Items.Count;
                        _logger.LogTrace($"Transfer count: {batchCounter}");
                        foreach (var protoItem in response.Items)
                        {
                            _storage.Enqueue(new TagValueDouble()
                            {
                                TagId = protoItem.TagId,
                                DateTime = protoItem.DateTime.ToDateTime(),
                                DateTimeUtc = protoItem.DateTimeUtc.ToDateTime(),
                                TimeStampUtc = protoItem.TimeStampUtc.ToDateTime(),
                                OpcQuality = protoItem.OpcQuality,
                                Tagname = protoItem.Tagname,
                                Value = protoItem.Value 
                            });
                        }
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
                _logger.LogInformation($"Transfer speed: {batchCounter/((double)stopwatch.ElapsedMilliseconds / MS_IN_SECOND)} msg/sec.");
                _logger.LogInformation("Client is stopped");

            }
        }
    }
}
