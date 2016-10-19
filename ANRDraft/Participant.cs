using System;
using System.Collections.Generic;
using System.Linq;

namespace ANRDraft
{
    public class Participant : IEquatable<Participant>
    {
        string _name;
        List<Card> selectedCards;
        readonly Draft _draft;
        public List<Card> currentChoices;

        public string Name
        {
            get
            {
                return _name;
            }

            set
            {
                _name = value;
            }
        }

        public Models.ParticipantViewModel ViewModel { get
            {
                return new Models.ParticipantViewModel
                {
                    DraftName = _draft.Name,
                    Name = _name
                    //NumWaitingPacks = _draft.WaitingPacks(this).Count,
                    //SelectedCards = selectedCards.Select(c=> c.ViewModel),
                    //CurrentPack = currentChoices.Select(c => c.ViewModel)
                };
            }
        }

        public Participant(string name, Draft draft)
        {
            _name = name;
            _draft = draft;
            selectedCards = new List<Card>();
            currentChoices = null;
        }

        //check for name equality
        public bool Equals(Participant other)
        {
            return other != null && other.Name.Equals(Name);
        }

        public void SelectCardAndPass(Card c)
        {
            _draft.SelectCard(this, c);
            selectedCards.Add(c);
        }
    }
}