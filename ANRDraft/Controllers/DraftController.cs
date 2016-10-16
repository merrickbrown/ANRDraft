using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace ANRDraft.Controllers
{
    public class DraftController : Controller
    {
        // GET: Draft
        public ActionResult Index(string id)
        {
            if (id != null)
            {
                Draft d = DraftManager.Instance.draftByName(id);
                if (d != null)
                {
                    return View(d.ViewModel);
                }
            }
            return RedirectToAction("Index", "Home");
        }

        [Route("draft/{draftname}/{participantname}")]
        public ActionResult Play(string draftname, string participantname)
        {
            if (draftname != null)
            {
                Participant p = DraftManager.Instance.draftByName(draftname).participantByName(participantname);
                if (p != null)
                {
                    return View(p.ViewModel);
                }
            }
            return RedirectToAction("Index", "Home");
        }
        
    }
}