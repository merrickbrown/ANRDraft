using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace ANRDraft.Controllers
{
    public class HomeController : Controller
    {
        public ActionResult Index()
        {
            return View(DraftManager.Instance.Count);
        }

        public ActionResult About()
        {
            ViewBag.Message = "This is a protoype for a web app that allows players to draft decks for Android:Netrunner remotely.";

            return View();
        }

        public ActionResult Contact()
        {
            ViewBag.Message = "Your contact page.";

            return View();
        }
    }
}