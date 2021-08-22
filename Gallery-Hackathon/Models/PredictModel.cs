using System.Collections.Generic;

namespace Gallery_Hackathon.Models
{
    public class PredictModel
    {
        public Dictionary<int, decimal> DayOfWeekModifiers;
        public Dictionary<int, decimal> MonthModifiers;
        public Dictionary<int, decimal> HourModifiers;
        public Dictionary<int, decimal> PlayerModifiers;
        public int AverageOtsPerAdvert;

        public PredictModel(Dictionary<int, decimal> dayOfWeekModifiers, Dictionary<int, decimal> monthModifiers,
            Dictionary<int, decimal> hourModifiers, Dictionary<int, decimal> playerModifiers, int averageOtsPerAdvert)
        {
            DayOfWeekModifiers = dayOfWeekModifiers;
            MonthModifiers = monthModifiers;
            HourModifiers = hourModifiers;
            PlayerModifiers = playerModifiers;
            AverageOtsPerAdvert = averageOtsPerAdvert;
        }
    }
}