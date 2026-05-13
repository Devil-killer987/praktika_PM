
using api_work2.Models;
using System;
using System.Linq;
using System.Net;
using System.Web.Http;
using System.Web.Http.Description;

namespace Api_work.Controllers
{
    [RoutePrefix("api/orders")]
    public class ProductionOrdersController : ApiController
    {
        private AgroControlEntities db = new AgroControlEntities();

        // GET: api/orders
        public IQueryable<production_orders> Getproduction_orders()
        {
            return db.production_orders;
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

        // GET: api/orders/5
        [ResponseType(typeof(production_orders))]
        public IHttpActionResult Getproduction_orders(int id)
        {
            production_orders production_orders = db.production_orders.Find(id);
            if (production_orders == null)
            {
                return NotFound();
            }

            return Ok(production_orders);
        }

        // GET: api/orders/5/details
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
                    product_id = o.recipes.product_id,
                    recipe_id = o.recipe_id,
                    recipe_version = o.recipes.version,
                    o.planned_quantity_kg,
                    o.status,
                    o.planned_start_date,
                    o.actual_start_date,
                    o.actual_end_date,
                    o.created_at,
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
            {
                return NotFound();
            }

            return Ok(order);
        }

        // POST: api/orders
        [ResponseType(typeof(production_orders))]
        public IHttpActionResult Postproduction_orders(production_orders production_orders)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            // Check if recipe exists and is active
            var recipe = db.recipes.Find(production_orders.recipe_id);
            if (recipe == null)
            {
                return BadRequest("Recipe not found");
            }

            if (recipe.status != "active")
            {
                return BadRequest("Cannot create order for inactive recipe");
            }

            // Generate order number
            var lastOrder = db.production_orders.OrderByDescending(o => o.id).FirstOrDefault();
            int newNumber = (lastOrder != null) ? lastOrder.id + 1 : 1;
            production_orders.order_number = $"PO-{DateTime.Now.Year}{newNumber:D4}";
            production_orders.status = "planned";
            production_orders.created_at = DateTime.Now;

            db.production_orders.Add(production_orders);
            db.SaveChanges();

            return CreatedAtRoute("DefaultApi", new { id = production_orders.id }, production_orders);
        }

        // POST: api/orders/5/start
        [HttpPost]
        [Route("{id}/start")]
        [ResponseType(typeof(object))]
        public IHttpActionResult StartOrder(int id)
        {
            var order = db.production_orders.Find(id);
            if (order == null)
            {
                return NotFound();
            }

            if (order.status != "planned")
            {
                return BadRequest("Order can only be started from planned status");
            }

            order.status = "in_progress";
            order.actual_start_date = DateTime.Now;

            // Create batch automatically
            var batch = new batches
            {
                batch_number = $"B-{order.order_number}-01",
                order_id = order.id,
                status = "planned",
                created_at = DateTime.Now
            };

            db.batches.Add(batch);
            db.SaveChanges();

            // Create batch steps from tech card
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
                status = order.status,
                batch_id = batch.id,
                batch_number = batch.batch_number,
                message = "Order started and batch created"
            });
        }

        // POST: api/orders/5/cancel
        [HttpPost]
        [Route("{id}/cancel")]
        [ResponseType(typeof(string))]
        public IHttpActionResult CancelOrder(int id)
        {
            var order = db.production_orders.Find(id);
            if (order == null)
            {
                return NotFound();
            }

            if (order.status == "completed")
            {
                return BadRequest("Cannot cancel completed order");
            }

            var runningBatches = order.batches.Any(b => b.status == "running");
            if (runningBatches)
            {
                return BadRequest("Cannot cancel order with running batches. Stop batches first.");
            }

            order.status = "cancelled";
            db.SaveChanges();

            return Ok($"Order {order.order_number} cancelled");
        }

        // PUT: api/orders/5
        [ResponseType(typeof(void))]
        public IHttpActionResult Putproduction_orders(int id, production_orders production_orders)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            if (id != production_orders.id)
            {
                return BadRequest();
            }

            var existing = db.production_orders.Find(id);
            if (existing.status == "completed" || existing.status == "cancelled")
            {
                return BadRequest("Cannot modify completed or cancelled order");
            }

            db.Entry(production_orders).State = System.Data.Entity.EntityState.Modified;
            db.SaveChanges();

            return StatusCode(HttpStatusCode.NoContent);
        }

        // DELETE: api/orders/5
        [ResponseType(typeof(production_orders))]
        public IHttpActionResult Deletedeleteproduction_orders(int id)
        {
            var order = db.production_orders.Find(id);
            if (order == null)
            {
                return NotFound();
            }

            if (order.status != "planned")
            {
                return BadRequest("Only planned orders can be deleted");
            }

            db.production_orders.Remove(order);
            db.SaveChanges();

            return Ok(order);
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                db.Dispose();
            }
            base.Dispose(disposing);
        }

        private bool production_ordersExists(int id)
        {
            return db.production_orders.Count(e => e.id == id) > 0;
        }
    }
}