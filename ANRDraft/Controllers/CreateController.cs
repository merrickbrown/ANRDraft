using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using ANRDraft.Models;
using System.Threading.Tasks;
using System.Web.Routing;

namespace ANRDraft.Controllers
{
    public class CreateController : Controller
    {
        // GET: Create
        public ActionResult Index(string message = "")
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
                if (DraftManager.Instance.Contains(dcm.SecretName)) {
                    return RedirectToAction("Index", new { message = "That draft name is already taken: use a different one" });
                } 
                Dictionary<CardData, int> decklist = await NRDBClient.GetDecklist(dcm.DecklistLocator);
                if (decklist == null)
                {
                    return RedirectToAction("Index", new { message = "Whoops! It appears that was not a valid decklist ID, please try again!" });
                }
                if(decklist.Values.Sum() < dcm.NumRounds * dcm.PackSize * dcm.Names.Count)
                {
                    return RedirectToAction("Index", new { message = "Whoops! That decklist doesn't contain enough cards for a draft. Try reducing the packsize or number of rounds." });
                }
                Draft d = Draft.CreateDraft(dcm, decklist);
                if (DraftManager.Instance.TryAddDraft(d))
                {
                    return Redirect("Draft/Index/" + dcm.SecretName);
                } else
                {
                    return RedirectToAction("Index", new { message = "That draft name is already taken: use a different one" });
                }
            }
            
            return RedirectToAction("Index", new {message = "Hmm, not sure about that... Somethign went wrong."});
            
        }

    }
}