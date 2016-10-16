using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web;

namespace ANRDraft
{
    public class DraftManager
    {
        //singleton
        private static readonly Lazy<DraftManager> _instance = new Lazy<DraftManager>(() => new DraftManager());
        private volatile int _num = 0;

        private ConcurrentDictionary<string, Draft> _drafts = new ConcurrentDictionary<string, Draft>();
        private object _draftsLock = new object();


        public int Num
        {
            get
            {
                return _num;
            }
        }
        public static DraftManager Instance { get { return _instance.Value; } }

        public IEnumerable<Draft> GetAllDrafts()
        {
            return _instance.Value._drafts.Values;
        }

        public bool TryAddDraft(Draft d)
        {
            if (d == null) return false;
            else
            {
                lock (_draftsLock)
                {
                    if(_drafts.ContainsKey(d.Name))
                    {
                        return false;
                    } else
                    {
                        _drafts.TryAdd(d.Name, d);
                        return true;
                    }
                }
            }
        }

        public bool TryRemove(Draft d)
        {
            if (d == null) return false;
            else
            {
                lock(_draftsLock)
                {
                    if (_drafts.ContainsKey(d.Name))
                    {
                        _drafts.TryRemove(d.Name, out d);
                        return true;
                    }
                    return false;
                }
            }
        }

        public void incNum()
        {
            _num++;
        }

        public Draft draftByName(string draftname)
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