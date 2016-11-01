using Microsoft.AspNet.SignalR;
using Microsoft.AspNet.SignalR.Hubs;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ANRDraft
{
    /// <summary>
    /// This represents a single ANR draft
    /// </summary>
    public class Draft
    {
        private readonly string _name;
        private readonly DateTime _created;
        private readonly IReadOnlyDictionary<CardData, int> _decklist;
        private List<Card> _cardList;
        private Dictionary<string, Card> _cardLookup;
        readonly LinkedList<Participant> _participants;
        private Queue<Pack> _remainingPacks;

        int numRoundsRemaining;
        bool passRight;
        private object _newRoundLock = new object();
        private object _packsLock = new object();
        private readonly ConcurrentDictionary<Participant, Queue<Pack>> _waitingPacks = new ConcurrentDictionary<Participant, Queue<Pack>>();
        private readonly ConcurrentDictionary<Participant, Pack> _currentPacks = new ConcurrentDictionary<Participant, Pack>();
        private volatile bool _updatingPacks = false;
        /// <summary>
        /// A list of the participant names
        /// </summary>
        public List<string> ParticipantNames
        {
            get
            {
                return Participants.Select(p => p.Name).ToList();
            }
        }
        /// <summary>
        /// Returns the linked list backing the participants of the draft 
        /// </summary>
        public LinkedList<Participant> Participants
        {
            get
            {
                return _participants;
            }
        }
        /// <summary>
        /// All the packs that have not been drafted, nor are currently being drafted
        /// </summary>
        public Queue<Pack> RemainingPacks
        {
            get
            {
                return _remainingPacks;
            }
        }
        /// <summary>
        /// A list of the cards that populated the draft
        /// </summary>
        public List<Card> AllCards
        {
            get
            {
                return _cardList;
            }
        }
        /// <summary>
        /// The number of rounds remaining in the draft, not including the current round
        /// </summary>
        public int NumRoundsRemaining
        {
            get
            {
                return numRoundsRemaining;
            }

            private set
            {
                numRoundsRemaining = value;
            }
        }
        /// <summary>
        /// true if the rule is to pass cards to the right, false if otherwise
        /// </summary>
        public bool PassRight
        {
            get
            {
                return passRight;
            }

            private set
            {
                passRight = value;
            }
        }
        /// <summary>
        /// The packs that a given participant still needs to look at
        /// </summary>
        /// <param name="p">The participant</param>
        /// <returns>A Queue of the packs that the participant still needs to choose from, not including their current pack.</returns>
        private Queue<Pack> WaitingPacks(Participant p)
        {
            return _waitingPacks[p];

        }
        /// <summary>
        /// The pack that a given particiant is choosing from
        /// </summary>
        /// <param name="p"></param>
        /// <returns></returns>
        public Pack CurrentPack(Participant p)
        {
            return _currentPacks[p];
        }
        /// <summary>
        /// Creates and starts a new draft
        /// </summary>
        /// <param name="name">the name which will identify the draft</param>
        /// <param name="decklist">the number of each card and its data that will be used to populate the packs</param>
        /// <param name="participantNames">the names fo the participants</param>
        /// <param name="packSize">how many cards will initially fill each pack</param>
        /// <param name="numRounds">the number of rounds in the draft</param>
        private Draft(string name, Dictionary<CardData, int> decklist, List<string> participantNames, int packSize, int numRounds)
        {
            _name = name;
            _decklist = decklist;
            _created = DateTime.Now;
            _cardList = new List<Card>();
            _cardLookup = new Dictionary<string, Card>();
            //create all cards and populate _cardLookup
            foreach (var kvp in decklist)
            {
                for (int i = 0; i < kvp.Value; i++)
                {
                    //the unique ID is just {database ID}{draft name}{copy number}
                    string uniqueID = kvp.Key.DBID + _name + i.ToString();
                    Card c = new Card(kvp.Key, uniqueID);
                    _cardList.Add(c);
                    _cardLookup[uniqueID] = c;
                }
            }


            //shuffle cards using draftname as the seed - it is not properly random so that if something goes wrong, a draft can be restarted and the players can re-pick the same cards
            Random rng = new Random(name.GetHashCode());
            int n = _cardList.Count;
            while (n > 1)
            {
                n--;
                int k = rng.Next(n + 1);
                Card v = _cardList[k];
                _cardList[k] = _cardList[n];
                _cardList[n] = v;
            }

            //create participants
            _participants = new LinkedList<Participant>(participantNames.Select(pname => new Participant(pname, this)));

            foreach (Participant p in _participants)
            {
                _waitingPacks[p] = new Queue<Pack>(_participants.Count);
            }
            NumRoundsRemaining = numRounds;
            //since we want the draft to start with passing to the right, we set this to false, so that when the first round starts, it will be set to true 
            PassRight = false;
            //num packs will be num rounds * num participants
            int numPacks = numRounds * _participants.Count;
            _remainingPacks = new Queue<Pack>(numPacks);
            //enqueue packs to _remainingPacks and associate the card list to each pack in _cardLists
            for (int packIndex = 0; packIndex < numPacks; packIndex++)
            {
                List<Card> currPackList = AllCards.GetRange(packIndex * packSize, packSize);
                RemainingPacks.Enqueue(new Pack(currPackList));
            }
            //start the round
            StartNewRound();
        }
        public static Draft CreateDraft(Models.DraftCreateModel dcm, Dictionary<CardData, int> decklist)
        {
            Draft result = new Draft(dcm.SecretName, decklist, dcm.Names, dcm.PackSize, dcm.NumRounds);
            return result;
        }
        /// <summary>
        /// A participant selects a card and passes their current pack to the next player. If the participant has any waiting packs, that gets assigned to their current pack as well.
        /// </summary>
        /// <param name="participant">The participant who is selecting a card</param>
        /// <param name="c">The card to be selected</param>
        /// <returns>true if the selection was successful, otherwise false</returns>
        public bool TrySelectCardAndPass(Participant participant, Card c)
        {
            //lock the packs - we only want one thread to have access to the waiting packs at a time. 
            lock (_packsLock)
            {
                //I don't think this is necessary, but I really don't want multiple threads updating the waiting packs at the same time
                if (!_updatingPacks)
                {
                    _updatingPacks = true;

                    try
                    {
                        // check that this is actually a card in the current pack
                        if (!participant.currentChoices.Contains(c)) throw new Exception("That card is no longer in that pack!");
                        //record who selected the card
                        c.SelectedBy = participant;
                    }
                    catch
                    {
                        _updatingPacks = false;
                        return false;
                    }
                    //this is outside of the try-catch block because any exception here is really catastrophic, in that we don't have code to recover from it

                    participant.SelectCard(c);
                    Pack packToPass = _currentPacks[participant];
                    Participant passesTo = PassesTo(participant);
                    if (packToPass.remainingCards.Count() > 0)  //if there are any cards left to be chosen
                    {
                        _waitingPacks[passesTo].Enqueue(packToPass);
                        if (_currentPacks[passesTo] == null) //check to see if the next player had no current pack
                        {
                            SetParticipantCurrentPack(passesTo, WaitingPacks(passesTo).Dequeue()); //dequeue the given pack and assign to the correct participant
                            NotifyParticipantOfNewPack(passesTo);
                        }
                    }

                    if (_waitingPacks[participant].Count > 0) //if there are any waiting packs
                    {

                        SetParticipantCurrentPack(participant, _waitingPacks[participant].Dequeue()); //dequeue and make that pack the current pack
                    }

                    else
                    {
                        SetParticipantCurrentPack(participant, null); // otherwise set the currentpack to null
                    }

                    if (IsRoundComplete()) StartNewRound(); //check if round is over, and if so, start a new one
                    _updatingPacks = false;
                    return true;

                }
                return false;
            }
        }
        private void StartNewRound()
        {
            lock (_newRoundLock)
            {
                //if there are no more rounds, end the draft
                if (NumRoundsRemaining == 0) { EndDraft(); }
                else
                {
                    //toggle passign rules
                    PassRight = !PassRight;
                    //populate current packs
                    foreach (Participant p in Participants)
                    {
                        SetParticipantCurrentPack(p, RemainingPacks.Dequeue());
                    }
                    //decrement remaining rounds
                    NumRoundsRemaining--;
                    BroadCastNewRound();
                }
            }
        }
        private void EndDraft()
        {
            BroadCastEndDraft();
        }

        private void BroadCastEndDraft()
        {
            Context.Clients.Group(Name).draftOver();
        }

        private bool IsRoundComplete()
        {
            //given the code for selecting and passing packs I am pretty sure if the first condition is true then the second one is as well, but i havent totally though it out yet
            return _currentPacks.Values.All(cp => cp == null) && _waitingPacks.Values.All(q => q.Count == 0);
        }
        /// <summary>
        /// Returns who gets passed to by the current passing rules.
        /// </summary>
        /// <param name="p">The participant who is doing the passing</param>
        /// <returns>The participant who gets passed to</returns>
        private Participant PassesTo(Participant p)
        {
            if (PassRight)
            {
                LinkedListNode<Participant> pNode = Participants.Find(p);
                return pNode.Next?.Value ?? Participants.First.Value;
            }
            else
            {
                LinkedListNode<Participant> pNode = Participants.Find(p);
                return pNode.Previous?.Value ?? Participants.Last.Value;
            }
        }
        public Participant ParticipantByName(string name)
        {
            return Participants.Where(p => p.Name == name).SingleOrDefault();
        }
        /// <summary>
        /// Sets a current pack for the given participant
        /// </summary>
        /// <param name="participant">The participant who's pack we want to set</param>
        /// <param name="pack">Which pack to set</param>
        private void SetParticipantCurrentPack(Participant participant, Pack pack)
        {
            _currentPacks[participant] = pack;

            // should I refactor this into participant? pros/cons?
            if (pack != null)
            {
                participant.currentChoices = pack.remainingCards.ToList();
            }
            else
            {
                participant.currentChoices = new List<Card>();
            }
        }
        public string Name
        {
            get
            {
                return _name;
            }
        }
        private DateTime Created
        {
            get
            {
                return _created;
            }
        }
        public Models.DraftViewModel ViewModel
        {
            get
            {
                Models.DraftViewModel model = new Models.DraftViewModel
                {
                    Name = Name,
                    Created = Created,
                    Decklist = new Dictionary<string, int>(),
                    PlayerNames = Participants.Select(p => p.Name).ToList()
                };
                foreach (var kvp in Decklist)
                {
                    model.Decklist[kvp.Key.Title] = kvp.Value;
                }
                return model;
            }
        }
        private IReadOnlyDictionary<CardData, int> Decklist
        {
            get
            {
                return _decklist;
            }
        }
        private IHubContext Context { get { return DraftManager.Instance.Context; } }

        private void BroadCastNewRound()
        {
            Context.Clients.Group(Name).newRound();

        }

        private void NotifyParticipantOfNewPack(Participant p)
        {
            Context.Clients.Group(Name).newPack(p.Name);
        }

        public Card CardByID(string uniqueID)
        {
            return _cardLookup[uniqueID];
        }


    }
}