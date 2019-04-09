using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;

namespace CrimeAnalyzer
{
    class Program
    {
        public class Crimes
        {
            private static List<CrimeData> CrimeDataList = new List<CrimeData>();
            public Crimes(string[] args)
            {
                string csvFilePath = string.Empty;
                string reportFilePath = string.Empty;
                string startupPath = Directory.GetCurrentDirectory();

                if (Debugger.IsAttached) //Debug mode
                {
                    csvFilePath = Path.Combine(startupPath, "CrimeData.csv");
                    reportFilePath = Path.Combine(startupPath, "CrimeReport.txt");
                } 
                else // Release Mode
                {
                    if (args.Length != 2)
                    {
                        Console.WriteLine("Invalid.\nValid example : CrimeAnalyzer <crime_csv_file_path> <report_file_path>");
                        Console.ReadLine();
                        return;
                    }
                    else
                    {
                        csvFilePath = args[0];
                        reportFilePath = args[1];
                        if (!csvFilePath.Contains("\\"))
                        {
                            csvFilePath = Path.Combine(startupPath, csvFilePath);
                        }
                        if (!reportFilePath.Contains("\\"))
                        {                        
                            reportFilePath = Path.Combine(startupPath, reportFilePath);
                        }
                    }
                }
                if (File.Exists(csvFilePath))
                {
                    if (ReadData(csvFilePath))
                    {
                        try
                        {
                            var file = File.Create(reportFilePath);
                            file.Close();
                        }
                        catch (Exception)
                        {
                            Console.WriteLine($"Unable to create report file at : {reportFilePath}");
                        }
                        WriteReport(reportFilePath);
                    }
                }
                else
                {
                    Console.Write($"Crime data file does not exist at path: {csvFilePath}");
                }
                Console.ReadLine();
            }
            private static bool ReadData(string filePath)
            {
                try
                {
                    int columns = 0;
                    string[] crimeDataLines = File.ReadAllLines(filePath);
                    for (int index = 0; index < crimeDataLines.Length; index++)
                    {
                        string crimeDataLine = crimeDataLines[index];
                        string[] data = crimeDataLine.Split(',');
                        if (index == 0) // Header
                        {
                            columns = data.Length;
                        }
                        else
                        {
                            if (columns != data.Length)
                            {
                                Console.WriteLine($"Row {index} contains {data.Length} values. It should contain {columns}.");
                                return false;
                            }
                            else
                            {
                                try                                    
                                {
                                    CrimeData crimeData = new CrimeData();
                                    crimeData.Year = Convert.ToInt32(data[0]);
                                    crimeData.Population = Convert.ToInt32(data[1]);
                                    crimeData.Murders = Convert.ToInt32(data[2]);
                                    crimeData.Rapes = Convert.ToInt32(data[3]);
                                    crimeData.Robberies = Convert.ToInt32(data[4]);
                                    crimeData.ViolentCrimes = Convert.ToInt32(data[5]);
                                    crimeData.Thefts = Convert.ToInt32(data[6]);
                                    crimeData.MotorVehicleThefts = Convert.ToInt32(data[7]);
                                    CrimeDataList.Add(crimeData);
                                }
                                catch (InvalidCastException e)
                                {
                                    Console.WriteLine($"Row {index} contains invalid value.");
                                    return false;
                                }
                            }
                        }
                    }
                    return true;
                }
                catch (Exception ex)
                {
                    Console.WriteLine("Error in reading data from csv file.");
                    throw ex;
                }
            }
            private static void WriteReport(string filePath)
            {
                try
                {
                    if (CrimeDataList != null && CrimeDataList.Any())
                    {
                        StringBuilder sb = new StringBuilder();
                        sb.Append("Crime Analyzer Report");
                        sb.Append(Environment.NewLine);
                        // Period
                        int minYear = CrimeDataList.Min(x => x.Year);                    
                        int maxYear = CrimeDataList.Max(x => x.Year);
                        // Years
                        int years = maxYear - minYear + 1;
                        sb.Append($"Period: {minYear}-{maxYear} ({years} years)");
                        sb.Append(Environment.NewLine);

                        // Q2
                        var mYears = from crimeData in CrimeDataList
                                     where crimeData.Murders < 15000
                                     select crimeData.Year;

                        string mYearsStr = string.Empty;
                        for (int i = 0; i < mYears.Count(); i++)
                        {
                            mYearsStr += mYears.ElementAt(i).ToString();
                            // No comma after last value
                            if (i < mYears.Count() - 1) mYearsStr += ", ";
                        }

                        sb.Append($"Years murders per year < 15000: {mYearsStr}");
                        sb.Append(Environment.NewLine);
                        // 3
                        var rYears = from crimeData in CrimeDataList
                                     where crimeData.Robberies > 500000
                                     select crimeData;

                        string rYearsStr = string.Empty;
                        for (int i = 0; i < rYears.Count(); i++)
                        {
                            CrimeData crimeData = rYears.ElementAt(i);
                            rYearsStr += $"{crimeData.Year} = {crimeData.Robberies}";
                            if (i < rYears.Count() - 1) rYearsStr += ", ";
                        }

                        sb.Append($"Robberies per year > 500000: {rYearsStr}");
                        sb.Append(Environment.NewLine);

                        // 4
                        var vCrime = from crimeData in CrimeDataList
                                     where crimeData.Year == 2010
                                     select crimeData;
                        CrimeData vCrimeData = vCrime.First();
                        double vCrimePerCapita = (double)vCrimeData.ViolentCrimes / (double)vCrimeData.Population;                   
                        sb.Append($"Violent crime per capita rate (2010): {vCrimePerCapita}");
                        sb.Append(Environment.NewLine);

                        // 5
                        double avgMurders = CrimeDataList.Sum(x => x.Murders) / CrimeDataList.Count;
                        sb.Append($"Average murder per year (all years): {avgMurders}");
                        sb.Append(Environment.NewLine);

                        // 6
                        int murders1 = CrimeDataList
                        .Where(x => x.Year >= 1994 && x.Year <= 1997)                    
                        .Sum(y => y.Murders);
                        double avgMurders1 = murders1 / CrimeDataList.Count;
                        sb.Append($"Average murder per year (1994-1997): {avgMurders1}");
                        sb.Append(Environment.NewLine);

                        // 7
                        int murders2 = CrimeDataList
                        .Where(x => x.Year >= 2010 && x.Year <= 2014)
                        .Sum(y => y.Murders);
                        double avgMurders2 = murders2 / CrimeDataList.Count;
                        sb.Append($"Average murder per year (2010-2014): {avgMurders2}");
                        sb.Append(Environment.NewLine);

                        // 8
                        int minTheft = CrimeDataList
                        .Where(x => x.Year >= 1999 && x.Year <= 2004)
                        .Min(x => x.Thefts);
                        sb.Append($"Minimum thefts per year (1999-2004): {minTheft}");
                        sb.Append(Environment.NewLine);

                        // 9
                        int maxTheft = CrimeDataList
                        .Where(x => x.Year >= 1999 && x.Year <= 2004)
                        .Max(x => x.Thefts);
                        sb.Append($"Maximum thefts per year (1999-2004): {maxTheft}");
                        sb.Append(Environment.NewLine);

                        // 10
                        int yMaxVehicleTheft = CrimeDataList.OrderByDescending(x => x.MotorVehicleThefts).First().Year;
                        sb.Append($"Year of highest number of motor vehicle thefts: {yMaxVehicleTheft}");
                        sb.Append(Environment.NewLine);
                        using (var stream = new StreamWriter(filePath))
                        {
                            stream.Write(sb.ToString());
                        }
                        Console.WriteLine();
                        Console.WriteLine(sb.ToString());
                    }
                    else
                    {
                        Console.WriteLine($"No data to write.");
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine("Error in writing report file.");
                    throw ex;
                }
            }
        }
        public class CrimeData
        {
            public int Year { get; set; }
            public int Population { get; set; }
            public int Murders { get; set; }
            public int Rapes { get; set; }
            public int Robberies { get; set; }
            public int ViolentCrimes { get; set; }
            public int Thefts { get; set; }
            public int MotorVehicleThefts { get; set; }
        }

        static void Main(string[] args)
        {
            Crimes crimes = new Crimes(args);
        }
    }
   
    
}
