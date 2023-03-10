namespace VAC_T.Data.DTO
{
    public class SolicitationDTOSmall
    {
        public int id { get; set; }
        public string name { get; set; }
        public bool selected { get; set; }
        public UserDTOSmall user { get; set; }
    }
}
