﻿using Microsoft.AspNet.Identity.EntityFramework;
using System.Linq;
using System.Web.Mvc;
using Test_2.Models;

namespace Test_2.Controllers
{
    public class RoleController : Controller
    {
        
        ApplicationDbContext context;
        
        public RoleController()
        {
            context = new ApplicationDbContext();
        }
        
        public ActionResult Index()
        {
            var Roles = context.Roles.ToList();
            return View(Roles);
        }

        public ActionResult Create()
        {
            var Role = new IdentityRole();
            return View(Role);
        }


        [HttpPost]
        public ActionResult Create(IdentityRole Role)
        {
            context.Roles.Add(Role);
            context.SaveChanges();
            return RedirectToAction("Index");
        }
        
    }
}