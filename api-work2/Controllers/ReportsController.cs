using api_work2.Models;
using System;
using System.Linq;
using System.Web.Http;
using System.Web.Http.Description;

namespace Api_work.Controllers
{
    [RoutePrefix("api/reports")]
    public class ReportsController : ApiController
    {
        private AgroControlEntities db = new AgroControlEntities();   

        // GET: api/reports/batches
        [HttpGet]
        [Route("batches")]
        public IHttpActionResult GetBatchReport(DateTime? fromDate = null, DateTime? toDate = null, string status = null)
        {
            var query = db.batches.AsQueryable();

            if (fromDate.HasValue)
                query = query.Where(b => b.start_time >= fromDate.Value);
            if (toDate.HasValue)
                query = query.Where(b => b.start_time <= toDate.Value);
            if (!string.IsNullOrEmpty(status))
                query = query.Where(b => b.status == status);

            var report = query
                .Select(b => new
                {
                    b.id,
                    b.batch_number,
                    order_number = b.production_orders.order_number,
                    product_name = b.production_orders.recipes.products.name,
                    b.status,
                    b.start_time,
                    b.end_time,
                    b.actual_quantity_kg,
                    planned_quantity = b.production_orders.planned_quantity_kg,
                    b.deviation_count,
                    quality_decision = b.quality_tests.FirstOrDefault(qt => qt.sample_type == "finished_product").decision,
                    step_count = b.batch_steps.Count(),
                    completed_steps = b.batch_steps.Count(bs => bs.end_time != null)
                })
                .ToList();

            return Ok(report);
        }

        // GET: api/reports/deviations
        [HttpGet]
        [Route("deviations")]
        public IHttpActionResult GetDeviationReport(DateTime? fromDate = null, DateTime? toDate = null, string severity = null)
        {
            var query = db.deviation_events.AsQueryable();

            if (fromDate.HasValue)
                query = query.Where(d => d.created_at >= fromDate.Value);
            if (toDate.HasValue)
                query = query.Where(d => d.created_at <= toDate.Value);
            if (!string.IsNullOrEmpty(severity))
                query = query.Where(d => d.severity == severity);

            var report = query
                .Select(d => new
                {
                    d.id,
                    batch_number = d.batches.batch_number,
                    product_name = d.batches.production_orders.recipes.products.name,
                    step_name = d.batch_steps.step_name,
                    d.deviation_type,
                    d.description,
                    d.severity,
                    d.created_at,
                    d.resolved,
                    d.resolved_at
                })
                .ToList();

            return Ok(report);
        }

        // GET: api/reports/quality
        [HttpGet]
        [Route("quality")]
        public IHttpActionResult GetQualityReport(DateTime? fromDate = null, DateTime? toDate = null)
        {
            var query = db.quality_tests.AsQueryable();

            if (fromDate.HasValue)
                query = query.Where(q => q.analysis_date >= fromDate.Value);
            if (toDate.HasValue)
                query = query.Where(q => q.analysis_date <= toDate.Value);

            var report = query
                .Where(q => q.sample_type == "finished_product")
                .Select(q => new
                {
                    q.id,
                    batch_number = q.batches.batch_number,
                    product_name = q.batches.production_orders.recipes.products.name,
                    q.analysis_date,
                    q.decision,
                    q.analyst_comment,
                    test_results = q.test_results.Select(tr => new
                    {
                        tr.parameter_name,
                        tr.measured_value,
                        tr.standard_value,
                        tr.unit,
                        tr.result
                    })
                })
                .ToList();

            int approvedCount = report.Count(r => r.decision == "approved");
            int blockedCount = report.Count(r => r.decision == "blocked");

            return Ok(new
            {
                total = report.Count,
                approved = approvedCount,
                blocked = blockedCount,
                approval_rate = report.Count > 0 ? (double)approvedCount / report.Count * 100 : 0,
                tests = report
            });
        }

        // GET: api/reports/equipment
        [HttpGet]
        [Route("equipment")]
        public IHttpActionResult GetEquipmentReport(DateTime? fromDate = null, DateTime? toDate = null)
        {
            // Based on batch_steps data grouped by equipment/line
            var report = db.batch_steps
                .Where(bs => (fromDate == null || bs.created_at >= fromDate) && (toDate == null || bs.created_at <= toDate))
                .GroupBy(bs => bs.batches.production_orders.recipes.products.name)
                .Select(g => new
                {
                    product = g.Key,
                    total_steps = g.Count(),
                    completed_steps = g.Count(bs => bs.end_time != null),
                    deviations = g.Count(bs => bs.deviation_flag == true),
                    avg_temp = g.Average(bs => bs.actual_temp_c),
                    avg_pressure = g.Average(bs => bs.actual_pressure_bar)
                })
                .ToList();

            return Ok(report);
        }

        // GET: api/reports/recipes-usage
        [HttpGet]
        [Route("recipes-usage")]
        public IHttpActionResult GetRecipesUsageReport(DateTime? fromDate = null, DateTime? toDate = null)
        {
            var query = db.production_orders.AsQueryable();

            if (fromDate.HasValue)
                query = query.Where(o => o.planned_start_date >= fromDate.Value);
            if (toDate.HasValue)
                query = query.Where(o => o.planned_start_date <= toDate.Value);

            var report = query
                .GroupBy(o => new { o.recipe_id, product_name = o.recipes.products.name, recipe_version = o.recipes.version })
                .Select(g => new
                {
                    recipe_id = g.Key.recipe_id,
                    product_name = g.Key.product_name,
                    version = g.Key.recipe_version,
                    order_count = g.Count(),
                    total_planned_quantity = g.Sum(o => o.planned_quantity_kg),
                    completed_orders = g.Count(o => o.status == "completed"),
                    batch_count = g.SelectMany(o => o.batches).Count(),
                    total_actual_quantity = g.SelectMany(o => o.batches).Sum(b => b.actual_quantity_kg) ?? 0
                })
                .ToList();

            return Ok(report);
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