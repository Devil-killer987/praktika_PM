
using api_work2.Models;
using System;
using System.Linq;
using System.Net;
using System.Web.Http;
using System.Web.Http.Description;

namespace Api_work.Controllers
{
    public class ProductsController : ApiController
    {
        private AgroControlEntities db = new AgroControlEntities();

        // GET: api/products
        public IQueryable<products> Getproducts()
        {
            return db.products;
        }

        // GET: api/products/active
        [HttpGet]
        [Route("api/products/active")]
        public IHttpActionResult GetActiveProducts()
        {
            var products = db.products.Where(p => p.status == "active")
                .Select(p => new
                {
                    p.id,
                    p.code,
                    p.name,
                    p.product_type,
                    p.release_form,
                    active_recipe = p.recipes.FirstOrDefault(r => r.status == "active"),
                    active_tech_card = p.tech_cards.FirstOrDefault(tc => tc.status == "active")
                })
                .ToList();

            return Ok(products);
        }

        // GET: api/products/5
        [ResponseType(typeof(products))]
        public IHttpActionResult Getproducts(int id)
        {
            products products = db.products.Find(id);
            if (products == null)
            {
                return NotFound();
            }

            return Ok(products);
        }

        // GET: api/products/5/recipes
        [HttpGet]
        [Route("api/products/{id}/recipes")]
        public IHttpActionResult GetProductRecipes(int id)
        {
            var recipes = db.recipes.Where(r => r.product_id == id)
                .Select(r => new
                {
                    r.id,
                    r.version,
                    r.status,
                    r.sum_percentage,
                    r.created_at,
                    r.approved_at,
                    component_count = r.recipe_components.Count()
                })
                .ToList();

            return Ok(recipes);
        }

        // GET: api/products/5/techcards
        [HttpGet]
        [Route("api/products/{id}/techcards")]
        public IHttpActionResult GetProductTechCards(int id)
        {
            var techCards = db.tech_cards.Where(tc => tc.product_id == id)
                .Select(tc => new
                {
                    tc.id,
                    tc.version,
                    tc.status,
                    tc.created_at,
                    tc.approved_at,
                    step_count = tc.tech_card_steps.Count()
                })
                .ToList();

            return Ok(techCards);
        }

        // POST: api/products
        [ResponseType(typeof(products))]
        public IHttpActionResult Postproducts(products products)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            if (db.products.Any(p => p.code == products.code))
            {
                return BadRequest("Product code already exists");
            }

            products.created_at = DateTime.Now;
            products.updated_at = DateTime.Now;
            products.status = "draft";
            db.products.Add(products);
            db.SaveChanges();

            return CreatedAtRoute("DefaultApi", new { id = products.id }, products);
        }

        // PUT: api/products/5
        [ResponseType(typeof(void))]
        public IHttpActionResult Putproducts(int id, products products)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            if (id != products.id)
            {
                return BadRequest();
            }

            products.updated_at = DateTime.Now;
            db.Entry(products).State = System.Data.Entity.EntityState.Modified;
            db.SaveChanges();

            return StatusCode(HttpStatusCode.NoContent);
        }

        // DELETE: api/products/5
        [ResponseType(typeof(products))]
        public IHttpActionResult Deleteproducts(int id)
        {
            products products = db.products.Find(id);
            if (products == null)
            {
                return NotFound();
            }

            // Soft delete - change status to archived
            products.status = "archived";
            db.SaveChanges();

            return Ok(products);
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                db.Dispose();
            }
            base.Dispose(disposing);
        }

        private bool productsExists(int id)
        {
            return db.products.Count(e => e.id == id) > 0;
        }
    }
}