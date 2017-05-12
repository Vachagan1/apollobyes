using System.Net;
using System.Web.Mvc;
using MvcAffableBean.Models;
using ProductStore.Controllers;

namespace MvcAffableBean.Controllers
{
    public class StoreFrontController : Controller
    {

        private ProductStoreContext db = new ProductStoreContext();
        // GET: StoreFront
        public ActionResult Index(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }

            Category category = db.Categories.Find(id);
            if (category == null)
            {
                return HttpNotFound();
            }
            return View(category);
        }
    }
}