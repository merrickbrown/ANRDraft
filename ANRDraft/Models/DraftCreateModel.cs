using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace ANRDraft.Models
{
    public class DraftCreateModel
    {
        [Required]
        [RegularExpression(@"^[a-zA-Z0-9\-_]+")]
        public string SecretName { get; set; }

        [Required]
        public string DecklistLocator { get; set; }
        [Required]

        public int PackSize { get; set; }
        [Required]

        public int NumRounds { get; set; }
        public List<string> Names { get; internal set; }

        public DraftCreateModel()
        {
            Names = new List<string>();
        }
    }
}