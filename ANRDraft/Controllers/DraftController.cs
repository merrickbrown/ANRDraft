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
                Draft d = DraftManager.Instance.DraftByName(id);
                if (d != null)
                {
                    return View(d.ViewModel);
                }
            }
            return RedirectToAction("Index", "Home");
        }

        [Route("draft/play/{draftname}/{participantname}")]
        public ActionResult Play(string draftname, string participantname)
        {
            if (draftname != null)
            {
                Participant p = DraftManager.Instance.DraftByName(draftname).ParticipantByName(participantname);
                if (p != null)
                {
                    return View(p.ViewModel);
                }
            }
            return RedirectToAction("Index", "Home");
        }
        
    }
}