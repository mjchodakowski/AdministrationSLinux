using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Security;
using System.Web;
using System.Web.Mvc;
using Renci.SshNet.Sftp;
using Renci.SshNet;
using Test_2.Models;
using System.IO;
using Test_2.DAL;
using Microsoft.AspNet.Identity;

namespace Test_2.Controllers
{
    public class HomeController : Controller
    {
        public ActionResult Index()
        {
                return View();
        }
    }
}