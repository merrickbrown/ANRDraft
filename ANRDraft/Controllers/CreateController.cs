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
        [HttpGet]
        public ActionResult Index(string message = "")
        {
            DraftCreateModel dcm = new DraftCreateModel();
            dcm.Message = message;
            return View(dcm);
        }

      

        //POST: Create
        [HttpPost]
        [ValidateAntiForgeryToken]

        public async Task<ActionResult> Index(DraftCreateModel dcm)
        {
            if (ModelState.IsValid)
            {
                dcm.Names = dcm.Names.Select(n => n.ToLowerInvariant()).ToList();
                try
                {
                    if (!dcm.Names.All(n => n.All(char.IsLetterOrDigit))) throw new Exception("Player names can only contain alphanumeric characters and may not contain whitespace");
                    bool repeats = false;
                    for (int i = 0; i < dcm.Names.Count; i++)
                    {
                        for (int j = i + 1; j < dcm.Names.Count; j++)
                        {
                            if (dcm.Names[i] == dcm.Names[j])
                            {
                                repeats = true;
                                i = dcm.Names.Count;
                                break;
                            }
                        }
                    }
                    if (repeats) throw new Exception("All the players must be given different names. (Player names are case-insensitive.)");
                    if (DraftManager.Instance.Contains(dcm.SecretName)) throw new Exception("That draft name is already taken: use a different one");
                    Dictionary<CardData, int> decklist = await NRDBClient.GetDecklist(dcm.DecklistLocator);
                    if (decklist == null) throw new Exception("Whoops! It appears that was not a valid decklist ID, please try again!");
                    if (decklist.Values.Sum() < dcm.NumRounds * dcm.PackSize * dcm.Names.Count) throw new Exception("Whoops! That decklist doesn't contain enough cards for a draft. Try reducing the packsize or number of rounds.");
                    Draft d = Draft.CreateDraft(dcm, decklist);
                    if (!DraftManager.Instance.TryAddDraft(d)) throw new Exception("That draft name is already taken: use a different one");
                    return Redirect("Draft/Index/" + dcm.SecretName);

                }
                catch (Exception e)
                {
                    return RedirectToAction("Index", new { message = e.Message });
                }
                
            }
            
            return RedirectToAction("Index", new {message = "Hmm, not sure about that... Something went wrong."});
            
        }

    }
}