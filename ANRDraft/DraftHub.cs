using Microsoft.AspNet.SignalR;
using Microsoft.AspNet.SignalR.Hubs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using ANRDraft.Models;

namespace ANRDraft
{
    /// <summary>
    /// SignalR Hub to communicate with clients.
    /// </summary>
    public class DraftHub : Hub
    {
        /// <summary>
        /// Calls addNewMessageToPage on each client playing a given draft
        /// </summary>
        /// <param name="draftname">The name of the draft to broadcast to</param>
        /// <param name="playername">The name of the player sending the message</param>
        /// <param name="message">The message text</param>
        public void NewChatMessage(string draftname, string playername, string message)
        {
            Clients.Group(draftname).addNewMessageToPage(playername, message);
        }
        /// <summary>
        /// Try to select the given card for the playername in the draft
        /// </summary>
        /// <param name="draftname">The name of the draft</param>
        /// <param name="playername">The player's name who wants to select the card</param>
        /// <param name="cardID">The unique ID of the card to select</param>
        /// <returns>true if the all the parameters were successfully looked up and the selection was successful, false otherwise</returns>
        public bool TrySelectCard(string draftname, string playername, string cardID)
        {
            return Do<bool>((d, p, c) => { d.TrySelectCardAndPass(p, c); return true; }, draftname, playername, cardID);
        }

        /// <summary>
        /// A utility method that makes it simpler to call a method that depends on a draft, a player, and/or a card id
        /// </summary>
        /// <typeparam name="TResult">The return value of the callback function</typeparam>
        /// <param name="callback">the function to call once the references have been looked up</param>
        /// <param name="draftname">the name of the draft</param>
        /// <param name="playername">the participant name</param>
        /// <param name="cardID">the unique card ID</param>
        /// <returns>the result of callback</returns>
        private TResult Do<TResult>(Func<Draft, Participant, Card, TResult> callback, string draftname, string playername, string cardID)
        {
            Draft d = DraftManager.Instance.DraftByName(draftname);
            Participant p = d?.ParticipantByName(playername);
            Card c = d?.CardByID(cardID);
            if (c == null || p == null) return default(TResult);
            else
            {
                return callback(d,p,c);
            }
        }

        private TResult Do<TResult>(Func<Draft, Participant, TResult> callback, string draftname, string playername)
        {
            Draft d = DraftManager.Instance.DraftByName(draftname);
            Participant p = d?.ParticipantByName(playername);
            if (p == null) return default(TResult);
            else return callback(d, p);
        }

        private TResult Do<TResult>(Func<Draft, Card, TResult> callback, string draftname, string cardID)
        {
            Draft d = DraftManager.Instance.DraftByName(draftname);
            Card c = d?.CardByID(cardID);
            if (c == null) return default(TResult);
            else return callback(d, c);
        }

        private TResult Do<TResult>(Func<Draft, TResult> callback, string draftname)
        {
            Draft d = DraftManager.Instance.DraftByName(draftname);
            if (d == null) return default(TResult);
            else return callback(d);
        }



        public IEnumerable<CardViewModel> GetCurrentPack(string draftname, string playername)
        {
            return Do((d, p) => p.currentChoices.Select(card => card.ViewModel), draftname, playername);
        }

        public IEnumerable<CardViewModel> GetSelectedCards(string draftname, string playername)
        {
            return Do((Draft d,Participant p) => p.SelectedCards.Select(card => card.ViewModel), draftname, playername);
        }

        public CardViewModel GetCard(string draftname, string cardID)
        {
            return Do((Draft d, Card c) => c.ViewModel, draftname, cardID);
        }
        /// <summary>
        /// Adds this connection ID to the given draft
        /// </summary>
        /// <param name="draftname">The name of the draft to join</param>
        public Task JoinDraft(string draftname)
        {
            return Groups.Add(Context.ConnectionId, draftname);
        }


    }
}