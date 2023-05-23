namespace VAC_T.Data.DTO
{
    public class QuestionDTOComplete : QuestionDTOMedium
    {
        public int OptionsAmount { get; set; } = 2;
        public ICollection<QuestionOptionDTOSmall>? Options { get; set; }
    }
}