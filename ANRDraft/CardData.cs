using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Runtime.Serialization;

#pragma warning disable CS0660 // Type defines operator == or operator != but does not override Object.Equals(object o)
namespace ANRDraft
{
    [Serializable]
    public class CardData : IEquatable<CardData>
    {
        private readonly string _dBID;
        private readonly JObject _data;

        public CardData(string cardDBID, string rawData)
        {
            _dBID = cardDBID;
            _data = (JObject)JObject.Parse(rawData)["data"].First();
        }

        public string Title { get { return _data["title"].Value<string>(); } }


        public string DBID
        {
            get
            {
                return _dBID;
            }
        }

        public Uri DBUri
        {
            get
            {
                return new Uri($"https://netrunnerdb.com/en/card/{DBID}");
            }
        }

        public Uri ImageUri
        {
            get
            {
                return new Uri($"https://netrunnerdb.com/card_image/{DBID}.png");
            }
        }

        public object Data
        {
            get
            {
                return _data;
            }
        }
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
