using CsvHelper.Configuration.Attributes;

namespace VAC_T.Models
{
    [Delimiter(";")]
    //[CultureInfo("")]
    public class CSVLine
    {
        [Name("Vraag")]
        public string Question { get; set; }
        [Name("Antwoord")]
        public string Answer { get; set; }
        [Name("Uitleg")]
        public string? Explanation { get; set; }
    }
}
