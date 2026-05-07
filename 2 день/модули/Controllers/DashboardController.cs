using api_work2.Models;
using System;
using System.Linq;
using System.Web.Http;

namespace Api_work.Controllers
{
    [RoutePrefix("api/dashboard")]
    public class DashboardController : ApiController
    {
        private AgroControlEntities db = new AgroControlEntities();

        // GET: api/dashboard/stats
        [HttpGet]
        [Route("stats")]
        public IHttpActionResult GetDashboardStats()
        {
            var stats = new
            {
                active_products = db.products.Count(p => p.status == "active"),
                active_recipes = db.recipes.Count(r => r.status == "active"),
                active_tech_cards = db.tech_cards.Count(tc => tc.status == "active"),
                orders_in_progress = db.production_orders.Count(o => o.status == "in_progress"),
                batches_in_production = db.batches.Count(b => b.status == "running"),
                batches_with_deviations = db.batches.Count(b => (b.deviation_count ?? 0) > 0),
                pending_quality_tests = db.quality_tests.Count(q => q.status == "in_progress"),
                blocked_batches = db.batches.Count(b => b.status == "blocked")
            };

            return Ok(stats);
        }

        // GET: api/dashboard/recent-events
        [HttpGet]
        [Route("recent-events")]
        public IHttpActionResult GetRecentEvents(int count = 10)
        {
            // Combine different events
            var batchStarts = db.batches
                .Where(b => b.start_time != null)
                .OrderByDescending(b => b.start_time)
                .Take(count)
                .Select(b => new
                {
                    type = "batch_started",
                    batch_number = b.batch_number,
                    product_name = b.production_orders.recipes.products.name,
                    event_time = b.start_time,
                    message = $"Batch {b.batch_number} started"
                });

            var batchCompletions = db.batches
                .Where(b => b.end_time != null)
                .OrderByDescending(b => b.end_time)
                .Take(count)
                .Select(b => new
                {
                    type = "batch_completed",
                    batch_number = b.batch_number,
                    product_name = b.production_orders.recipes.products.name,
                    event_time = b.end_time,
                    message = $"Batch {b.batch_number} completed"
                });

            var deviations = db.deviation_events
                .OrderByDescending(d => d.created_at)
                .Take(count)
                .Select(d => new
                {
                    type = d.severity == "critical" ? "critical_deviation" : "deviation",
                    batch_number = d.batches.batch_number,
                    product_name = d.batches.production_orders.recipes.products.name,
                    event_time = d.created_at,
                    message = d.description
                });

            var qualityDecisions = db.quality_tests
                .Where(q => q.decision != null)
                .OrderByDescending(q => q.analysis_date)
                .Take(count)
                .Select(q => new
                {
                    type = q.decision == "approved" ? "quality_approved" : "quality_blocked",
                    batch_number = q.batches.batch_number,
                    product_name = q.batches.production_orders.recipes.products.name,
                    event_time = q.analysis_date,
                    message = $"Quality {q.decision} for batch {q.batches.batch_number}"
                });

            var allEvents = batchStarts
                .Union(batchCompletions)
                .Union(deviations)
                .Union(qualityDecisions)
                .OrderByDescending(e => e.event_time)
                .Take(count)
                .ToList();

            return Ok(allEvents);
        }

        // GET: api/dashboard/active-batches-summary
        [HttpGet]
        [Route("active-batches-summary")]
        public IHttpActionResult GetActiveBatchesSummary()
        {
            var activeBatches = db.batches
                .Where(b => b.status == "running")
                .Select(b => new
                {
                    b.id,
                    b.batch_number,
                    product_name = b.production_orders.recipes.products.name,
                    b.start_time,
                    current_step = b.batch_steps.FirstOrDefault(bs => bs.end_time == null).step_name,
                    completed_steps = b.batch_steps.Count(bs => bs.end_time != null),
                    total_steps = b.batch_steps.Count(),
                    has_deviation = (b.deviation_count ?? 0) > 0
                })
                .ToList();

            return Ok(activeBatches);
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
}