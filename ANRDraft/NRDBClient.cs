using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using System.Web;

namespace ANRDraft
{
    /// <summary>
    /// Provides access to netrunnerdb
    /// </summary>
    public class NRDBClient
    {
        //static singleton
        private static readonly NRDBClient _instance = new NRDBClient();
        private Dictionary<string, JObject> _allCards;
        private readonly HttpClient _httpClient;

        public static NRDBClient Instance
        {
            get { return _instance; }
        }

        private NRDBClient()
        {
            _httpClient = new HttpClient();
            _allCards = new Dictionary<string, JObject>();
            using (HttpResponseMessage response = _httpClient.GetAsync("http://netrunnerdb.com/api/2.0/public/cards").Result)
            {
                string result = response.Content.ReadAsStringAsync().Result;
                IEnumerable<JObject> cards = JObject.Parse(result)["data"].ToList().Select(obj => (JObject)obj);
                foreach(var cardObj in cards)
                {
                   _allCards[cardObj.Value<string>("code")] = cardObj;
                }

            }
        }

        public CardData GetCard(string cardID)
        {
                return new CardData(cardID, Instance._allCards[cardID]);
        }
        // what if the deckID is not valid?
        public async Task<Dictionary<CardData, int>> GetDecklist(string deckID)
        {
            using (HttpResponseMessage decklistResponse = await Instance._httpClient.GetAsync($"http://netrunnerdb.com/api/2.0/public/decklist/{deckID}"))
            {
                if (decklistResponse.IsSuccessStatusCode)
                {
                    string decklistResponseText = await decklistResponse.Content.ReadAsStringAsync();
                    JObject decklistResponseObj = JObject.Parse(decklistResponseText);
                    JObject deckListTokens = (JObject)decklistResponseObj["data"].First()["cards"];
                    Dictionary<string, int> decklistData = new Dictionary<string, int>();
                    foreach (var kvp in deckListTokens)
                    {
                        decklistData[kvp.Key] = kvp.Value.Value<int>();
                    }
                    Dictionary<CardData, int> result = new Dictionary<CardData, int>();
                    var cards = decklistData.Keys.Select(GetCard);
                    bool identityFound = false;
                    foreach (var cd in cards)
                    {
                        if (identityFound)
                        {
                            result[cd] = decklistData[cd.DBID];
                        }
                        else
                        {
                            // filter out the identity from the list
                            if (cd.Data.Value<string>("type_code") == "identity")
                            {
                                identityFound = true;
                            }
                            else
                            {
                                result[cd] = decklistData[cd.DBID];
                            }
                        }
                    }
                    return result;
                } else
                {
                    return null;
                }
            }
        }

    }
}