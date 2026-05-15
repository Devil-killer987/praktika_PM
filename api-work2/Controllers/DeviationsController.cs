using System;
using System.Linq;
using System.Web.Http;

using api_work2.Models;

namespace Api_work.Controllers
{
    [RoutePrefix("api/deviations")]
    public class DeviationsController : ApiController
    {
        private AgroControlEntities db = new AgroControlEntities();

        // GET: api/deviations
        [HttpGet]
        [Route("")]
        public IHttpActionResult GetAllDeviations()
        {
            var deviations = db.deviation_events
                .Select(d => new
                {
                    d.id,
                    d.batch_id,
                    batch_number = d.batches.batch_number,
                    d.step_id,
                    d.deviation_type,
                    d.description,
                    d.severity,
                    d.resolved,
                    d.created_at
                })
                .ToList();

            return Ok(deviations);
        }

        // GET: api/deviations/batch/{batchId}
        [HttpGet]
        [Route("batch/{batchId}")]
        public IHttpActionResult GetDeviationsByBatch(int batchId)
        {
            var deviations = db.deviation_events
                .Where(d => d.batch_id == batchId)
                .Select(d => new
                {
                    d.id,
                    d.deviation_type,
                    d.description,
                    d.severity,
                    d.resolved,
                    d.created_at
                })
                .ToList();

            return Ok(deviations);
        }

        // POST: api/deviations/report
        [HttpPost]
        [Route("report")]
        public IHttpActionResult ReportProblem([FromBody] ReportProblemRequest request)
        {
            if (request == null)
                return BadRequest("Request is null");

            var deviation = new deviation_events
            {
                batch_id = request.batch_id,
                deviation_type = request.deviation_type,
                description = request.description,
                severity = request.severity ?? "critical",
                created_at = DateTime.Now
            };

            db.deviation_events.Add(deviation);
            db.SaveChanges();

            return Ok(new { message = "Problem reported", id = deviation.id });
        }

        // POST: api/deviations/{id}/resolve
        [HttpPost]
        [Route("{id}/resolve")]
        public IHttpActionResult ResolveDeviation(int id, [FromBody] ResolveDeviationRequest request)
        {
            var deviation = db.deviation_events.Find(id);
            if (deviation == null)
                return NotFound();

            deviation.resolved = true;
            deviation.resolved_at = DateTime.Now;
            deviation.resolved_by = request?.ResolvedBy;

            db.SaveChanges();

            return Ok(new { message = "Deviation resolved", id = deviation.id });
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

    public class ReportProblemRequest
    {
        public int batch_id { get; set; }
        public string deviation_type { get; set; }
        public string description { get; set; }
        public string severity { get; set; }
    }

    public class ResolveDeviationRequest
    {
        public int? ResolvedBy { get; set; }
    }
}