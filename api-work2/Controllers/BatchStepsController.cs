using System;
using System.Linq;
using System.Web.Http;

using api_work2.Models;

namespace Api_work.Controllers
{
    [RoutePrefix("api/batchsteps")]
    public class BatchStepsController : ApiController
    {
        private AgroControlEntities db = new AgroControlEntities();

        // GET: api/batchsteps
        [HttpGet]
        [Route("")]
        public IHttpActionResult GetAllBatchSteps()
        {
            var steps = db.batch_steps
                .Select(bs => new
                {
                    bs.id,
                    bs.batch_id,
                    bs.step_id,
                    bs.step_order,
                    bs.step_name,
                    bs.actual_temp_c,
                    bs.actual_duration_min,
                    bs.actual_pressure_bar,
                    bs.start_time,
                    bs.end_time,
                    bs.deviation_flag,
                    bs.operator_comment
                })
                .ToList();

            return Ok(steps);
        }

        // GET: api/batchsteps/{id}
        [HttpGet]
        [Route("{id}")]
        public IHttpActionResult GetBatchStepById(int id)
        {
            var step = db.batch_steps
                .Where(bs => bs.id == id)
                .Select(bs => new
                {
                    bs.id,
                    bs.batch_id,
                    bs.step_id,
                    bs.step_order,
                    bs.step_name,
                    bs.actual_temp_c,
                    bs.actual_duration_min,
                    bs.actual_pressure_bar,
                    bs.start_time,
                    bs.end_time,
                    bs.deviation_flag,
                    bs.operator_comment
                })
                .FirstOrDefault();

            if (step == null)
                return NotFound();

            return Ok(step);
        }

        // POST: api/batchsteps/{id}/start
        [HttpPost]
        [Route("{id}/start")]
        public IHttpActionResult StartStep(int id)
        {
            var step = db.batch_steps.Find(id);
            if (step == null)
                return NotFound();

            if (step.start_time != null)
                return BadRequest("Шаг уже начат");

            step.start_time = DateTime.Now;
            db.SaveChanges();

            return Ok(new { message = "Step started", stepId = step.id });
        }

        // PUT: api/batchsteps/{id}/complete
        [HttpPut]
        [Route("{id}/complete")]
        public IHttpActionResult CompleteStep(int id, [FromBody] CompleteStepRequest request)
        {
            var step = db.batch_steps.Find(id);
            if (step == null)
                return NotFound();

            if (request != null)
            {
                step.actual_temp_c = request.ActualTempC;
                step.actual_duration_min = request.ActualDurationMin;
                step.actual_pressure_bar = request.ActualPressureBar;
                step.operator_comment = request.OperatorComment;
            }
            step.end_time = DateTime.Now;

            // Проверка на отклонение
            bool hasDeviation = false;
            if (request != null && (request.ActualTempC.HasValue || request.ActualPressureBar.HasValue))
            {
                var techStep = db.tech_card_steps.Find(step.step_id);
                if (techStep != null)
                {
                    if (request.ActualTempC.HasValue && techStep.planned_temp_c.HasValue)
                    {
                        var tolerance = techStep.temp_tolerance_max ?? 2;
                        if (Math.Abs(request.ActualTempC.Value - techStep.planned_temp_c.Value) > tolerance)
                        {
                            hasDeviation = true;
                        }
                    }
                    if (request.ActualPressureBar.HasValue && techStep.planned_pressure_bar.HasValue)
                    {
                        var tolerance = techStep.pressure_tolerance_max ?? 0.3m;
                        if (Math.Abs(request.ActualPressureBar.Value - techStep.planned_pressure_bar.Value) > tolerance)
                        {
                            hasDeviation = true;
                        }
                    }
                }
            }

            if (hasDeviation)
            {
                step.deviation_flag = true;

                var deviation = new deviation_events
                {
                    batch_id = step.batch_id,
                    step_id = step.id,
                    deviation_type = "parameter",
                    description = request?.OperatorComment ?? "Отклонение параметров",
                    severity = request?.Severity ?? "warning",
                    created_at = DateTime.Now
                };
                db.deviation_events.Add(deviation);

                var batch = db.batches.Find(step.batch_id);
                if (batch != null)
                {
                    batch.deviation_count = (batch.deviation_count ?? 0) + 1;
                }
            }

            db.SaveChanges();

            // Проверяем, все ли шаги завершены
            var batchSteps = db.batch_steps.Where(bs => bs.batch_id == step.batch_id);
            var allCompleted = !batchSteps.Any(bs => bs.end_time == null);

            if (allCompleted)
            {
                var batch = db.batches.Find(step.batch_id);
                if (batch != null)
                {
                    batch.status = "completed";
                    batch.end_time = DateTime.Now;
                    db.SaveChanges();
                }
            }

            return Ok(new
            {
                step_id = step.id,
                step_name = step.step_name,
                completed = true,
                deviation_flag = step.deviation_flag ?? false,
                all_steps_completed = allCompleted
            });
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                db.Dispose();
            }
            base.Dispose(disposing);
        }
    }

    public class CompleteStepRequest
    {
        public decimal? ActualTempC { get; set; }
        public int? ActualDurationMin { get; set; }
        public decimal? ActualPressureBar { get; set; }
        public string OperatorComment { get; set; }
        public string Severity { get; set; }
    }
}