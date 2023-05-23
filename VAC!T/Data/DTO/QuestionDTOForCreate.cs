namespace VAC_T.Data.DTO
{
    public class QuestionDTOForCreate : QuestionDTOSmall
    {
        public int CompanyId { get; set; }
        public bool MultipleOptions { get; set; }
        public string ExplanationType { get; set; } = string.Empty;
        public int OptionsAmount { get; set; } = 2;
    }
}