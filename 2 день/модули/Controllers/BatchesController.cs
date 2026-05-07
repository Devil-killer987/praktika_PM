using api_work2.Models;
using System;
using System.Linq;
using System.Net;
using System.Web.Http;
using System.Web.Http.Description;

namespace Api_work.Controllers
{
    [RoutePrefix("api/batches")]
    public class BatchesController : ApiController
    {
        private AgroControlEntities db = new AgroControlEntities();

        // GET: api/batches
        public IQueryable<batches> Getbatches()
        {
            return db.batches;
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
                    b.status,
                    b.actual_quantity_kg,
                    b.start_time,
                    b.end_time,
                    b.deviation_count,
                    order_number = b.production_orders.order_number,
                    product_name = b.production_orders.recipes.products.name,
                    current_step = b.batch_steps.FirstOrDefault(bs => bs.end_time == null)
                })
                .ToList();

            return Ok(batches);
        }

        // GET: api/batches/5
        [ResponseType(typeof(batches))]
        public IHttpActionResult Getbatches(int id)
        {
            batches batches = db.batches.Find(id);
            if (batches == null)
            {
                return NotFound();
            }

            return Ok(batches);
        }

        // GET: api/batches/5/steps
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
        [ResponseType(typeof(batches))]
        public IHttpActionResult Postbatches(batches batches)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            batches.created_at = DateTime.Now;
            batches.status = "planned";
            db.batches.Add(batches);
            db.SaveChanges();

            // Create batch steps from tech card
            var order = db.production_orders.Find(batches.order_id);
            if (order != null)
            {
                var recipe = db.recipes.Find(order.recipe_id);
                if (recipe != null)
                {
                    var techCard = db.tech_cards.FirstOrDefault(tc => tc.product_id == recipe.product_id && tc.status == "active");
                    if (techCard != null)
                    {
                        var steps = db.tech_card_steps.Where(ts => ts.card_id == techCard.id).OrderBy(ts => ts.step_order);
                        foreach (var step in steps)
                        {
                            var batchStep = new batch_steps
                            {
                                batch_id = batches.id,
                                step_id = step.id,
                                step_order = step.step_order,
                                step_name = step.step_name
                            };
                            db.batch_steps.Add(batchStep);
                        }
                        db.SaveChanges();
                    }
                }
            }

            return CreatedAtRoute("DefaultApi", new { id = batches.id }, batches);
        }

        // POST: api/batches/5/start
        [HttpPost]
        [Route("{id}/start")]
        [ResponseType(typeof(string))]
        public IHttpActionResult StartBatch(int id)
        {
            var batch = db.batches.Find(id);
            if (batch == null)
            {
                return NotFound();
            }

            if (batch.status != "planned")
            {
                return BadRequest("Batch can only be started from planned status");
            }

            batch.status = "running";
            batch.start_time = DateTime.Now;
            db.SaveChanges();

            return Ok($"Batch {batch.batch_number} started");
        }

        // POST: api/batches/5/pause
        [HttpPost]
        [Route("{id}/pause")]
        [ResponseType(typeof(string))]
        public IHttpActionResult PauseBatch(int id)
        {
            var batch = db.batches.Find(id);
            if (batch == null)
            {
                return NotFound();
            }

            if (batch.status != "running")
            {
                return BadRequest("Only running batches can be paused");
            }

            batch.status = "paused";
            db.SaveChanges();

            return Ok($"Batch {batch.batch_number} paused");
        }

        // POST: api/batches/5/resume
        [HttpPost]
        [Route("{id}/resume")]
        [ResponseType(typeof(string))]
        public IHttpActionResult ResumeBatch(int id)
        {
            var batch = db.batches.Find(id);
            if (batch == null)
            {
                return NotFound();
            }

            if (batch.status != "paused")
            {
                return BadRequest("Only paused batches can be resumed");
            }

            batch.status = "running";
            db.SaveChanges();

            return Ok($"Batch {batch.batch_number} resumed");
        }

        // PUT: api/batches/5
        [ResponseType(typeof(void))]
        public IHttpActionResult Putbatches(int id, batches batches)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            if (id != batches.id)
            {
                return BadRequest();
            }

            db.Entry(batches).State = System.Data.Entity.EntityState.Modified;
            db.SaveChanges();

            return StatusCode(HttpStatusCode.NoContent);
        }

        // DELETE: api/batches/5
        [ResponseType(typeof(batches))]
        public IHttpActionResult Deletebatches(int id)
        {
            batches batches = db.batches.Find(id);
            if (batches == null)
            {
                return NotFound();
            }

            db.batches.Remove(batches);
            db.SaveChanges();

            return Ok(batches);
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                db.Dispose();
            }
            base.Dispose(disposing);
        }

        private bool batchesExists(int id)
        {
            return db.batches.Count(e => e.id == id) > 0;
        }
    }
}