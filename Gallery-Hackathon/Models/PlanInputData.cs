using System;
using System.Collections.Generic;

namespace Gallery_Hackathon.Models
{
    public class PlanInputData
    {
        public List<int> Players;
        public List<int> DaysOfWeek;
        public int AdvertFrequency;
        public DateTime StartDate;
        public DateTime EndDate;
        public TimeSpan StartTime;
        public TimeSpan EndTime;
        public int NeedOts;
        public decimal SpecialModifier;

        public PlanInputData(List<int> players, List<int> daysOfWeek, int advertFrequency, decimal specialModifier,
            int needOts, DateTime startDate, DateTime endDate, TimeSpan startTime, TimeSpan endTime)
        {
            Players = players;
            DaysOfWeek = daysOfWeek;
            AdvertFrequency = advertFrequency;
            SpecialModifier = specialModifier;
            NeedOts = needOts;
            StartDate = startDate;
            EndDate = endDate;
            StartTime = startTime;
            EndTime = endTime;
        }
    }
}