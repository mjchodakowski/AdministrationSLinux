using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace Test_2.Controllers
{
    public class MenuController : Controller
    {
        // GET: Menu
        public ActionResult Index()
        {
            return View();
        }

        public ActionResult MenadzerPlikow()
        {
            return RedirectToAction("About","ChangeFile");
        }

        public ActionResult ConfigAccount()
        {
            return RedirectToAction("Index","configAccount");
        }





    }
}