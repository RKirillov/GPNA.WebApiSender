using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using AutoMapper;
using System.Xml.Linq;
using GPNA.WebApiSender.Model;
using GPNA.WebApiSender.Configuration;
using GPNA.WebApiSender;
using Google.Protobuf;
using Grpc.Net.Client;

namespace GPNA.WebApiSender.Controllers
{
    [ApiController]
    [Produces("application/json")]
    [Route("api/[controller]")]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
    public class WeatherForecastController : ControllerBase
    {
        #region Using
        private readonly MessageConfiguration _jsonConfiguration;
        #endregion Using

        private static readonly string[] Summaries = new[]
        {
            "Freezing", "Bracing", "Chilly", "Cool", "Mild", "Warm", "Balmy", "Hot", "Sweltering", "Scorching"
        };

        private readonly ILogger<WeatherForecastController> _logger;
        private readonly string _url;
        private readonly MessageConfiguration _message;

        #region Constructors
        public WeatherForecastController(MessageConfiguration jsonConfiguration,
            ILogger<WeatherForecastController> logger, IConfiguration configuration, MessageConfiguration message)
        {
            _jsonConfiguration = jsonConfiguration;
            _logger = logger;
            _logger = logger;
            _url = configuration[key: "Kestrel:Endpoints:gRPC:Url"];
            _message = message;
        }
        #endregion Constructors



        [HttpGet]
        public IEnumerable<WeatherForecast> Get()
        {
            var rng = new Random();
            return Enumerable.Range(1, 5).Select(index => new WeatherForecast
            {
                Date = DateTime.Now.AddDays(index),
                TemperatureC = rng.Next(-20, 55),
                Summary = Summaries[rng.Next(Summaries.Length)]
            })
            .ToArray();
        }

        #region Methods
        /// <summary>
        /// Получить все топики
        /// </summary>
        /// <response code="200">Коллекция объектов топиков</response>
        [HttpGet("GetAll")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<ActionResult<SampleReport>> GetAllAsync()
        {
            using var channel = GrpcChannel.ForAddress(_url);
            var client = new GreeterRoman.GreeterRomanClient(channel);

                var reply = await client.SayHello1Async(new HelloRequest
                {
                    Name = _message.Name,
                    Value = _message.Value
                });

            return Ok(reply);
        }
        #endregion Methods
    }
}
