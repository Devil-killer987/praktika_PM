
using api_work2.Models;
using System;
using System.Linq;
using System.Net;
using System.Web.Http;
using System.Web.Http.Description;

namespace Api_work.Controllers
{
    [RoutePrefix("api/qualitytests")]
    public class QualityTestsController : ApiController
    {
        private AgroControlEntities db = new AgroControlEntities();

        // GET: api/qualitytests
        public IQueryable<quality_tests> Getquality_tests()
        {
            return db.quality_tests;
        }

        // GET: api/qualitytests/pending
        [HttpGet]
        [Route("pending")]
        public IHttpActionResult GetPendingTests()
        {
            var tests = db.quality_tests
                .Where(qt => qt.status == "in_progress")
                .Select(qt => new
                {
                    qt.id,
                    qt.batch_id,
                    batch_number = qt.batches.batch_number,
                    qt.material_id,
                    qt.sample_type,
                    qt.analysis_date,
                    qt.status
                })
                .ToList();

            return Ok(tests);
        }

        // GET: api/qualitytests/batch/5
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

        // GET: api/qualitytests/5
        [ResponseType(typeof(quality_tests))]
        public IHttpActionResult Getquality_tests(int id)
        {
            quality_tests quality_tests = db.quality_tests.Find(id);
            if (quality_tests == null)
            {
                return NotFound();
            }

            return Ok(quality_tests);
        }

        // POST: api/qualitytests
        [ResponseType(typeof(quality_tests))]
        public IHttpActionResult Postquality_tests([FromBody] quality_tests quality_tests)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            quality_tests.analysis_date = DateTime.Now;
            quality_tests.status = "in_progress";
            db.quality_tests.Add(quality_tests);
            db.SaveChanges();

            return CreatedAtRoute("DefaultApi", new { id = quality_tests.id }, quality_tests);
        }

        // POST: api/qualitytests/create
        [HttpPost]
        [Route("create")]
        [ResponseType(typeof(quality_tests))]
        public IHttpActionResult CreateTest([FromBody] CreateTestRequest request)
        {
            if (request == null)
            {
                return BadRequest("Request cannot be null");
            }

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

            return Ok(new { id = test.id, message = "Test created successfully" });
        }

        // POST: api/qualitytests/5/complete
        [HttpPost]
        [Route("{id}/complete")]
        [ResponseType(typeof(string))]
        public IHttpActionResult CompleteTest(int id, [FromBody] CompleteTestRequest request)
        {
            var test = db.quality_tests.Find(id);
            if (test == null)
            {
                return NotFound();
            }

            test.status = "completed";
            if (request != null)
            {
                test.analyst_comment = request.Comment;
            }
            db.SaveChanges();

            return Ok($"Test {id} completed");
        }

        // POST: api/qualitytests/5/decision
        [HttpPost]
        [Route("{id}/decision")]
        [ResponseType(typeof(string))]
        public IHttpActionResult MakeDecision(int id, [FromBody] DecisionRequest request)
        {
            if (request == null)
            {
                return BadRequest("Request cannot be null");
            }

            var test = db.quality_tests.Find(id);
            if (test == null)
            {
                return NotFound();
            }

            test.decision = request.Decision;
            test.analyst_comment = request.Comment;
            test.status = "completed";
            db.SaveChanges();

            // Update batch status based on decision
            if (test.batch_id.HasValue)
            {
                var batch = db.batches.Find(test.batch_id.Value);
                if (batch != null)
                {
                    if (request.Decision == "approved")
                    {
                        batch.status = "completed";
                        batch.end_time = DateTime.Now;
                    }
                    else if (request.Decision == "blocked")
                    {
                        batch.status = "blocked";
                    }
                    db.SaveChanges();
                }
            }

            return Ok(new { decision = request.Decision, message = $"Decision: {request.Decision}" });
        }

        // PUT: api/qualitytests/5
        [ResponseType(typeof(void))]
        public IHttpActionResult Putquality_tests(int id, quality_tests quality_tests)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            if (id != quality_tests.id)
            {
                return BadRequest();
            }

            db.Entry(quality_tests).State = System.Data.Entity.EntityState.Modified;
            db.SaveChanges();

            return StatusCode(HttpStatusCode.NoContent);
        }

        // DELETE: api/qualitytests/5
        [ResponseType(typeof(quality_tests))]
        public IHttpActionResult Deletequality_tests(int id)
        {
            quality_tests quality_tests = db.quality_tests.Find(id);
            if (quality_tests == null)
            {
                return NotFound();
            }

            db.quality_tests.Remove(quality_tests);
            db.SaveChanges();

            return Ok(quality_tests);
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
    public class CreateTestRequest
    {
        public int? BatchId { get; set; }
        public int? MaterialId { get; set; }
        public string SampleType { get; set; }
    }

    public class CompleteTestRequest
    {
        public string Comment { get; set; }
    }

    public class DecisionRequest
    {
        public string Decision { get; set; } // approved, blocked
        public string Comment { get; set; }
    }
}