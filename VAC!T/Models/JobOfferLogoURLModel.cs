using System.ComponentModel.DataAnnotations;

namespace VAC_T.Models
{
    public class JobOfferLogoURLModel
    {
        int id { get; set; }
        public string LogoURL { get; set; }

        [Display(Name = "Programeer Taal")]
        public string LanguageName { get; set; }
    }
}
