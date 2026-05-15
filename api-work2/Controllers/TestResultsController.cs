using System;
using System.Linq;
using System.Web.Http;
using api_work2.Models;

namespace api_work2.Controllers
{
    [RoutePrefix("api/testresults")]
    public class TestResultsController : ApiController
    {
        private AgroControlEntities db = new AgroControlEntities();

        // GET: api/testresults
        [HttpGet]
        [Route("")]
        public IHttpActionResult GetAllTestResults()
        {
            var results = db.test_results
                .Select(tr => new
                {
                    tr.id,
                    tr.test_id,
                    tr.parameter_name,
                    tr.measured_value,
                    tr.standard_value,
                    tr.unit,
                    tr.result
                })
                .ToList();

            return Ok(results);
        }

        // GET: api/testresults/bytest/5
        [HttpGet]
        [Route("bytest/{testId}")]
        public IHttpActionResult GetResultsByTest(int testId)
        {
            var results = db.test_results
                .Where(tr => tr.test_id == testId)
                .Select(tr => new
                {
                    tr.id,
                    tr.parameter_name,
                    tr.measured_value,
                    tr.standard_value,
                    tr.unit,
                    tr.result
                })
                .ToList();

            return Ok(results);
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

        // GET: api/testresults/5
        [HttpGet]
        [Route("{id}")]
        public IHttpActionResult GetTestResultById(int id)
        {
            var result = db.test_results
                .Where(tr => tr.id == id)
                .Select(tr => new
                {
                    tr.id,
                    tr.test_id,
                    tr.parameter_name,
                    tr.measured_value,
                    tr.standard_value,
                    tr.unit,
                    tr.result
                })
                .FirstOrDefault();

            if (result == null)
            {
                return NotFound();
            }

            return Ok(result);
        }

        // POST: api/testresults/batch
        [HttpPost]
        [Route("batch")]
        public IHttpActionResult AddBatchResults([FromBody] BatchResultsRequest request)
        {
            if (request == null)
            {
                return BadRequest("Request is null");
            }

            // Проверяем, существует ли тест
            var test = db.quality_tests.Find(request.TestId);
            if (test == null)
            {
                return NotFound();
            }

            // Добавляем результаты
            foreach (var item in request.Results)
            {
                var testResult = new test_results
                {
                    test_id = request.TestId,
                    parameter_name = item.ParameterName,
                    measured_value = item.MeasuredValue,
                    standard_value = item.StandardValue,
                    unit = item.Unit,
                    result = item.Result
                };
                db.test_results.Add(testResult);
            }

            // Обновляем комментарий к тесту
            if (!string.IsNullOrEmpty(request.Comment))
            {
                test.analyst_comment = request.Comment;
            }

            db.SaveChanges();

            int resultsCount = request.Results.Length;
            return Ok(new { count = resultsCount, message = $"{resultsCount} results added to test {request.TestId}" });
        }

        // POST: api/testresults
        [HttpPost]
        [Route("")]
        public IHttpActionResult CreateTestResult([FromBody] test_results testResult)
        {
            if (testResult == null)
            {
                return BadRequest("Request is null");
            }

            db.test_results.Add(testResult);
            db.SaveChanges();

            return Ok(new { id = testResult.id, message = "Test result created" });
        }

        // PUT: api/testresults/5
        [HttpPut]
        [Route("{id}")]
        public IHttpActionResult UpdateTestResult(int id, [FromBody] test_results testResult)
        {
            if (testResult == null)
            {
                return BadRequest("Request is null");
            }

            var existing = db.test_results.Find(id);
            if (existing == null)
            {
                return NotFound();
            }

            existing.parameter_name = testResult.parameter_name;
            existing.measured_value = testResult.measured_value;
            existing.standard_value = testResult.standard_value;
            existing.unit = testResult.unit;
            existing.result = testResult.result;

            db.SaveChanges();

            return Ok(new { message = "Test result updated" });
        }

        // DELETE: api/testresults/5
        [HttpDelete]
        [Route("{id}")]
        public IHttpActionResult DeleteTestResult(int id)
        {
            var testResult = db.test_results.Find(id);
            if (testResult == null)
            {
                return NotFound();
            }

            db.test_results.Remove(testResult);
            db.SaveChanges();

            return Ok(new { message = "Test result deleted" });
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
    public class BatchResultsRequest
    {
        public int TestId { get; set; }
        public string Comment { get; set; }
        public TestResultItem[] Results { get; set; }
    }

    public class TestResultItem
    {
        public string ParameterName { get; set; }
        public string MeasuredValue { get; set; }
        public string StandardValue { get; set; }
        public string Unit { get; set; }
        public string Result { get; set; }
    }
}