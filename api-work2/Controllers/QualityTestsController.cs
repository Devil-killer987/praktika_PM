using System;
using System.Linq;
using System.Web.Http;
using api_work2.Models;

namespace Api_work.Controllers
{
    [RoutePrefix("api/qualitytests")]
    public class QualityTestsController : ApiController
    {
        private AgroControlEntities db = new AgroControlEntities();

        // GET: api/qualitytests/pending?type=raw_material
        [HttpGet]
        [Route("pending")]
        public IHttpActionResult GetPendingTests(string type = null)
        {
            if (type == "raw_material")
            {
                // Возвращаем партии сырья, требующие контроля
                var materials = db.batch_materials
                    .Where(bm => !db.quality_tests.Any(qt => qt.material_id == bm.material_id && qt.status == "completed"))
                    .Select(bm => new
                    {
                        id = bm.material_id,
                        batch_number = bm.lot_number,
                        material_name = bm.materials.name,
                        supplier = bm.materials.supplier,
                        receipt_date = DateTime.Now,
                        quantity = bm.quantity_used_kg,
                        test_status = "pending"
                    })
                    .Distinct()
                    .ToList();

                return Ok(materials);
            }
            else if (type == "finished_product")
            {
                // Возвращаем партии готовой продукции, требующие контроля
                var batches = db.batches
                    .Where(b => b.status == "completed" && !db.quality_tests.Any(qt => qt.batch_id == b.id))
                    .Select(b => new
                    {
                        b.id,
                        b.batch_number,
                        product_name = b.production_orders.recipes.products.name,
                        order_number = b.production_orders.order_number,
                        production_date = b.end_time,
                        quantity = b.actual_quantity_kg,
                        quality_status = "pending"
                    })
                    .ToList();

                return Ok(batches);
            }

            // Все испытания
            var allTests = db.quality_tests
                .Select(qt => new
                {
                    qt.id,
                    qt.sample_type,
                    qt.analysis_date,
                    qt.status,
                    qt.decision,
                    object_name = qt.batch_id != null ? qt.batches.batch_number : "Сырье"
                })
                .ToList();

            return Ok(allTests);
        }

        // GET: api/qualitytests/{id}
        [HttpGet]
        [Route("{id}")]
        public IHttpActionResult GetTestById(int id)
        {
            var test = db.quality_tests
                .Where(qt => qt.id == id)
                .Select(qt => new
                {
                    qt.id,
                    qt.sample_type,
                    qt.analysis_date,
                    qt.status,
                    qt.decision,
                    qt.analyst_comment,
                    batch_number = qt.batch_id != null ? qt.batches.batch_number : null,
                    material_name = qt.material_id != null ? "Материал" : null
                })
                .FirstOrDefault();

            if (test == null)
                return NotFound();

            return Ok(test);
        }

        // GET: api/qualitytests/batch/{batchId}
        [HttpGet]
        [Route("batch/{batchId}")]
        public IHttpActionResult GetTestsByBatch(int batchId)
        {
            var tests = db.quality_tests
                .Where(qt => qt.batch_id == batchId)
                .Select(qt => new
                {
                    qt.id,
                    qt.sample_type,
                    qt.analysis_date,
                    qt.status,
                    qt.decision,
                    qt.analyst_comment,
                    results = qt.test_results.Select(tr => new
                    {
                        tr.parameter_name,
                        tr.measured_value,
                        tr.standard_value,
                        tr.unit,
                        tr.result
                    })
                })
                .ToList();

            return Ok(tests);
        }

        // POST: api/qualitytests/create
        [HttpPost]
        [Route("create")]
        public IHttpActionResult CreateTest([FromBody] CreateTestRequest request)
        {
            if (request == null)
                return BadRequest("Request is null");

            var test = new quality_tests
            {
                batch_id = request.BatchId,
                material_id = request.MaterialId,
                sample_type = request.SampleType,
                analysis_date = DateTime.Now,
                status = "in_progress"
            };

            db.quality_tests.Add(test);
            db.SaveChanges();

            return Ok(new { id = test.id, message = "Test created" });
        }

        // POST: api/qualitytests/{id}/decision
        [HttpPost]
        [Route("{id}/decision")]
        public IHttpActionResult MakeDecision(int id, [FromBody] DecisionRequest request)
        {
            if (request == null)
                return BadRequest("Request is null");

            var test = db.quality_tests.Find(id);
            if (test == null)
                return NotFound();

            test.decision = request.Decision;
            test.analyst_comment = request.Comment;
            test.status = "completed";
            db.SaveChanges();

            // Обновляем статус партии если есть
            if (test.batch_id.HasValue)
            {
                var batch = db.batches.Find(test.batch_id.Value);
                if (batch != null)
                {
                    batch.status = request.Decision == "approved" ? "completed" : "blocked";
                    db.SaveChanges();
                }
            }

            return Ok(new { message = $"Decision: {request.Decision}" });
        }

        // GET: api/qualitytests
        [HttpGet]
        [Route("")]
        public IHttpActionResult GetAllTests()
        {
            var tests = db.quality_tests
                .OrderByDescending(qt => qt.analysis_date)
                .Select(qt => new
                {
                    qt.id,
                    qt.sample_type,
                    qt.analysis_date,
                    qt.status,
                    qt.decision,
                    object_name = qt.batch_id != null ? qt.batches.batch_number : "Сырье"
                })
                .ToList();

            return Ok(tests);
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

    public class CreateTestRequest
    {
        public int? BatchId { get; set; }
        public int? MaterialId { get; set; }
        public string SampleType { get; set; }
    }

    public class DecisionRequest
    {
        public string Decision { get; set; }
        public string Comment { get; set; }
    }
}