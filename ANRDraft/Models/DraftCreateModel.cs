using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace ANRDraft.Models
{
    public class DraftCreateModel
    {
        private string _secretName;

        [Required(ErrorMessage = "A draft name is required", AllowEmptyStrings = false)]
        [RegularExpression(@"^[a-zA-Z0-9\-_]+", ErrorMessage = "Draft name can only contain letters and digits")]
        [Display(Name = "Draft Name", Prompt = "Enter the draft name")]
        public string SecretName { get { return _secretName; } set { _secretName = value.ToLowerInvariant(); } }

        [Required(ErrorMessage = "A decklist ID is required")]
        [Display()]
        public string DecklistLocator { get; set; }

        [Required(ErrorMessage = "Number of cards in a pack is required")]
        [Range(1, 350, ErrorMessage ="A pack must have atleast one card!")]
                public int PackSize { get; set; }

        [Required(ErrorMessage = "You must decide on how many rounds to go")]
        [Range(1, 50, ErrorMessage = "A draft needs at least one round!")]
                public int NumRounds { get; set; }

        public List<string> Names { get; internal set; }

        public DraftCreateModel()
        {
            Names = new List<string>();
        }

        public string ServerErrorMessage { get; set; }
    }
}