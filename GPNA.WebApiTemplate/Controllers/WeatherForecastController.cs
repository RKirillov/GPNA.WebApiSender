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

namespace GPNA.WebApiSender.Controllers
{
    [ApiController]
    [Produces("application/json")]
    [Route("api/[controller]")]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
    public class WeatherForecastController : ControllerBase
    {
        #region Using
        private readonly JsonConfiguration _jsonConfiguration;
        #endregion Using

        private static readonly string[] Summaries = new[]
        {
            "Freezing", "Bracing", "Chilly", "Cool", "Mild", "Warm", "Balmy", "Hot", "Sweltering", "Scorching"
        };

        private readonly ILogger<WeatherForecastController> _logger;

        #region Constructors
        public WeatherForecastController(JsonConfiguration jsonConfiguration,
            ILogger<WeatherForecastController> logger)
        {
            _jsonConfiguration = jsonConfiguration;
            _logger = logger;
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
        public ActionResult<IEnumerable<SampleReport>> GetAll()
        {
            SampleReport sampleReport = new() { Name = _jsonConfiguration.Name, Value = _jsonConfiguration.Value };
            return Ok(sampleReport);
        }
        #endregion Methods
    }
}
