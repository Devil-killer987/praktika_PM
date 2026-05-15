using System;
using System.Linq;
using System.Web.Http;

using api_work2.Models;

namespace Api_work.Controllers
{
    [RoutePrefix("api/batches")]
    public class BatchesController : ApiController
    {
        private AgroControlEntities db = new AgroControlEntities();

        // GET: api/batches
        [HttpGet]
        [Route("")]
        public IHttpActionResult GetAllBatches()
        {
            var batches = db.batches
                .Select(b => new
                {
                    b.id,
                    b.batch_number,
                    b.order_id,
                    order_number = b.production_orders.order_number,
                    product_name = b.production_orders.recipes.products.name,
                    b.start_time,
                    b.end_time,
                    b.status,
                    b.actual_quantity_kg,
                    b.deviation_count,
                    b.created_at
                })
                .ToList();

            return Ok(batches);
        }

        // GET: api/batches/active
        [HttpGet]
        [Route("active")]
        public IHttpActionResult GetActiveBatches()
        {
            var batches = db.batches
                .Where(b => b.status == "running" || b.status == "paused")
                .Select(b => new
                {
                    b.id,
                    b.batch_number,
                    product_name = b.production_orders.recipes.products.name,
                    line = "Линия №1",
                    b.status,
                    current_step = b.batch_steps.FirstOrDefault(bs => bs.end_time == null).step_name ?? "Не начат",
                    current_step_progress = b.batch_steps.Count(bs => bs.end_time != null),
                    total_steps = b.batch_steps.Count(),
                    has_deviation = (b.deviation_count ?? 0) > 0,
                    b.start_time
                })
                .ToList();

            return Ok(batches);
        }

        // GET: api/batches/{id}
        [HttpGet]
        [Route("{id}")]
        public IHttpActionResult GetBatchById(int id)
        {
            var batch = db.batches
                .Where(b => b.id == id)
                .Select(b => new
                {
                    b.id,
                    b.batch_number,
                    b.order_id,
                    order_number = b.production_orders.order_number,
                    product_name = b.production_orders.recipes.products.name,
                    b.start_time,
                    b.end_time,
                    b.status,
                    b.actual_quantity_kg,
                    b.deviation_count,
                    b.created_at
                })
                .FirstOrDefault();

            if (batch == null)
                return NotFound();

            return Ok(batch);
        }

        // GET: api/batches/{id}/program
        [HttpGet]
        [Route("{id}/program")]
        public IHttpActionResult GetProgram(int id)
        {
            var batch = db.batches.Find(id);
            if (batch == null)
                return NotFound();

            var steps = batch.batch_steps
                .OrderBy(bs => bs.step_order)
                .Select(bs => new
                {
                    bs.id,
                    bs.step_order,
                    bs.step_name,
                    status = bs.end_time != null ? "completed" : (bs.start_time != null ? "in_progress" : "pending"),
                    bs.actual_temp_c,
                    bs.actual_duration_min,
                    bs.actual_pressure_bar,
                    bs.deviation_flag,
                    bs.operator_comment,
                    bs.start_time,
                    bs.end_time,
                    planned_temp_c = bs.tech_card_steps.planned_temp_c,
                    planned_duration_min = bs.tech_card_steps.planned_duration_min,
                    planned_pressure_bar = bs.tech_card_steps.planned_pressure_bar,
                    instruction = bs.tech_card_steps.instruction ?? "Выполните операцию",
                    temp_tolerance_max = bs.tech_card_steps.temp_tolerance_max,
                    pressure_tolerance_max = bs.tech_card_steps.pressure_tolerance_max,
                    is_mandatory = bs.tech_card_steps.is_mandatory
                })
                .ToList();

            return Ok(new
            {
                batch.id,
                batch.batch_number,
                product_name = batch.production_orders.recipes.products.name,
                batch.status,
                batch.start_time,
                steps = steps,
                current_step_id = steps.FirstOrDefault(s => s.status == "in_progress")?.id ?? steps.FirstOrDefault()?.id
            });
        }

        // GET: api/batches/{id}/steps
        [HttpGet]
        [Route("{id}/steps")]
        public IHttpActionResult GetBatchSteps(int id)
        {
            var steps = db.batch_steps
                .Where(bs => bs.batch_id == id)
                .OrderBy(bs => bs.step_order)
                .Select(bs => new
                {
                    bs.id,
                    bs.step_order,
                    bs.step_name,
                    bs.actual_temp_c,
                    bs.actual_duration_min,
                    bs.actual_pressure_bar,
                    bs.start_time,
                    bs.end_time,
                    bs.deviation_flag,
                    bs.operator_comment,
                    planned_temp_c = bs.tech_card_steps.planned_temp_c,
                    planned_duration_min = bs.tech_card_steps.planned_duration_min,
                    planned_pressure_bar = bs.tech_card_steps.planned_pressure_bar
                })
                .ToList();

            return Ok(steps);
        }

        // POST: api/batches
        [HttpPost]
        [Route("")]
        public IHttpActionResult CreateBatch([FromBody] CreateBatchRequest request)
        {
            if (request == null)
                return BadRequest("Request is null");

            var batch = new batches
            {
                batch_number = request.batch_number,
                order_id = request.order_id,
                status = "planned",
                created_at = DateTime.Now
            };

            db.batches.Add(batch);
            db.SaveChanges();

            return Ok(new { id = batch.id, message = "Batch created" });
        }

        // POST: api/batches/{id}/start
        [HttpPost]
        [Route("{id}/start")]
        public IHttpActionResult StartBatch(int id)
        {
            var batch = db.batches.Find(id);
            if (batch == null)
                return NotFound();

            if (batch.status != "planned")
                return BadRequest("Batch can only be started from planned status");

            batch.status = "running";
            batch.start_time = DateTime.Now;
            db.SaveChanges();

            return Ok(new { message = "Batch started", batch_id = batch.id, status = batch.status });
        }

        // POST: api/batches/{id}/pause
        [HttpPost]
        [Route("{id}/pause")]
        public IHttpActionResult PauseBatch(int id)
        {
            var batch = db.batches.Find(id);
            if (batch == null)
                return NotFound();

            if (batch.status != "running")
                return BadRequest("Only running batches can be paused");

            batch.status = "paused";
            db.SaveChanges();

            return Ok(new { message = "Batch paused", batch_id = batch.id, status = batch.status });
        }

        // POST: api/batches/{id}/resume
        [HttpPost]
        [Route("{id}/resume")]
        public IHttpActionResult ResumeBatch(int id)
        {
            var batch = db.batches.Find(id);
            if (batch == null)
                return NotFound();

            if (batch.status != "paused")
                return BadRequest("Only paused batches can be resumed");

            batch.status = "running";
            db.SaveChanges();

            return Ok(new { message = "Batch resumed", batch_id = batch.id, status = batch.status });
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

    public class CreateBatchRequest
    {
        public string batch_number { get; set; }
        public int order_id { get; set; }
    }
}