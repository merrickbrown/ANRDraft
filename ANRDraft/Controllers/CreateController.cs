using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using ANRDraft.Models;
using System.Threading.Tasks;

namespace ANRDraft.Controllers
{
    public class CreateController : Controller
    {
        // GET: Create
        public ActionResult Index()
        {
            DraftCreateModel dcm = new DraftCreateModel();
            return View(dcm);
        }

        //POST: Create
        [HttpPost]
        [ValidateAntiForgeryToken]

        public async Task<ActionResult> Index(DraftCreateModel dcm)
        {
            if (ModelState.IsValid)
            {
                Draft d = await Draft.AsyncCreateDraft(dcm);
                if (DraftManager.Instance.TryAddDraft(d))
                {
                    DraftManager.Instance.incNum();
                    return Redirect("Draft/Index/" + dcm.SecretName);
                }
            }
            
            return RedirectToAction("Index");
            
        }

    }
}