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
        public IHttpActionResult GetBatches()
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
                    b.status,
                    b.start_time,
                    b.deviation_count
                })
                .ToList();

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
    }
}