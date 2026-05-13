using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System;
using System.Linq;
using System.Web.Http;
using api_work2.Models;


namespace api_work2.Controllers
{
    [RoutePrefix("api/orders")]
    public class OrdersController : ApiController
    {
        private AgroControlEntities db = new AgroControlEntities();

        // GET: api/orders
        [HttpGet]
        [Route("")]
        public IHttpActionResult GetOrders()
        {
            var orders = db.production_orders
                .Select(o => new
                {
                    o.id,
                    o.order_number,
                    o.recipe_id,
                    product_name = o.recipes.products.name,
                    o.planned_quantity_kg,
                    o.status,
                    o.planned_start_date,
                    o.created_at
                })
                .ToList();

            return Ok(orders);
        }

        // GET: api/orders/active
        [HttpGet]
        [Route("active")]
        public IHttpActionResult GetActiveOrders()
        {
            var orders = db.production_orders
                .Where(o => o.status == "in_progress" || o.status == "planned")
                .Select(o => new
                {
                    o.id,
                    o.order_number,
                    product_name = o.recipes.products.name,
                    o.planned_quantity_kg,
                    o.status,
                    o.planned_start_date,
                    has_active_batch = o.batches.Any(b => b.status == "running" || b.status == "planned")
                })
                .ToList();

            return Ok(orders);
        }

        // GET: api/orders/{id}
        [HttpGet]
        [Route("{id}")]
        public IHttpActionResult GetOrder(int id)
        {
            var order = db.production_orders
                .Where(o => o.id == id)
                .Select(o => new
                {
                    o.id,
                    o.order_number,
                    o.recipe_id,
                    product_name = o.recipes.products.name,
                    o.planned_quantity_kg,
                    o.status,
                    o.planned_start_date,
                    o.actual_start_date,
                    o.actual_end_date,
                    o.created_at
                })
                .FirstOrDefault();

            if (order == null)
                return NotFound();

            return Ok(order);
        }

        // GET: api/orders/{id}/details
        [HttpGet]
        [Route("{id}/details")]
        public IHttpActionResult GetOrderDetails(int id)
        {
            var order = db.production_orders
                .Where(o => o.id == id)
                .Select(o => new
                {
                    o.id,
                    o.order_number,
                    product_name = o.recipes.products.name,
                    o.planned_quantity_kg,
                    o.status,
                    o.planned_start_date,
                    batches = o.batches.Select(b => new
                    {
                        b.id,
                        b.batch_number,
                        b.status,
                        b.start_time,
                        b.end_time,
                        b.actual_quantity_kg,
                        b.deviation_count
                    })
                })
                .FirstOrDefault();

            if (order == null)
                return NotFound();

            return Ok(order);
        }

        // POST: api/orders
        [HttpPost]
        [Route("")]
        public IHttpActionResult CreateOrder([FromBody] CreateOrderRequest request)
        {
            if (request == null)
                return BadRequest("Request is null");

            var recipe = db.recipes.Find(request.recipe_id);
            if (recipe == null)
                return BadRequest("Recipe not found");

            var lastOrder = db.production_orders.OrderByDescending(o => o.id).FirstOrDefault();
            int newNumber = (lastOrder != null) ? lastOrder.id + 1 : 1;
            string orderNumber = $"PO-{DateTime.Now.Year}{newNumber:D4}";

            var order = new production_orders
            {
                order_number = orderNumber,
                recipe_id = request.recipe_id,
                planned_quantity_kg = request.planned_quantity_kg,
                status = "planned",
                planned_start_date = request.planned_start_date,
                created_at = DateTime.Now
            };

            db.production_orders.Add(order);
            db.SaveChanges();

            return Ok(new { id = order.id, order_number = order.order_number, message = "Order created" });
        }

        // POST: api/orders/{id}/start
        [HttpPost]
        [Route("{id}/start")]
        public IHttpActionResult StartOrder(int id)
        {
            var order = db.production_orders.Find(id);
            if (order == null)
                return NotFound();

            if (order.status != "planned")
                return BadRequest("Order can only be started from planned status");

            order.status = "in_progress";
            order.actual_start_date = DateTime.Now;

            // Создаем партию
            var lastBatch = db.batches.OrderByDescending(b => b.id).FirstOrDefault();
            int newBatchNum = (lastBatch != null) ? lastBatch.id + 1 : 1;
            string batchNumber = $"B-{DateTime.Now.Year}{newBatchNum:D4}";

            var batch = new batches
            {
                batch_number = batchNumber,
                order_id = order.id,
                status = "planned",
                created_at = DateTime.Now
            };

            db.batches.Add(batch);
            db.SaveChanges();

            // Создаем шаги партии из техкарты
            var productId = order.recipes.product_id;
            var techCard = db.tech_cards.FirstOrDefault(tc => tc.product_id == productId && tc.status == "active");

            if (techCard != null)
            {
                var steps = db.tech_card_steps.Where(ts => ts.card_id == techCard.id).OrderBy(ts => ts.step_order);
                foreach (var step in steps)
                {
                    var batchStep = new batch_steps
                    {
                        batch_id = batch.id,
                        step_id = step.id,
                        step_order = step.step_order,
                        step_name = step.step_name,
                        created_at = DateTime.Now
                    };
                    db.batch_steps.Add(batchStep);
                }
                db.SaveChanges();
            }

            return Ok(new
            {
                order_id = order.id,
                order_number = order.order_number,
                batch_id = batch.id,
                batch_number = batch.batch_number,
                message = "Order started"
            });
        }

        // PUT: api/orders/{id}
        [HttpPut]
        [Route("{id}")]
        public IHttpActionResult UpdateOrder(int id, [FromBody] UpdateOrderRequest request)
        {
            var order = db.production_orders.Find(id);
            if (order == null)
                return NotFound();

            if (order.status != "planned")
                return BadRequest("Only planned orders can be edited");

            order.recipe_id = request.recipe_id;
            order.planned_quantity_kg = request.planned_quantity_kg;
            order.planned_start_date = request.planned_start_date;

            db.SaveChanges();

            return Ok(new { message = "Order updated" });
        }

        // DELETE: api/orders/{id}
        [HttpDelete]
        [Route("{id}")]
        public IHttpActionResult DeleteOrder(int id)
        {
            var order = db.production_orders.Find(id);
            if (order == null)
                return NotFound();

            if (order.status != "planned")
                return BadRequest("Only planned orders can be deleted");

            db.production_orders.Remove(order);
            db.SaveChanges();

            return Ok(new { message = "Order deleted" });
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
    public class CreateOrderRequest
    {
        public int recipe_id { get; set; }
        public decimal planned_quantity_kg { get; set; }
        public DateTime planned_start_date { get; set; }
    }

    public class UpdateOrderRequest
    {
        public int recipe_id { get; set; }
        public decimal planned_quantity_kg { get; set; }
        public DateTime planned_start_date { get; set; }
    }
}