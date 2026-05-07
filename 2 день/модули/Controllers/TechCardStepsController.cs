
using api_work2.Models;
using System.Linq;
using System.Net;
using System.Web.Http;
using System.Web.Http.Description;

namespace Api_work.Controllers
{
    [RoutePrefix("api/techcardsteps")]
    public class TechCardStepsController : ApiController
    {
        private AgroControlEntities db = new AgroControlEntities();

        // GET: api/techcardsteps
        public IQueryable<tech_card_steps> Gettech_card_steps()
        {
            return db.tech_card_steps;
        }

        // GET: api/techcardsteps/bycard/5
        [HttpGet]
        [Route("bycard/{cardId}")]
        public IHttpActionResult GetStepsByCard(int cardId)
        {
            var steps = db.tech_card_steps
                .Where(ts => ts.card_id == cardId)
                .OrderBy(ts => ts.step_order)
                .Select(ts => new
                {
                    ts.id,
                    ts.step_order,
                    ts.step_name,
                    ts.step_type,
                    ts.planned_temp_c,
                    ts.planned_duration_min,
                    ts.planned_pressure_bar,
                    ts.temp_tolerance_min,
                    ts.temp_tolerance_max,
                    ts.pressure_tolerance_min,
                    ts.pressure_tolerance_max,
                    ts.is_mandatory,
                    ts.instruction
                })
                .ToList();

            return Ok(steps);
        }

        // GET: api/techcardsteps/5
        [ResponseType(typeof(tech_card_steps))]
        public IHttpActionResult Gettech_card_steps(int id)
        {
            tech_card_steps tech_card_steps = db.tech_card_steps.Find(id);
            if (tech_card_steps == null)
            {
                return NotFound();
            }

            return Ok(tech_card_steps);
        }

        // POST: api/techcardsteps
        [ResponseType(typeof(tech_card_steps))]
        public IHttpActionResult Posttech_card_steps(tech_card_steps tech_card_steps)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var techCard = db.tech_cards.Find(tech_card_steps.card_id);
            if (techCard == null)
            {
                return BadRequest("Tech card not found");
            }

            if (techCard.status == "active")
            {
                return BadRequest("Cannot add steps to active tech card");
            }

            // Auto-assign step order if not provided
            if (tech_card_steps.step_order == 0)
            {
                var maxOrder = db.tech_card_steps
                    .Where(ts => ts.card_id == tech_card_steps.card_id)
                    .Select(ts => (int?)ts.step_order)
                    .Max() ?? 0;
                tech_card_steps.step_order = maxOrder + 1;
            }

            db.tech_card_steps.Add(tech_card_steps);
            db.SaveChanges();

            return CreatedAtRoute("DefaultApi", new { id = tech_card_steps.id }, tech_card_steps);
        }

        // PUT: api/techcardsteps/5
        [ResponseType(typeof(void))]
        public IHttpActionResult Puttech_card_steps(int id, tech_card_steps tech_card_steps)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            if (id != tech_card_steps.id)
            {
                return BadRequest();
            }

            var existing = db.tech_card_steps.Find(id);
            var techCard = db.tech_cards.Find(existing.card_id);
            if (techCard.status == "active")
            {
                return BadRequest("Cannot modify steps of active tech card");
            }

            db.Entry(tech_card_steps).State = System.Data.Entity.EntityState.Modified;
            db.SaveChanges();

            return StatusCode(HttpStatusCode.NoContent);
        }

        // DELETE: api/techcardsteps/5
        [ResponseType(typeof(tech_card_steps))]
        public IHttpActionResult Deletetech_card_steps(int id)
        {
            tech_card_steps tech_card_steps = db.tech_card_steps.Find(id);
            if (tech_card_steps == null)
            {
                return NotFound();
            }

            var techCard = db.tech_cards.Find(tech_card_steps.card_id);
            if (techCard.status == "active")
            {
                return BadRequest("Cannot delete steps from active tech card");
            }

            db.tech_card_steps.Remove(tech_card_steps);
            db.SaveChanges();

            return Ok(tech_card_steps);
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