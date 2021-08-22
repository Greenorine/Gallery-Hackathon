using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.Serialization.Formatters.Binary;
using System.Threading;
using System.Threading.Tasks;
using Gallery_Hackathon.Models;
using Parquet;
using Parquet.Data;

namespace Gallery_Hackathon.Services
{
    public static class ParquetManager
    {
        public static PredictModel ProcessCrowdItems(string folderPath)
        {
            var otsPerAdvertSum = 0;
            var crowdsInfo = new DirectoryInfo(folderPath);
            using var resetEvent = new ManualResetEvent(false);
            var parallelOptions = new ParallelOptions {MaxDegreeOfParallelism = 6};
            var checkedTotal = 0;
            Parallel.ForEach(crowdsInfo.GetDirectories(), parallelOptions, folder =>
            {
                var id = int.Parse(folder.Name.Substring(7));
                foreach (var parquet in folder.GetFiles()
                    .Where(file => file.Name.Substring(file.Name.LastIndexOf('.')) == ".parquet"))
                {
                    using Stream fileStream = File.OpenRead(parquet.FullName);
                    using var parquetReader = new ParquetReader(fileStream);
                    var dataFields = parquetReader.Schema.GetDataFields();
                    var advertDictionary = new Dictionary<string, int>();
                    for (var i = 0; i < parquetReader.RowGroupCount; i++)
                        ProcessRow(parquetReader, dataFields, id, i, advertDictionary);
                    otsPerAdvertSum += advertDictionary.Select(x => x.Value).Sum() / advertDictionary.Count;
                    GC.Collect();
                    checkedTotal++;
                    Console.WriteLine($"[{checkedTotal}] Checked {parquet.FullName}");
                }
            });

            var otsPerAdvert = otsPerAdvertSum / checkedTotal;
            var hourModifiers = ProcessHoursMiddleValues();
            var dayOfWeekModifiers = ProcessMiddleValuesByType(EProcessType.DayOfWeek);
            var monthModifiers = ProcessMiddleValuesByType(EProcessType.Month);
            var playerModifiers = ProcessMiddleValuesByType(EProcessType.Player);

            Console.WriteLine("ADVERTS");
            Console.WriteLine(otsPerAdvert);

            Console.WriteLine("HOURS");
            hourModifiers.ToList().ForEach(x => Console.WriteLine($"{x.Key}: {x.Value}"));
            Console.WriteLine();

            Console.WriteLine("DAY OF WEEKS");
            dayOfWeekModifiers.ToList().ForEach(x => Console.WriteLine($"{x.Key}: {x.Value}"));
            Console.WriteLine();

            Console.WriteLine("MONTHS");
            monthModifiers.ToList().ForEach(x => Console.WriteLine($"{x.Key}: {x.Value}"));
            Console.WriteLine();

            Console.WriteLine("PLAYERS");
            playerModifiers.ToList().ForEach(x => Console.WriteLine($"{x.Key}: {x.Value}"));
            Console.WriteLine();
            
            return new PredictModel(dayOfWeekModifiers, monthModifiers, hourModifiers, playerModifiers, otsPerAdvert);
        }

        private const string MacField = "Mac";
        private const string AddedOnTickField = "AddedOnTick";
        private const string FrameOidField = "FrameOid";

        private static void ProcessRow(ParquetReader reader, IEnumerable<DataField> dataFields, int id, int index,
            IDictionary<string, int> advertDictionary)
        {
            using var groupReader = reader.OpenRowGroupReader(index);
            var columns = dataFields.Where(field => field.Name is MacField or AddedOnTickField or FrameOidField)
                .Select(groupReader.ReadColumn).ToArray();
            for (var j = 0; j < columns[0].Data.Length; j++)
            {
                var mac = ((string[]) columns[0].Data)[j];
                var dateTime = ((long?[]) columns[1].Data)[j];
                var frameOid = ((string[]) columns[2].Data)[j];
                var crowdItem = new CrowdItem(id, dateTime / 1000, mac, frameOid);
                ProcessCrowdItem(crowdItem, advertDictionary);
            }
        }

        private static Dictionary<int, decimal> ProcessHoursMiddleValues()
        {
            var result = new Dictionary<int, decimal>();
            var temp = new int[24];

            var hourViewsKeyValues = PlayerDateToHoursDictionary.Values
                .SelectMany(x => x).ToList();

            foreach (var hourViewsKeyValue in hourViewsKeyValues)
                temp[hourViewsKeyValue.Key] += hourViewsKeyValue.Value;

            for (var i = 0; i < 24; i++)
            {
                var count = hourViewsKeyValues.Count(x => x.Key == i);
                if (count == 0) continue;
                result[i] = (decimal) temp[i] / hourViewsKeyValues.Count(x => x.Key == i);
            }

            var middleHours = result.Select(x => x.Value).Sum() / result.Count;
            foreach (var valuePair in result.ToList())
                result[valuePair.Key] = decimal.Round(result[valuePair.Key] / middleHours, 3);
            return result;
        }

        private enum EProcessType
        {
            DayOfWeek,
            Player,
            Month
        }

        private static Dictionary<int, decimal> ProcessMiddleValuesByType(EProcessType type)
        {
            var result = new Dictionary<int, decimal>();

            var groups = PlayerDateToHoursDictionary
                .GroupBy(x => type switch
                {
                    EProcessType.DayOfWeek => (int) DateTime.ParseExact(x.Key.Substring(0, 10),
                        "dd/MM/yyyy", null).DayOfWeek,
                    EProcessType.Month => DateTime.ParseExact(x.Key.Substring(0, 10),
                        "dd/MM/yyyy", null).Month,
                    EProcessType.Player => int.Parse(x.Key.Substring(11)),
                    _ => 0
                });

            foreach (var group in groups)
            {
                var divider = type switch
                {
                    EProcessType.Month or EProcessType.Player => 1,
                    EProcessType.DayOfWeek => group.Count(),
                    _ => 1
                };

                result[group.Key] = (decimal) group.Select(x => x.Value)
                    .SelectMany(x => x).Select(x => x.Value).Sum() / divider;
            }

            var middleValue = result.Select(x => x.Value).Sum() / result.Count;
            foreach (var valuePair in result.ToList())
                result[valuePair.Key] = decimal.Round((decimal) result[valuePair.Key] / middleValue, 3);
            return result;
        }

        private static readonly Dictionary<string, Dictionary<int, int>> PlayerDateToHoursDictionary = new();


        private static void ProcessCrowdItem(CrowdItem item, IDictionary<string, int> advertDictionary)
        {
            if (item.AdvertId == null || item.MacAddress == "02:00:00:00:00:00") return;

            if (!advertDictionary.ContainsKey(item.AdvertId))
                advertDictionary.Add(item.AdvertId, 1);
            else advertDictionary[item.AdvertId]++;

            var key = $"{item.CreatedOn.ToShortDateString()}/{item.PlayerId}";
            if (!PlayerDateToHoursDictionary.TryGetValue(key, out var hoursToViewsDictionary))
            {
                PlayerDateToHoursDictionary[key] = new Dictionary<int, int> {{item.Hour, 1}};
                return;
            }

            if (!hoursToViewsDictionary.TryGetValue(item.Hour, out _))
                hoursToViewsDictionary[item.Hour] = 1;
            else hoursToViewsDictionary[item.Hour]++;
        }
    }
}