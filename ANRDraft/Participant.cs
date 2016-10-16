using System;
using System.Collections.Generic;

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
                return new Models.ParticipantViewModel();
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

        public void SelectCardAndPass(int cardIndex)
        {
            SelectCardAndPass(currentChoices[cardIndex]);
        }
    }
}