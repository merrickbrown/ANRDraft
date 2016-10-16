using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace ANRDraft
{
    public class Card : System.IEquatable<Card>
    {
        private readonly CardData _cardData;
        private Participant _selectedBy = null;
        //should be unique per draft instance
        private readonly string _cardID;

        public Card(CardData cd, string cardID)
        {
            _cardData = cd;
            _cardID = cardID;
        }

        public CardData CardData
        {
            get
            {
                return _cardData;
            }
        }

        public Participant SelectedBy
        {
            get
            {
                return _selectedBy;
            }

            set
            {
                if (_selectedBy == null)
                {
                    _selectedBy = value;
                }
                else throw new InvalidOperationException("Cannot select an already selected card");
            }
        }

        public string CardID
        {
            get
            {
                return _cardID;
            }
        }

        public bool Equals(Card other)
        {
            return CardID.Equals(other.CardID);
        }

        public static bool operator ==(Card a, Card b)
        {
            if(ReferenceEquals(a,null) && ReferenceEquals(b, null))
            {
                return true;
            }

            if(ReferenceEquals(a,null) || ReferenceEquals(b,null))
            {
                return false;
            }

            return a.Equals(b);
        }

        public static bool operator !=(Card a, Card b)
        {
            return !(a == b);
        }

        public override int GetHashCode()
        {
            return CardID.GetHashCode();
        }
    }
}