using System;
using System.Web.Http;

namespace Api_work.Controllers
{
    [RoutePrefix("api/equipment")]
    public class EquipmentController : ApiController
    {
        private static readonly Random _random = new Random();

        // GET: api/equipment/extruder/telemetry
        [HttpGet]
        [Route("extruder/telemetry")]
        public IHttpActionResult GetExtruderTelemetry()
        {
            // Симуляция телеметрии
            var telemetry = new
            {
                current_temperature = Math.Round(70 + (decimal)_random.NextDouble() * 20, 1),
                current_pressure = Math.Round(2.5m + (decimal)_random.NextDouble() * 1.5m, 1),
                current_rpm = 1500 + _random.Next(500),
                equipment_status = "Работает",
                last_update = DateTime.Now.ToString("HH:mm:ss")
            };

            return Ok(telemetry);
        }
    }
}