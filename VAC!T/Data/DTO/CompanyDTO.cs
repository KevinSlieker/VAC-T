﻿using VAC_T.Models;

namespace VAC_T.Data.DTO
{
    public class CompanyDTO
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public string LogoURL { get; set; }
        public string WebsiteURL { get; set; }
        public string Address { get; set; }
        public string? Postcode { get; set; }
        public string? Residence { get; set; }
        public UserDTOSmall? User { get; set; }
        public ICollection<JobOfferDTOSmall>? JobOffers { get; set; }
    }
}
