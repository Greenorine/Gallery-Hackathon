using System.Collections.Generic;
using Gallery_Hackathon.Models;

namespace Gallery_Hackathon
{
    public class Program
    {
        public static List<Holiday> Holidays = new List<Holiday>()
        {
            new Holiday(1, 8, 1),
            new Holiday(23, 23, 2),
            new Holiday(6, 8, 3),
            new Holiday(1, 3, 5),
            new Holiday(8, 10, 5),
            new Holiday(12, 14, 6),
            new Holiday(27, 27, 6),
            new Holiday(1, 1, 9),
            new Holiday(4, 7, 11),
            new Holiday(31, 31, 12)
        };

        public static Dictionary<int, string> PlayerIdToNumber = new()
        {
            {257, "NVS036APL"},
            {258, "NVS053APL"},
            {259, "NVS024BPL"},
            {260, "NVS002APL"},
            {261, "NVS037APL"},
            {262, "NVS031APL"},
            {263, "NVS054APL"},
            {264, "NVS032APL"},
            {265, "NVS058APL"},
            {266, "NVS017APL"},
            {267, "NVS025APL"},
            {268, "NVS047APL"},
            {269, "NVS055APL"},
            {270, "NVS071APL"},
            {271, "NVS011APL"},
            {272, "NVS034APL"},
            {274, "NVS015APL"},
            {333, "NVS005BPL"},
            {403, "NVS006APL"},
            {1548, "NVS201ASS"},
            {1549, "NVS201BSS"},
            {1572, "NVS101BSS"},
            {2657, "NVS013APL"},
        };
    }
}