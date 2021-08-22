namespace Gallery_Hackathon.Models
{
    public class Shows
    {
        public int BasicShows;
        public decimal Multiplier;

        public Shows(int basicShows, decimal multiplier)
        {
            BasicShows = basicShows;
            Multiplier = multiplier;
        }

        public int ModifiedShows => (int) (BasicShows * Multiplier);
    }
}