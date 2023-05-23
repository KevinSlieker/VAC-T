namespace VAC_T.Data.DTO
{
    public class QuestionDTOMedium : QuestionDTOSmall
    {
        public CompanyDTOSmall Company { get; set; }
        public int CompanyId { get; set; }
        public bool MultipleOptions { get; set; }
        public string ExplanationType { get; set; } = string.Empty;
    }
}