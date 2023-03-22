namespace VAC_T.Data.DTO
{
    public class AppointmentDTOForCreate
    {
        public int Id { get; set; }
        public DateTime Date { get; set; } = DateTime.Now;
        public DateTime Time { get; set; }
        public TimeSpan Duration { get; set; }
        public bool IsOnline { get; set; }
        public int? JobOfferId { get; set; }
    }
}
