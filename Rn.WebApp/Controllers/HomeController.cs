using Rx.Filemanagers;
using System.Web.Mvc;

namespace Rn.WebApp.Controllers
{
    public class HomeController : Controller
    {
        public ActionResult Index()
        {
            return View();
        }

        public ActionResult FileManager(string id, Request request)
        {
            var fc = new FileContext();
            {
                fc.SetRequest(request);
                //fc.SetUserName(HttpContext.User.Identity.Name);

                if (fc.Content as byte[] == null)
                    return Content(fc.Content.ToString(), fc.Type);
                else
                    return File(fc.Content as byte[], fc.Type, request.fileName);
            }
        }
    }
}