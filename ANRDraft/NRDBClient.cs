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

        public static async Task<CardData> GetCard(string cardID)
        {
            using (HttpResponseMessage response = await Instance.GetAsync($"http://netrunnerdb.com/api/2.0/public/card/{cardID}"))
            {
                string result = await response.Content.ReadAsStringAsync();

                return new CardData(cardID, result);
            }
        }
        // what if the deckID is not valid?
        public static async Task<Dictionary<CardData, int>> GetDecklist(string deckID)
        {
            using (HttpResponseMessage decklistResponse = await Instance.GetAsync($"http://netrunnerdb.com/api/2.0/public/decklist/{deckID}"))
            {
                string decklistResponseText = await decklistResponse.Content.ReadAsStringAsync();
                JObject decklistResponseObj = JObject.Parse(decklistResponseText);
                JObject deckListTokens = (JObject)decklistResponseObj["data"].First()["cards"];
                Dictionary<string, int> deckListData = new Dictionary<string, int>();
                foreach(var kvp in deckListTokens)
                {
                    deckListData[kvp.Key] = kvp.Value.Value<int>();
                }
                ConcurrentDictionary<string, int> decklistRaw = new ConcurrentDictionary<string, int>(deckListData);
                ConcurrentDictionary<CardData, int> decklist = new ConcurrentDictionary<CardData, int>();
                await Task.WhenAll(decklistRaw.Keys.Select(cardID => Task.Run(async () => decklist[await GetCard(cardID)] = decklistRaw[cardID])));
                return new Dictionary<CardData, int>(decklist);
            }
        }

    }
}