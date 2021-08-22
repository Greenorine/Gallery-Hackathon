using System;
using System.Linq;
using Gallery_Hackathon.Services;

namespace Gallery_Hackathon.Models
{
    public class CrowdItem
    {
        public int PlayerId;
        public string PlayerNumber;
        public DateTime CreatedOn;
        public string MacAddress;
        public string AdvertId;
        public bool isHoliday;
        public int Month => CreatedOn.Month;
        public int Hour => CreatedOn.Hour;
        public int Day => CreatedOn.Day;
        public int DayOfWeek => (int) CreatedOn.DayOfWeek;

        public CrowdItem(int playerId, long? rawTime, string macAddress, string advertId)
        {
            PlayerId = playerId;
            PlayerNumber = Program.PlayerIdToNumber[PlayerId];
            CreatedOn = new DateTime(1970, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc)
                .AddSeconds(rawTime ?? 0)
                .ToLocalTime();
            MacAddress = macAddress;
            AdvertId = advertId;
            isHoliday = Program.Holidays.FirstOrDefault(x =>
                CreatedOn.Day >= x.StartDay && CreatedOn.Day <= x.EndDay && CreatedOn.Month == x.Month) != null;
        }

        public override string ToString() =>
            $"{PlayerId}: Number[{PlayerNumber}], Mac[{MacAddress}], Advert[{AdvertId}], IsHoliday[{isHoliday}]," +
            $" Day[{Day}], DayOfWeek[{DayOfWeek}], Month[{Month}], Hour[{Hour}]";
    }
}