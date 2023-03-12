using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Web;
using System.Web.Mvc;
using BookGrotto.Models;
using BookGrotto.Models.EF;
using PagedList;

namespace BookGrotto.Areas.Admin.Controllers
{
    [Authorize(Roles = "Admin, Staff")]
    public class HomeController : Controller
    {
        // GET: Admin/Home
        private ApplicationDbContext db = new ApplicationDbContext();
        public ActionResult Index()
        {
            ViewBag.Orders = db.Orders.Count();
            ViewBag.Products = db.Products.Count();
            ViewBag.Posts = db.Posts.Count();
            ViewBag.News = db.News.Count();
            return View();
        }
    }
}