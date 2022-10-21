using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;

namespace CovidLetter.Frontend.WebApp.Models
{
    public class DigitalContactPreferenceViewModel
    {
        [Required(ErrorMessage = "noSelection")]
        public DigitalContactPreferenceOptions? Selection { get; set; }
        
        public Dictionary<DigitalContactPreferenceOptions, string> ObfuscatedContactDetails { get; } = new Dictionary<DigitalContactPreferenceOptions, string>();
    }

    public enum DigitalContactPreferenceOptions
    {
        NoContact,
        Email,
        Phone
    }
}