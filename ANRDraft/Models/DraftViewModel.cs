using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace ANRDraft.Models
{
    public class DraftViewModel
    {
        public string Name { get;  set; }
        public DateTime Created { get;  set; }
        public Dictionary<string,int> Decklist { get; set; }
    }
}