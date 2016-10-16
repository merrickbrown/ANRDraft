﻿using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ANRDraft
{
    public class Draft
    {
        private readonly string _name;
        private readonly DateTime _created;
        private readonly IReadOnlyDictionary<CardData, int> _decklist;
        private List<Card> _cardList;


        readonly LinkedList<Participant> _participants;
        private readonly Queue<Pack> _remainingPacks;
        int numRoundsRemaining;
        bool passRight;

        private object _newRoundLock = new object();

        private readonly ConcurrentDictionary<Participant, Queue<Pack>> _waitingPacks = new ConcurrentDictionary<Participant, Queue<Pack>>();
        private readonly ConcurrentDictionary<Participant, Pack> _currentPacks = new ConcurrentDictionary<Participant, Pack>();

        private volatile bool _updatingPacks = false;

        public List<string> ParticipantNames
        {
            get
            {
                return Participants.Select(p => p.Name).ToList();
            }
        }
        public LinkedList<Participant> Participants
        {
            get
            {
                return _participants;
            }
        }
        public Queue<Pack> RemainingPacks
        {
            get
            {
                return _remainingPacks;
            }
        }
        public List<Card> AllCards
        {
            get
            {
                return _cardList;
            }
        }
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
        public Queue<Pack> WaitingPack(Participant p)
        {
            return _waitingPacks[p];

        }
        public Pack CurrentPack(Participant p)
        {
            return _currentPacks[p];


        }

        private Draft(string name, Dictionary<CardData, int> decklist, List<string> participantNames, int packSize, int numRounds)
        {
            _name = name;
            _decklist = decklist;
            _created = DateTime.Now;
            _cardList = new List<Card>();
            //create all cards
            foreach (var kvp in decklist)
            {
                for (int i = 0; i < kvp.Value; i++)
                {
                    _cardList.Add(new Card(kvp.Key, kvp.Key.DBID + _name + kvp.Value.ToString()));
                }
            }
            //create participants
            _participants = new LinkedList<Participant>(participantNames.Select(n => new Participant(n, this)));
            foreach (Participant p in _participants)
            {
                _waitingPacks[p] = new Queue<Pack>();
            }
            NumRoundsRemaining = numRounds;
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

        }

        public static async Task<Draft> AsyncCreateDraft(Models.DraftCreateModel dcm)
        {
            Dictionary<CardData, int> decklist = await NRDBClient.GetDecklist(dcm.DecklistLocator);
            Draft result = new Draft(dcm.SecretName, decklist, dcm.Names, dcm.PackSize, dcm.NumRounds);
            return result;
        }


        public void SelectCard(Participant participant, Card c)
        {
            //record who selected the card
            c.SelectedBy = participant;
            Pack packToPass = _currentPacks[participant];
            Participant passesTo = PassesTo(participant);
            //if there are any cards left to be chosen
            if (packToPass.remainingCards.Count() > 0)
            {
                _waitingPacks[passesTo].Enqueue(packToPass);
                //check to see if the next player had no current pack
                if (_currentPacks[passesTo] == null)
                {
                    //automatically dequeue the given pack and assign
                    SetParticipantCurrentPack(passesTo, WaitingPack(passesTo).Dequeue());
                }
            }
            //if there are any waiting packs
            if (_waitingPacks[participant].Count > 0)
            {
                //dequeue and make the pack the current pack
                SetParticipantCurrentPack(participant, _waitingPacks[participant].Dequeue());
            }
            // otherwise set their currentpack to null
            else
            {
                SetParticipantCurrentPack(participant, null);
            }
            //check if round is over
            if (isRoundComplete()) startNewRound();
        }
        public void startNewRound()
        {
            //if there are no more rounds, end the draft
            if (NumRoundsRemaining == 0) { endDraft(); }
            //change passign rules
            PassRight = !PassRight;
            //populate current packs
            foreach (Participant p in Participants)
            {
                SetParticipantCurrentPack(p, RemainingPacks.Dequeue());
            }
            //decrement remaining rounds
            NumRoundsRemaining--;
        }

        private void endDraft()
        {
            throw new NotImplementedException();
        }

        public bool isRoundComplete()
        {
            //given the code for selecting and passing packs I am pretty sure if the first condition is true then the second one is as well, but i havent totally though it out yet
            return _currentPacks.Values.All(cp => cp == null) && _waitingPacks.Values.All(q => q.Count == 0);
        }

        //Returns who p passes to
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
        public Participant participantByName(string name)
        {
            return Participants.Where(p => p.Name == name).SingleOrDefault();
        }

        private void SetParticipantCurrentPack(Participant participant, Pack pack)
        {
            _currentPacks[participant] = pack;
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

        public DateTime Created
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
                Models.DraftViewModel model = new Models.DraftViewModel { Name = Name, Created = Created, Decklist = new Dictionary<string, int>() };
                foreach(var kvp in Decklist)
                {
                    model.Decklist[kvp.Key.Title] = kvp.Value;
                }
                return model;
            }
        }

        public IReadOnlyDictionary<CardData, int> Decklist
        {
            get
            {
                return _decklist;
            }
        }
    }
}