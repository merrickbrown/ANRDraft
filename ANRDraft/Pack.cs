using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ANRDraft
{
    public class Pack
    {
        List<Card> _cards;

        public IEnumerable<Card> remainingCards { get { return Cards.Where(c => !c.IsSelected); } }
        public List<Card> Cards
        {
            get
            {
                return _cards;
            }

            private set
            {
                _cards = value;
            }
        }


        public Pack(List<Card> cards)
        {
            Cards = cards;
        }

    }
}
