
using api_work2.Models;
using System;
using System.Linq;
using System.Net;
using System.Web.Http;
using System.Web.Http.Description;

namespace Api_work.Controllers
{
    [RoutePrefix("api/techcards")]
    public class TechCardsController : ApiController
    {
        private AgroControlEntities db = new AgroControlEntities();

        // GET: api/techcards
        public IQueryable<tech_cards> Gettech_cards()
        {
            return db.tech_cards;
        }

        // GET: api/techcards/active
        [HttpGet]
        [Route("active")]
        public IHttpActionResult GetActiveTechCards()
        {
            var techCards = db.tech_cards
                .Where(tc => tc.status == "active")
                .Select(tc => new
                {
                    tc.id,
                    product_name = tc.products.name,
                    tc.version,
                    tc.status,
                    step_count = tc.tech_card_steps.Count()
                })
                .ToList();

            return Ok(techCards);
        }

        // GET: api/techcards/5
        [ResponseType(typeof(tech_cards))]
        public IHttpActionResult Gettech_cards(int id)
        {
            tech_cards tech_cards = db.tech_cards.Find(id);
            if (tech_cards == null)
            {
                return NotFound();
            }

            return Ok(tech_cards);
        }

        // GET: api/techcards/5/details
        [HttpGet]
        [Route("{id}/details")]
        public IHttpActionResult GetTechCardDetails(int id)
        {
            var techCard = db.tech_cards
                .Where(tc => tc.id == id)
                .Select(tc => new
                {
                    tc.id,
                    product_name = tc.products.name,
                    product_id = tc.product_id,
                    tc.version,
                    tc.status,
                    tc.created_at,
                    tc.approved_at,
                    steps = tc.tech_card_steps
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
                })
                .FirstOrDefault();

            if (techCard == null)
            {
                return NotFound();
            }

            return Ok(techCard);
        }

        // POST: api/techcards
        [ResponseType(typeof(tech_cards))]
        public IHttpActionResult Posttech_cards(tech_cards tech_cards)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            // Check if product exists
            if (!db.products.Any(p => p.id == tech_cards.product_id))
            {
                return BadRequest("Product not found");
            }

            // Get next version number
            var maxVersion = db.tech_cards
                .Where(tc => tc.product_id == tech_cards.product_id)
                .Select(tc => (int?)tc.version)
                .Max() ?? 0;
            tech_cards.version = maxVersion + 1;
            tech_cards.status = "draft";
            tech_cards.created_at = DateTime.Now;

            db.tech_cards.Add(tech_cards);
            db.SaveChanges();

            return CreatedAtRoute("DefaultApi", new { id = tech_cards.id }, tech_cards);
        }

        // POST: api/techcards/5/activate
        [HttpPost]
        [Route("{id}/activate")]
        [ResponseType(typeof(string))]
        public IHttpActionResult ActivateTechCard(int id, [FromBody] ActivateTechCardRequest request)
        {
            var techCard = db.tech_cards.Find(id);
            if (techCard == null)
            {
                return NotFound();
            }

            if (techCard.status == "active")
            {
                return BadRequest("Tech card is already active");
            }

            // Check if has at least one step
            var stepCount = db.tech_card_steps.Count(ts => ts.card_id == id);
            if (stepCount == 0)
            {
                return BadRequest("Cannot activate tech card with no steps");
            }

            // Deactivate other versions
            var activeCards = db.tech_cards.Where(tc => tc.product_id == techCard.product_id && tc.status == "active");
            foreach (var activeCard in activeCards)
            {
                activeCard.status = "archived";
            }

            techCard.status = "active";
            techCard.approved_by = request?.ApprovedBy;
            techCard.approved_at = DateTime.Now;
            db.SaveChanges();

            return Ok($"Tech card version {techCard.version} activated");
        }

        // PUT: api/techcards/5
        [ResponseType(typeof(void))]
        public IHttpActionResult Puttech_cards(int id, tech_cards tech_cards)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            if (id != tech_cards.id)
            {
                return BadRequest();
            }

            var existing = db.tech_cards.Find(id);
            if (existing.status == "active")
            {
                return BadRequest("Cannot edit active tech card. Create new version instead.");
            }

            db.Entry(tech_cards).State = System.Data.Entity.EntityState.Modified;
            db.SaveChanges();

            return StatusCode(HttpStatusCode.NoContent);
        }

        // DELETE: api/techcards/5
        [ResponseType(typeof(tech_cards))]
        public IHttpActionResult Deletetech_cards(int id)
        {
            tech_cards tech_cards = db.tech_cards.Find(id);
            if (tech_cards == null)
            {
                return NotFound();
            }

            if (tech_cards.status == "active")
            {
                return BadRequest("Cannot delete active tech card. Archive it instead.");
            }

            db.tech_cards.Remove(tech_cards);
            db.SaveChanges();

            return Ok(tech_cards);
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                db.Dispose();
            }
            base.Dispose(disposing);
        }

        private bool tech_cardsExists(int id)
        {
            return db.tech_cards.Count(e => e.id == id) > 0;
        }
    }

    public class ActivateTechCardRequest
    {
        public int? ApprovedBy { get; set; }
    }
}