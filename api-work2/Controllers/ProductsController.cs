using System.Linq;
using System.Web.Http;
using api_work2.Models;

namespace Api_work.Controllers
{
    [RoutePrefix("api/products")]
    public class ProductsController : ApiController
    {
        private AgroControlEntities db = new AgroControlEntities();

        // GET: api/products
        [HttpGet]
        [Route("")]
        public IHttpActionResult GetProducts()
        {
            var products = db.products
                .Select(p => new
                {
                    p.id,
                    p.code,
                    p.name,
                    p.product_type,
                    p.release_form,
                    p.status,
                    p.created_at
                })
                .ToList();

            return Ok(products);
        }

        // GET: api/products/active
        [HttpGet]
        [Route("active")]
        public IHttpActionResult GetActiveProducts()
        {
            var products = db.products
                .Where(p => p.status == "active")
                .Select(p => new
                {
                    p.id,
                    p.code,
                    p.name,
                    p.product_type,
                    p.release_form
                })
                .ToList();

            return Ok(products);
        }
    }
}