
using api_work2.Models;
using System;
using System.Linq;
using System.Web.Http;
using System.Web.Http.Description;

namespace Api_work.Controllers
{
    [RoutePrefix("api/batchsteps")]
    public class BatchStepsController : ApiController
    {
        private AgroControlEntities db = new AgroControlEntities();

        // GET: api/batchsteps
        public IQueryable<batch_steps> Getbatch_steps()
        {
            return db.batch_steps;
        }

        // GET: api/batchsteps/5
        [ResponseType(typeof(batch_steps))]
        public IHttpActionResult Getbatch_steps(int id)
        {
            batch_steps batch_steps = db.batch_steps.Find(id);
            if (batch_steps == null)
            {
                return NotFound();
            }

            return Ok(batch_steps);
        }

        // POST: api/batchsteps/5/start
        [HttpPost]
        [Route("{id}/start")]
        [ResponseType(typeof(string))]
        public IHttpActionResult StartStep(int id)
        {
            var step = db.batch_steps.Find(id);
            if (step == null)
            {
                return NotFound();
            }

            if (step.start_time != null)
            {
                return BadRequest("Step already started");
            }

            step.start_time = DateTime.Now;
            db.SaveChanges();

            return Ok($"Step {step.step_name} started at {step.start_time}");
        }

        // PUT: api/batchsteps/5/complete
        [HttpPut]
        [Route("{id}/complete")]
        [ResponseType(typeof(object))]
        public IHttpActionResult CompleteStep(int id, [FromBody] StepCompletionRequest request)
        {
            var step = db.batch_steps.Find(id);
            if (step == null)
            {
                return NotFound();
            }

            // Update actual values
            if (request.ActualTempC.HasValue)
                step.actual_temp_c = request.ActualTempC;
            if (request.ActualDurationMin.HasValue)
                step.actual_duration_min = request.ActualDurationMin;
            if (request.ActualPressureBar.HasValue)
                step.actual_pressure_bar = request.ActualPressureBar;

            step.end_time = DateTime.Now;
            step.operator_comment = request.OperatorComment;

            // Check for deviations
            var techStep = db.tech_card_steps.Find(step.step_id);
            if (techStep != null)
            {
                bool hasDeviation = false;
                string deviationMessage = "";

                // Check temperature deviation
                if (step.actual_temp_c.HasValue && techStep.planned_temp_c.HasValue)
                {
                    var tempDiff = Math.Abs(step.actual_temp_c.Value - techStep.planned_temp_c.Value);
                    var tempTolerance = techStep.temp_tolerance_max ?? 0;
                    if (tempDiff > tempTolerance)
                    {
                        hasDeviation = true;
                        deviationMessage += $"Temperature {step.actual_temp_c}°C (planned: {techStep.planned_temp_c}°C). ";
                    }
                }

                // Check pressure deviation
                if (step.actual_pressure_bar.HasValue && techStep.planned_pressure_bar.HasValue)
                {
                    var pressureDiff = Math.Abs(step.actual_pressure_bar.Value - techStep.planned_pressure_bar.Value);
                    var pressureTolerance = techStep.pressure_tolerance_max ?? 0;
                    if (pressureDiff > pressureTolerance)
                    {
                        hasDeviation = true;
                        deviationMessage += $"Pressure {step.actual_pressure_bar} bar (planned: {techStep.planned_pressure_bar} bar). ";
                    }
                }

                if (hasDeviation)
                {
                    step.deviation_flag = true;

                    // Create deviation event
                    var deviation = new deviation_events
                    {
                        batch_id = step.batch_id,
                        step_id = step.id,
                        deviation_type = "parameter_deviation",
                        description = deviationMessage + (request.OperatorComment ?? ""),
                        severity = request.Severity ?? "warning",
                        created_at = DateTime.Now
                    };
                    db.deviation_events.Add(deviation);

                    // Update batch deviation count
                    var batch = db.batches.Find(step.batch_id);
                    if (batch != null)
                    {
                        batch.deviation_count = (batch.deviation_count ?? 0) + 1;
                    }
                }
            }

            db.SaveChanges();

            // Check if all steps are completed
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
                deviation_flag = step.deviation_flag,
                all_steps_completed = allCompleted
            });
        }

        // POST: api/batchsteps/5/fix-deviation
        [HttpPost]
        [Route("{id}/fix-deviation")]
        [ResponseType(typeof(string))]
        public IHttpActionResult FixDeviation(int id, [FromBody] FixDeviationRequest request)
        {
            var deviation = db.deviation_events.FirstOrDefault(d => d.step_id == id && d.resolved == false);
            if (deviation == null)
            {
                return NotFound();
            }

            deviation.resolved = true;
            deviation.resolved_at = DateTime.Now;
            deviation.resolved_by = request.ResolvedBy;
            deviation.description = deviation.description + " | Fixed: " + request.FixComment;
            db.SaveChanges();

            return Ok($"Deviation fixed by user {request.ResolvedBy}");
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

    // Request DTOs
    public class StepCompletionRequest
    {
        public decimal? ActualTempC { get; set; }
        public int? ActualDurationMin { get; set; }
        public decimal? ActualPressureBar { get; set; }
        public string OperatorComment { get; set; }
        public string Severity { get; set; } // warning, critical
    }

    public class FixDeviationRequest
    {
        public int ResolvedBy { get; set; }
        public string FixComment { get; set; }
    }
}