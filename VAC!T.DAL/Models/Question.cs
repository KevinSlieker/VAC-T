﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace VAC_T.Models
{
    public class Question
    {
        public int Id { get; set; }
        [Display(Name = "Bedrijf")]
        public Company? Company { get; set; }
        public int? CompanyId { get; set; }
        [DisplayName("Soort vraag")]
        public string Type { get; set; } = string.Empty; // Open, Multiple choice and standpoint between 2.
        [DisplayName("Vraag")]
        public string QuestionText { get; set; }
        [DisplayName("Meerdere mogelijkheden")]
        public bool MultipleOptions { get; set; }
        [DisplayName("Opties")]
        public ICollection<QuestionOption> Options { get; set; } = new List<QuestionOption>();
        [DisplayName("Uitleg")]
        public string ExplanationType { get; set; } = string.Empty;
        [DisplayName("Aantal meerkeuze opties toevoegen")]
        public int OptionsAmount { get; set; } = 2;
        public ICollection<JobOffer>? JobOffers { get; set; } = new List<JobOffer>();
        public ICollection<Answer>? Answers { get; set; }
    }
}
