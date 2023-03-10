namespace VAC_T.Data.DTO
{
    public class JobOfferDTOForCreateTemp
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public string LogoURL { get; set; }
        public int CompanyId { get; set; }
        public string? Residence { get; set; }
        public string Level { get; set; }
    }
}
