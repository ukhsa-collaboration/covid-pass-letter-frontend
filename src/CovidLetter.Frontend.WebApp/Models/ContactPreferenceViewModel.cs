using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;

namespace CovidLetter.Frontend.WebApp.Models
{
    public class ContactPreferenceViewModel
    {
        public Guid? Selection { get; set; }
        
        public Dictionary<Guid, string> ObfuscatedContactDetails { get; } = new Dictionary<Guid, string>();

        public bool AnyContactDetails => ObfuscatedContactDetails.Any();
        
        public bool AccessibleFormatOrOtherLanguageRequested { get; set; }
        
        public bool userRequestAlternativeLanguage { get; set; }

        public bool postcodeIsWelsh { get; set; }
    }
}