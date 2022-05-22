using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ActressMas;
using CsvHelper;
using System.IO;
using System.Globalization;

namespace Coursework
{
    class LoggingAgent : Agent
    {
        string filepath = Directory.GetCurrentDirectory() +"log.csv";
        public LoggingAgent()
        {

        }
        private string UniqPath()
        {
            string dir = Path.GetDirectoryName(filepath);
            string filename = Path.GetFileNameWithoutExtension(filepath);
            string fileExt = Path.GetExtension(filepath);
            for (int i = 1; ; i++)
            {
                if (!File.Exists(filepath))
                    return filepath;
                filepath = Path.Combine(dir, filename + "" + i + fileExt);
            }
        }
        public override void Setup()
        {
            filepath = UniqPath();
            var csv = new StringBuilder();
            var newLine = $"Name,Type,Revenue,Initial Energy Needs, Renewable Energy Bought,Renewable Energy Sold, Utility Bought, Utility Sold, Utility Price,CurRate,UpdateRate,FinalRate";
            csv.AppendLine(newLine);
            File.AppendAllText(filepath, csv.ToString());
        }

        public override void Act(Message message)
        {
            try
            {
                message.Parse(out string action, out List<string> parameters);
                switch (action)
                {
                    case "log":
                        var csv = new StringBuilder();
                        var newLine = $"{parameters[0]},{parameters[1]},{parameters[2]},{parameters[3]},{parameters[4]},{parameters[5]},{parameters[6]},{parameters[7]},{parameters[8]},{parameters[9]},{parameters[10]},{parameters[11]}";
                        csv.AppendLine(newLine);
                        File.AppendAllText(filepath, csv.ToString());
                        break;
                    case "end":
                       
                        Console.WriteLine($"Total number of messages sent: {Log.messages}");
                        
                        break;
                }


            }
            catch(Exception e)
            {

            }
        }
    }
}
