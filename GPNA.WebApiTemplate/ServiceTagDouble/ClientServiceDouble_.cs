using GPNA.Converters.TagValues;
using Grpc.Core;
using gRPCClient;
using gRPCClient.Configuration;
using System.Collections.Concurrent;
using System.Diagnostics;


namespace GPNA.gRPCClient.ServiceTagDouble
{
    public class ClientServiceDouble_ : BackgroundService, IClientServiceDouble_
    {
        private readonly GreeterGrpcDouble.GreeterGrpcDoubleClient _client;
        private readonly ILogger<ClientServiceDouble_> _logger;
        private readonly ConcurrentQueue<TagValueDouble?> _storage;
        private readonly gRPCClientConfiguration _clientConfiguration;
        private const int MS_IN_SECOND = 1000;

        public ClientServiceDouble_(ILogger<ClientServiceDouble_> logger, GreeterGrpcDouble.GreeterGrpcDoubleClient client,
           gRPCClientConfiguration clientConfiguration)
        {
            _logger = logger;
            _client = client;
            _clientConfiguration = clientConfiguration;
            _storage = new();
        }

        public TagValueDouble? GetTag()
        {
            _storage.TryDequeue(out var parameter);
            return parameter;
        }

        public IEnumerable<TagValueDouble?> GetTags(int chunkSize)
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

            // посылаем  пустое сообщение Request серверу
            using var serverData = _client.TransferDouble(new RequestDouble(), new CallOptions()
                .WithWaitForReady(_clientConfiguration.WithWaitForReady)
                .WithDeadline(DateTime.UtcNow.AddSeconds(_clientConfiguration.DeadLineSec))
                .WithCancellationToken(stoppingToken));

            // получаем поток сервера
            var responseStream = serverData.ResponseStream;
            var batchCounter = 0;

            try
            {
                while (!stoppingToken.IsCancellationRequested && batchCounter < _clientConfiguration.BatchCount)
                {
                    await foreach (var response in serverData.ResponseStream.ReadAllAsync(stoppingToken))
                    {
                        batchCounter += response.Items.Count;
                        _logger.LogInformation($"Double transfer count: {batchCounter}");
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
                _logger.LogWarning("ClientD: Streaming was cancelled from the client!");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.Message);
            }
            finally
            {
                await StopAsync(stoppingToken);
                stopwatch.Stop();
                _logger.LogInformation($"Double transfer speed: {batchCounter / ((double)stopwatch.ElapsedMilliseconds / MS_IN_SECOND)} msg/sec.");
                _logger.LogInformation("Client is stopped");
            }
        }
    }
}
