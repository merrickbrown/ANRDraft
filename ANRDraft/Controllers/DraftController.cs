using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace ANRDraft.Controllers
{
    public class DraftController : Controller
    {
        // GET: Draft/draftname
        [Route("draft/{draftname}")]
        public ActionResult Index(string draftname)
        {
            if (draftname != null)
            {
                Draft d = DraftManager.Instance.DraftByName(draftname);
                if (d != null)
                {
                    return View(d.ViewModel);
                }
            }
            return RedirectToAction("Index", "Home");
        }
        // GET: Play/draftname/player
        [Route("play/{draftname}/{participantname}")]
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