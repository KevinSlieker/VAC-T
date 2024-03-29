﻿using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using VAC_T.Models;

namespace VAC_T.Data.DTO
{
    public class JobOfferDTO
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public string LogoURL { get; set; }
        public CompanyDTOSmall Company { get; set; }
        public int CompanyId { get; set; }
        public string Level { get; set; }
        public string? Residence { get; set; }
        public DateTime Created { get; set; } = DateTime.Now;
        public DateTime? Closed { get; set; }
        public ICollection<SolicitationDTOSmall>? Solicitations { get; set; }
        public ICollection<QuestionDTOSmall>? Questions { get; set; }
        public ICollection<AnswerDTOSmall>? Answers { get; set; }
    }
}
