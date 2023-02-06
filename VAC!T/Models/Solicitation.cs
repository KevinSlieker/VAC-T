using System.ComponentModel.DataAnnotations.Schema;

namespace VAC_T.Models
{
    public class Solicitation
    {
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }
        public VAC_TUser User { get; set; }
        public JobOffer JobOffer { get; set; }
        public bool Selected { get; set; }
        public DateTime Date { get; set; }

    }
}
