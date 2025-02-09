using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using System.IO;


namespace ECGDataStream
{
    public class Config
    {
        public static void LoadEnvVariables(string filePath)
        {
            foreach (var line in File.ReadAllLines(filePath))
            {
                if (line.Contains("="))
                {
                    var parts = line.Split(new char[] { '=' }, 2);
                    Environment.SetEnvironmentVariable(parts[0].Trim(), parts[1].Trim());
                }
            }
        }
    }

}
