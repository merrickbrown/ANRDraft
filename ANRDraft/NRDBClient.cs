using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web;

namespace ANRDraft
{
    public class NRDBClient : HttpClient
    {
        //singleton
        private static readonly Lazy<NRDBClient> _instance = new Lazy<NRDBClient>(() => new NRDBClient());

        public static NRDBClient Instance
        {
            get { return _instance.Value; }
        }

        private NRDBClient() : base()
        {

        }

        public static CardData GetCard(string cardID)
        {
            using (HttpResponseMessage response = Instance.GetAsync($"http://netrunnerdb.com/api/2.0/public/card/{cardID}").Result)
            {
                string result = response.Content.ReadAsStringAsync().Result;

                return new CardData(cardID, result);
            }
        }
        // what if the deckID is not valid?
        public static async Task<Dictionary<CardData, int>> GetDecklist(string deckID)
        {
            using (HttpResponseMessage decklistResponse = await Instance.GetAsync($"http://netrunnerdb.com/api/2.0/public/decklist/{deckID}"))
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
                    var cards = decklistData.Keys.AsParallel().AsOrdered().Select(cardID => GetCard(cardID));
                    //var cards = decklistData.Keys.AsParallel().AsOrdered().WithDegreeOfParallelism(20).Select(cardID => GetCard(cardID));
                    bool identityFound = false;
                    foreach (var cd in cards)
                    {
                        if (identityFound)
                        {
                            result[cd] = decklistData[cd.DBID];
                        }
                        else
                        {
                            if (((JObject)cd.Data).Value<string>("type_code") == "identity")
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