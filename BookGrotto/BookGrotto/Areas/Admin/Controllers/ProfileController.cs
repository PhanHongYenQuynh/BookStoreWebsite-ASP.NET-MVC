using BookGrotto.Models;
using BookGrotto.Models.EF;
using CKFinder.Settings;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.EntityFramework;
using Microsoft.AspNet.Identity.Owin;
using Microsoft.Owin.Security;
using PagedList;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;
using System.Web.Security;
using System.Web.UI;

namespace BookGrotto.Areas.Admin.Controllers
{
    public class ProfileController : Controller
    {
        private ApplicationSignInManager _signInManager;
        private ApplicationUserManager _userManager;
        private ApplicationDbContext db = new ApplicationDbContext();
       
        public ApplicationUserManager UserManager
        {
            get
            {
                return _userManager ?? HttpContext.GetOwinContext().GetUserManager<ApplicationUserManager>();
            }
            private set
            {
                _userManager = value;
            }
        }
        public ApplicationSignInManager SignInManager
        {
            get
            {
                return _signInManager ?? HttpContext.GetOwinContext().Get<ApplicationSignInManager>();
            }
            private set
            {
                _signInManager = value;
            }
        }
        // GET: Admin/Profile
        public ActionResult Index()
        {
            var items = db.Users.ToList();
            return View(items);
        }

        public string ProcessUpload(HttpPostedFileBase file)
        {
            if (file == null)
            {
                return "";
            }
            file.SaveAs(Server.MapPath("~/Content/Image/" + file.FileName));
            return "/Content/Image/" + file.FileName;
        }

        [AllowAnonymous]
        public async Task<ActionResult> Profile(string id)
        {
            ViewBag.Role = new SelectList(db.Roles.ToList(), "Name", "Name");
            var _user = await UserManager.FindByNameAsync(id);
            return View(_user);
        }

        // POST: /Account/Register
        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Profile(ApplicationUser model, string role)
        {
            if (ModelState.IsValid)
            {
                var _user = await UserManager.FindByNameAsync(model.UserName);
                _user.FullName=model.FullName;
                _user.Phone=model.Phone;
                _user.Email=model.Email;
                _user.Images= model.Images;

                var result = await UserManager.UpdateAsync(_user);
                if (result.Succeeded)
                {

                    if (!string.IsNullOrEmpty(role))
                        UserManager.AddToRole(_user.Id, role);
                    return RedirectToAction("Profile", "Profile");
                }
                AddErrors(result);
            }
            ViewBag.Role = new SelectList(db.Roles.ToList(), "Id", "Name");

            return View(model);
        }

       

        private void AddErrors(IdentityResult result)
        {
            foreach (var error in result.Errors)
            {
                ModelState.AddModelError("", error);
            }
        }

    }
}