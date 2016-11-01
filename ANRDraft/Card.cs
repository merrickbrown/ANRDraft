using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace ANRDraft
{
    /// <summary>
    /// This represents a unique card, whose data is backed by a CardData object and information about the player who selected it
    /// </summary>
    public class Card : System.IEquatable<Card>
    {
        private readonly CardData _cardData;
        private Participant _selectedBy = null;
        //should be unique per draft instance
        private readonly string _cardID;

        /// <summary>
        /// The participant who selected the card, null if unselected. This can only be set to a non-null value once.
        /// </summary>
        /// <exception cref="System.InvalidOperationException">Thrown when set from a non-null value to a different non-null value</exception>
        public Participant SelectedBy
        {
            private get
            {
                return _selectedBy;
            }

            set
            {
                if (_selectedBy == null || ReferenceEquals(value, _selectedBy))
                {
                    _selectedBy = value;
                }
                else throw new InvalidOperationException("Cannot select an already selected card");
            }
        }

        /// <summary>
        /// Creates a card from the given parameters
        /// </summary>
        /// <param name="cd">The CardData object which is represented by this Card</param>
        /// <param name="cardID">The unique (per draft) ID number for the card</param>
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


        public bool IsSelected
        {
            get
            {
                return SelectedBy != null;
            }
        }

        public string CardID
        {
            get
            {
                return _cardID;
            }
        }
        /// <summary>
        /// Checks equality with another card
        /// </summary>
        /// <param name="other">The card to compare this to</param>
        /// <returns>true if the cardIDs are equal, false otherwise</returns>
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

        public Models.CardViewModel ViewModel
        {
            get
            {
                return new Models.CardViewModel
                {
                    Data = _cardData,
                    ID = _cardID
                };
            }
        }

    }
}