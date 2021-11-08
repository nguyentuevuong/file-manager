using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using RN.FileManagers.NetCore;

namespace RN.Web.NetCore.Controllers
{
    public class HomeController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }

        public IActionResult FileManager(Request request)
        {
            var fc = FileContext.Current;
            {
                fc.SetRequest(request);
                fc.SetUserName(HttpContext.User.Identity.Name);

                if (fc.Content as byte[] == null)
                    return Content(fc.Content.ToString(), fc.Type);
                else
                    return File(fc.Content as byte[], fc.Type, request.fileName);
            }
        }
    }
}
