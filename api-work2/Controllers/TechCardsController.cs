using System.Linq;
using System.Web.Http;
using api_work2.Models;

namespace Api_work.Controllers
{
    [RoutePrefix("api/techcards")]
    public class TechCardsController : ApiController
    {
        private AgroControlEntities db = new AgroControlEntities();

        // GET: api/techcards
        [HttpGet]
        [Route("")]
        public IHttpActionResult GetTechCards()
        {
            var techCards = db.tech_cards
                .Select(tc => new
                {
                    tc.id,
                    tc.product_id,
                    product_name = tc.products.name,
                    tc.version,
                    tc.status,
                    tc.created_at,
                    step_count = tc.tech_card_steps.Count()
                })
                .ToList();

            return Ok(techCards);
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
                    tc.status
                })
                .ToList();

            return Ok(techCards);
        }
    }
}