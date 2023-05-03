using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VAC_T.Models
{
    public class QuestionOption
    {
        public int Id { get; set; }
        public Question Question { get; set; } = new Question();
        public int QuestionId { get; set; }
        public string? OptionShort { get; set; }
        public string OptionLong { get; set; } = string.Empty;
    }
}
