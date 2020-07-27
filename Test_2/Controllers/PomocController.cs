using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace Test_2.Controllers
{
    public class PomocController : Controller
    {
        public ActionResult helpExplorator()
        {
            return View();
        }
        public ActionResult helpProfiles()
        {
            return View();
        }
    }
}