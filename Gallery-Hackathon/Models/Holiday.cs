namespace Gallery_Hackathon.Models
{
    public class Holiday
    {
        public byte StartDay;
        public byte EndDay;
        public byte Month;

        public Holiday(byte startDay, byte endDay, byte month)
        {
            StartDay = startDay;
            EndDay = endDay;
            Month = month;
        }
    }
}