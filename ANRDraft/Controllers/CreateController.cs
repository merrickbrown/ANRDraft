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
            dcm.ServerErrorMessage = message;
            return View(dcm);
        }

      

        //POST: Create
        [HttpPost]
        [ValidateAntiForgeryToken]

        public async Task<ActionResult> Index(DraftCreateModel dcm)
        {
            if (ModelState.IsValid)
            {
                //Convert all player names to lowercase since we will be using them in the url query, which is case insensitive
                dcm.Names = dcm.Names.Select(n => n.ToLowerInvariant()).ToList();
                try
                {
                    // check that there isn't already a draft by the submitted name - note we don't need to convert to lower case since that is already in the set method of the property SecretName
                    if (DraftManager.Instance.Contains(dcm.SecretName)) throw new Exception("That draft name is already taken: use a different one");
                    //since we don't validate the name inputs client-side, we do that here
                    if (!dcm.Names.All(n => n.All(char.IsLetterOrDigit))) throw new Exception("Player names can only contain alphanumeric characters and may not contain whitespace");
                    //check that there are no duplicate player names
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
                    //try to get the decklist from netrunnerdb
                    Dictionary<CardData, int> decklist = await NRDBClient.GetDecklist(dcm.DecklistLocator);
                    //if that fails, let the user know
                    if (decklist == null) throw new Exception("Whoops! It appears that was not a valid decklist ID, please try again!");
                    //check that there are enough cards for the number of rounds and players
                    if (decklist.Values.Sum() < dcm.NumRounds * dcm.PackSize * dcm.Names.Count) throw new Exception("Whoops! That decklist doesn't contain enough cards for a draft. Try reducing the packsize or number of rounds.");
                    Draft d = Draft.CreateDraft(dcm, decklist);
                    //try to add to the list of all drafts
                    if (!DraftManager.Instance.TryAddDraft(d)) throw new Exception("That draft name is already taken: use a different one");
                    return RedirectToAction("Index", "Draft", new { draftname = dcm.SecretName });

                }
                catch (Exception e)
                {
                    dcm.ServerErrorMessage = e.Message;
                    dcm.Names.Clear(); // I think we have to clear out the player names since "remove" doesn't affect the model directly.
                    return View(dcm);
                }
                
            }
            DraftCreateModel unknownErrorDCM = new DraftCreateModel();
            unknownErrorDCM.ServerErrorMessage = "Hmm, not sure about that... Something went wrong.";
            return View(unknownErrorDCM);
            
        }

    }
}