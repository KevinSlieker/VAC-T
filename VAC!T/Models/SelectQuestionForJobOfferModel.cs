using System.ComponentModel;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace VAC_T.Models
{
    public class SelectQuestionForJobOfferModel
    {
        public int Id { get; set; }
        [DisplayName("Geselecteerden vragen")]
        public int[]? SelectedQuestionIds { get; set; }
        public int CompanyId { get; set; }
        //public SelectList? SelectListItems { get; set; }
    }
}
