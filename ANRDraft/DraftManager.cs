using Microsoft.AspNet.SignalR;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web;

namespace ANRDraft
{
    /// <summary>
    /// Manages all the ongoing drafts, also holds a reference to the signalr context
    /// </summary>
    public class DraftManager
    {
        //singleton
        private static readonly Lazy<DraftManager> _instance = new Lazy<DraftManager>(() => new DraftManager(GlobalHost.ConnectionManager.GetHubContext<DraftHub>()));


        private ConcurrentDictionary<string, Draft> _drafts = new ConcurrentDictionary<string, Draft>();
        private IHubContext _context;
        private int _count = 0;

        private DraftManager(IHubContext context)
        {
            _context = context;
        }

        public static DraftManager Instance { get { return _instance.Value; } }

        public IHubContext Context
        {
            get
            {
                return _context;
            }
        }

        public int Count
        {
            get
            {
                return _count;
            }

            set
            {
                _count = value;
            }
        }

        public bool Contains(string draftname)
        {
            return _drafts.ContainsKey(draftname);
        }



        public IEnumerable<Draft> GetAllDrafts()
        {
            return _instance.Value._drafts.Values;
        }

        public bool TryAddDraft(Draft d)
        {
            if (d == null) return false;
            else
            {
                if (_drafts.TryAdd(d.Name, d))
                {
                    _count++;
                    return true;
                }
                else
                {
                    return false;
                }
            }
        }

        public bool TryRemove(Draft d)
        {
            if (d == null) return false;
            else
            {
                if (_drafts.TryRemove(d.Name, out d))
                {
                    _count--;
                    return true;
                }
                else
                {
                    return false;
                }
            }
        }


        public Draft DraftByName(string draftname)
        {
            Draft returnDraft;
                if (_drafts.TryGetValue(draftname, out returnDraft))
                {
                    return returnDraft;
                }
                else
                {
                    return null;
                }
        }

    }
}