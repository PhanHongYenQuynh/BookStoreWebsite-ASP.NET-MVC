using BookGrotto.Models;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.EntityFramework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;
using Microsoft.AspNet.Identity.Owin;

namespace BookGrotto.Areas.Admin.Controllers
{
    [Authorize(Roles = "Admin")]
    public class RoleController : Controller
    {
        private ApplicationDbContext db = new ApplicationDbContext();
        // GET: Admin/Role
        public ActionResult Index()
        {
            var items=db.Roles.ToList();
            return View(items);
        }

        public ActionResult Create()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create(IdentityRole model)
        {
            if (ModelState.IsValid)
            {
                var roleManager = new RoleManager<IdentityRole>(new RoleStore<IdentityRole>(db));
                roleManager.Create(model);
                return RedirectToAction("Index");
            }
            return View(model);
        }

        public ActionResult Edit(string id)
        {
            var item = db.Roles.Find(id);
            return View(item);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Edit(string id, IdentityRole model)
        {
            if (ModelState.IsValid)
            {
                var roleManager = new RoleManager<IdentityRole>(new RoleStore<IdentityRole>(db));
                var role = await roleManager.FindByIdAsync(id);
                if (role == null)
                {
                    return HttpNotFound();
                }
                role.Name = model.Name;
                var result = await roleManager.UpdateAsync(role);
                if (result.Succeeded)
                {
                    return RedirectToAction("Index");
                }
                else
                {
                    ModelState.AddModelError("", "Không thể cập nhật quyền!.");
                }
            }
            return View(model);
        }

        //Fix This code Public ActionResult please
    }
}