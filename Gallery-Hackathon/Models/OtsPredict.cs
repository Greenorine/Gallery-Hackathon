using System;

namespace Gallery_Hackathon.Models
{
    public class OtsPredict
    {
        public DateTime Date;
        public int PlayerId;
        public Shows[] ShowsData;

        public OtsPredict(DateTime date, int playerId, Shows[] showsData)
        {
            Date = date;
            PlayerId = playerId;
            ShowsData = showsData;
        }
    }
}