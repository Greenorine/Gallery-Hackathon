using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Serialization.Formatters.Binary;
using Gallery_Hackathon.Models;
using Parquet;
using Parquet.Data;

namespace Gallery_Hackathon.Services
{
    public static class ParquetManager
    {
        public static void ProcessCrowdItems(string folderPath)
        {
            var crowdsInfo = new DirectoryInfo(folderPath);
            var x = 0;
            foreach (var folder in crowdsInfo.GetDirectories())
            {
                var id = int.Parse(folder.Name.Substring(7));
                foreach (var parquet in folder.GetFiles()
                    .Where(file => file.Name.Substring(file.Name.LastIndexOf('.')) == ".parquet"))
                {
                    Console.WriteLine($"Checking {parquet.FullName}");
                    using Stream fileStream = File.OpenRead(parquet.FullName);
                    using var parquetReader = new ParquetReader(fileStream);
                    var dataFields = parquetReader.Schema.GetDataFields();

                    x++;
                    if (x != 6)
                        continue;
                    for (var i = 0; i < parquetReader.RowGroupCount; i++)
                        ProcessRow(parquetReader, dataFields, id, i);
                    if (x == 6)
                        return;
                }
            }
        }

        public static void ProcessRow(ParquetReader reader, DataField[] dataFields, int id, int index)
        {
            using var groupReader = reader.OpenRowGroupReader(index);
            var columns = dataFields.Where(field => field.Name is "Mac" or "AddedOnTick" or "FrameOid")
                .Select(groupReader.ReadColumn).ToArray();
            for (var j = 0; j < columns[0].Data.Length; j++)
            {
                var mac = ((string[]) columns[0].Data)[j];
                var dateTime = ((long?[]) columns[1].Data)[j];
                var frameOid = ((string[]) columns[2].Data)[j];
                var crowdItem = new CrowdItem(id, dateTime / 1000, mac, frameOid);
                ProcessCrowdItem(crowdItem);
                ProcessDays();
            }
        }

        public static Dictionary<int, Dictionary<string, Dictionary<int, int>>> PlayerToDatesDictionary = new();

        public static Dictionary<int, int> ProcessHoursMiddleValues()
        {
            var result = new Dictionary<int, int>();
            var temp = new int[24];
            var hourToViewsDictionary = PlayerToDatesDictionary.Values
                .SelectMany(x => x.Values)
                .SelectMany(hourToViews => hourToViews);
            var hourViewsKeyValues = hourToViewsDictionary.ToList();
            foreach (var hourViewsKeyValue in hourViewsKeyValues) 
                temp[hourViewsKeyValue.Key] += hourViewsKeyValue.Value;
            for (var i = 0; i < 24; i++)
                result[i] = temp[i] / hourViewsKeyValues.Count(x => x.Key == i);
            return result;
        }

        private static void ProcessDays()
        {
        }

        private static void ProcessCrowdItem(CrowdItem item)
        {
            var innerKey = $"{item.Day}/{item.Month}";
            if (!PlayerToDatesDictionary.TryGetValue(item.PlayerId, out var datesToHoursDictionary))
            {
                PlayerToDatesDictionary[item.PlayerId] = new Dictionary<string, Dictionary<int, int>>
                    {{innerKey, new Dictionary<int, int> {{item.Hour, 1}}}};
                return;
            }

            if (!datesToHoursDictionary.TryGetValue(innerKey, out var hoursToViewsDictionary))
            {
                datesToHoursDictionary[innerKey] = new Dictionary<int, int> {{item.Hour, 1}};
                return;
            }

            if (!hoursToViewsDictionary.ContainsKey(item.Hour))
            {
                hoursToViewsDictionary[item.Hour] = 1;
                return;
            }
                
            hoursToViewsDictionary[item.Hour]++;
        }
    }
}