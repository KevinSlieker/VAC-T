using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VAC_T.Models
{
    public class QuestionOption
    {
        public int Id { get; set; }
        public Question Question { get; set; }
        public int QuestionId { get; set; }
        [DisplayName("Optie kort (voor bedrijf)")]
        public string? OptionShort { get; set; }
        [DisplayName("Optie")]
        public string OptionLong { get; set; } = string.Empty;
    }
}
