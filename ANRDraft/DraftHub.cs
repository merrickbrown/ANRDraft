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

    public class DraftHub : Hub
    {

        public void NewChatMessage(string draftname, string playername, string message)
        {
            Clients.Group(draftname).addNewMessageToPage(playername, message);
        }

        public bool TrySelectCard(string draftname, string playername, string cardID)
        {
            return Do<bool>((d, p, c) => { d.SelectCardAndPass(p, c); return true; }, draftname, playername, cardID);
        }


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

        public Task JoinDraft(string draftname)
        {
            return Groups.Add(Context.ConnectionId, draftname);
        }


    }
}