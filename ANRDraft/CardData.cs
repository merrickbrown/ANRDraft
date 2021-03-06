﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Runtime.Serialization;

#pragma warning disable CS0660 // Type defines operator == or operator != but does not override Object.Equals(object o)
namespace ANRDraft
{
    /// <summary>
    /// Represents a card in Android: Netrunner
    /// </summary>
    [Serializable]
    public class CardData : IEquatable<CardData>
    {
        private readonly string _dBID;
        private readonly JObject _data;

        public CardData(string cardDBID, JObject data)
        {
            _dBID = cardDBID;
            _data = data;
        }
        /// <summary>
        /// The title of the card
        /// </summary>
        public string Title { get { return _data["title"].Value<string>(); } }

        /// <summary>
        /// The type of a card
        /// </summary>
        public string Type { get { return _data["type_code"].Value<string>().ToUpperInvariant(); } }

        /// <summary>
        /// The ID given to the card by netrunnerdb.com
        /// </summary>
        public string DBID
        {
            get
            {
                return _dBID;
            }
        }
        /// <summary>
        /// The address of the card information page on netrunnerdb.com
        /// </summary>
        public Uri DBUri
        {
            get
            {
                return new Uri($"https://netrunnerdb.com/en/card/{DBID}");
            }
        }
        /// <summary>
        /// The address of the card image on netrunnerdb.com
        /// </summary>
        public Uri ImageUri
        {
            get
            {
                return new Uri($"https://netrunnerdb.com/card_image/{DBID}.png");
            }
        }

        public JObject Data
        {
            get
            {
                return _data;
            }
        }
        /// <summary>
        /// Checks for equality of card data
        /// </summary>
        /// <param name="other">The other card</param>
        /// <returns>true if and only if the database IDs match</returns>
        public bool Equals(CardData other)
        {
            return DBID.Equals(other.DBID);
        }

        public static bool operator ==(CardData a, CardData b)
        {
            if (ReferenceEquals(a, null) && ReferenceEquals(b, null))
            {
                return true;
            }

            if (ReferenceEquals(a, null) || ReferenceEquals(b, null))
            {
                return false;
            }

            return a.Equals(b);
        }

        public static bool operator !=(CardData a, CardData b)
        {
            return !(a == b);
        }

        public override int GetHashCode()
        {
            return DBID.GetHashCode();
        }
    }


}
