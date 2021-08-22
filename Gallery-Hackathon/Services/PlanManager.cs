using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;
using Excel = Microsoft.Office.Interop.Excel;
using Gallery_Hackathon.Models;

namespace Gallery_Hackathon.Services
{
    public static class PlanManager
    {
        public static bool LoadPlanIntoFile(string filePath, List<OtsPredict> otsPredicts, PlanInputData inputData,
            PredictModel predictModel, int planPage, int predictPage, int fromPlanRow, int toPlanRow,
            int fromPredictRow, int toPredictRow, int fromPredictColumn, int toPredictColumn)
        {
            var app = new Excel.Application();
            var book = app.Workbooks.Open(filePath);
            var planSheet = (Excel.Worksheet) book.Sheets[planPage];
            var totalOts = 0;

            for (var i = fromPlanRow; i < toPlanRow; i++)
            {
                var date = DateTime.ParseExact(planSheet.Range[$"A{i}", Missing.Value].Text.ToString(),
                    "yyyy-MM-dd", null);
                var playerNumber = planSheet.Range[$"D{i}", Missing.Value].Text.ToString();
                var playerId = Program.PlayerIdToNumber.Where(x => x.Value == playerNumber)
                    .Select(x => x.Key).FirstOrDefault();

                if (!inputData.Players.Contains(playerId) || date.Day < inputData.StartDate.Day ||
                    date.Day > inputData.EndDate.Day) continue;

                var localInventory = otsPredicts.FirstOrDefault(x =>
                    x.PlayerId == playerId && x.Date.ToShortDateString() == date.ToShortDateString());
                if (localInventory == null) continue;

                var localHours = new int[24];
                var predictOts = 0;

                for (var j = inputData.StartTime.Hours; j <= inputData.EndTime.Hours; j++)
                {
                    if (totalOts >= inputData.NeedOts)
                    {
                        book.Save();
                        book.Close();
                        app.Quit();
                        if (totalOts > inputData.NeedOts)
                            Console.WriteLine(
                                $"Подобран план, превышающий запрос на {totalOts - inputData.NeedOts} OTS.");
                        return true;
                    }

                    var possibleShows = Math.Min(inputData.AdvertFrequency, localInventory.ShowsData[j].BasicShows);
                    var localOts = (int) (possibleShows * localInventory.ShowsData[j].Multiplier);
                    totalOts += localOts;
                    predictOts += localOts;
                    localHours[j] = possibleShows;
                }

                var range = planSheet.Range[$"G{i}", $"AD{i}"];
                range.Value = localHours;
                book.Save();

                var predictSheet = (Excel.Worksheet) book.Sheets[predictPage];
                var array = (Array) predictSheet.Range[$"B{fromPredictRow}:B{toPredictRow}", Missing.Value].Value;
                var players = array.OfType<object>().Select(o => o.ToString()).ToList();
                array = (Array) predictSheet
                    .Range[$"{GetColumnName(fromPredictColumn)}6:{GetColumnName(toPredictColumn)}6", Missing.Value]
                    .Value;
                var dates = array.OfType<object>().Select(o => o.ToString()).ToList();
                var offsetX = dates.FindLastIndex(x =>
                    DateTime.ParseExact(x, "dd.MM.yyyy h:mm:ss", null).ToShortDateString() == date.ToShortDateString());
                var offsetY = players.FindLastIndex(x => x == playerNumber);
                predictSheet.Range[$"{GetColumnName(3 + offsetX)}{8 + offsetY}", Missing.Value].Value = predictOts;
            }

            book.Save();
            book.Close();
            app.Quit();
            Console.WriteLine(
                $"Подобран план, не достающий запроса на {inputData.NeedOts - totalOts} OTS.");
            return false;
        }

        public static List<OtsPredict> CountOts(string inventoryPath, int sheetPage, PredictModel predictModel,
            int fromRow, int toRow)
        {
            var result = new List<OtsPredict>();

            var app = new Excel.Application();
            var book = app.Workbooks.Open(inventoryPath);
            var sheet = (Excel.Worksheet) book.Sheets[sheetPage];

            for (var i = fromRow; i < toRow; i++)
            {
                var date = DateTime.ParseExact(sheet.Range[$"A{i}", Missing.Value].Text.ToString(),
                    "yyyy-MM-dd", null);
                var playerNumber = sheet.Range[$"D{i}", Missing.Value].Text.ToString();
                var playerId = Program.PlayerIdToNumber.Where(x => x.Value == playerNumber)
                    .Select(x => x.Key).FirstOrDefault();

                if (playerId == 0) continue;

                var array = (Array) sheet.Range[$"G{i}:AD{i}", Missing.Value].Value;
                var hours = array.OfType<object>().Select(o => int.Parse(o.ToString())).ToArray();

                var ots = new Shows[24];
                for (var j = 0; j < 24; j++)
                {
                    ots[j] = new Shows(hours[j],
                        predictModel.AverageOtsPerAdvert
                        * predictModel.DayOfWeekModifiers[(int) date.DayOfWeek]
                        * predictModel.HourModifiers[j]
                        * predictModel.PlayerModifiers[playerId]);
                }

                result.Add(new OtsPredict(date, playerId, ots));
            }

            return result;
        }

        private static string GetColumnName(int index)
        {
            const string letters = "ABCDEFGHIJKLMNOPQRSTUVWXYZ";

            var value = "";

            if (index >= letters.Length)
                value += letters[index / letters.Length - 1];

            value += letters[index % letters.Length];

            return value;
        }
    }
}