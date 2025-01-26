using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using SemParserLibrary;

using System.IO;
using System;
using System.Xml.Linq;
using DotNetEnv;

namespace ECGDataManager
{
    class Program
    {

        private readonly DatabaseManager _dbManager;
        public string sessionId;

        public Program()
        {
            _dbManager = new DatabaseManager();
        }

        static void Main(string[] args)
        {

            try
            {
                string baseDirectory = AppDomain.CurrentDomain.BaseDirectory;

                // Construct the full path to the .env file
                string envFilePath = Path.Combine(baseDirectory, "..", "..", "..", ".env");
                //Console.WriteLine(envFilePath);
                // Get the directory of the executing assembly
       

                DotNetEnv.Env.Load(envFilePath);
                Console.WriteLine(".env file loaded successfully.");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error loading .env file: {ex.Message}");
            }


            Program program = new Program();

            string licenseKey = Environment.GetEnvironmentVariable("EQ_LICENSE_KEY");
            string devName = Environment.GetEnvironmentVariable("EQ_DEV_NAME");
            if (string.IsNullOrWhiteSpace(licenseKey) || string.IsNullOrWhiteSpace(devName))
            {
                Console.WriteLine("Environment variables are not set or empty.");
            }
            else
            {
                //Console.WriteLine($"Developer Name: {devName}");
            }

            Console.Write("Enter Session ID: ");
            string sessionID = Console.ReadLine();
            Console.Write("Enter Player Name/ID: ");
            string playerName = Console.ReadLine();


            sessionID = GenerateCustomSessionId(sessionID);
            program.sessionId = sessionID; //assign to static 

            Console.WriteLine($"customSessionId: {sessionID}");
            // create new session
            var sessionObj = new Dictionary<string, object>{
                { "session_id", sessionID },
                { "player_name", playerName },
                { "start_time", DateTime.UtcNow },
            };

            program.createSession(sessionObj);


            try
            {
                // Register SDK
                SemDevice.License.DeveloperName = devName; 
                SemDevice.License.LicenseCode = licenseKey;

                // Create a real-time "canned data" connection. 
                ISemConnection cannedConnection = SemRealTimeFileConnection.CreateConnection();

                // Create a decoder instance.
                SemDevice device = new SemDevice();


                // We're interested in heart rate and timer messages.
                device.HeartRateDataReceived += program.DeviceHeartRateDataReceived;
                device.ECGDataReceived += program.DeviceECGDataReceived;
                device.AccelerometerDataReceived += program.DeviceAccelerometerDataReceived;
                device.RawDataReceived += program.DeviceRawDataReceived;
                device.SynchronisationTimerDataReceived += program.DeviceSynchronisationTimerDataReceived;

                // Start the decoder.
                Console.WriteLine("Press any key to terminate the application.\r\n");
                device.Start(cannedConnection);
            }
            catch (BadLicenseException)
            {
                Console.WriteLine("Please enter a valid developer name and license code!");
            }
            catch (Exception e)
            {
                Console.WriteLine("An error occurred while executing this example.\r\n\r\n" + e.Message);
            }

            Console.ReadLine();
        }

        public void createSession(object sessionObj)
        {
            _dbManager.InsertData("sessions", sessionObj);
        }


        //public void updateSession(object sessionObj)
        //{
        //    //_dbManager.UpdatetData("sessions", sessionObj);
        //}

        public void DeviceSynchronisationTimerDataReceived(object sender, SyncrhonisationTimerEventArgs e)
        {
            DateTime sessionTime = correctedSesstionTime(e.SessionTime); // Assuming e.SessionTime is a DateTime with 1980 default
            DateTime currentDateWithSessionTime = DateTime.Today.Add(sessionTime.TimeOfDay);
            Console.WriteLine(currentDateWithSessionTime.ToString("yyyy-MM-dd HH:mm:ss"));

            Console.WriteLine(e + " (" + currentDateWithSessionTime.ToString("yyyy-MM-dd HH:mm:ss") + ")");
            this.SaveData("SynchronisationTimerData", e + " (" + currentDateWithSessionTime.ToString("yyyy-MM-dd HH:mm:ss") + ")");

        }

        public void DeviceHeartRateDataReceived(object sender, HeartRateEventArgs e)
        {
            object heartRateData = new
            {
                session_id = this.sessionId,
                hr_bpm = e.BeatsPerMinute,
                session_time = correctedSesstionTime(e.SessionTime),
            };
            Console.WriteLine(e);
            Console.WriteLine(heartRateData);

            this.SaveData("HeartRateData", heartRateData);

        }

        public void DeviceECGDataReceived(object sender, ECGSemMessageEventArgs e)
        { 

            object ECGData = new
            {
                session_id = this.sessionId,
                lead_one_raw = e.LeadOneRaw,
                lead_two_raw = e.LeadTwoRaw,
                sequence_number = e.SequenceNumber,
                lead_one_mv = e.LeadOne_mV,
                lead_two_mv = e.LeadTwo_mV,
                ecg_timestamp = correctedSesstionTime(e.SessionTime), //session_time
            };
            Console.WriteLine(e);
            Console.WriteLine(ECGData);

            this.SaveData("ECGData", ECGData);
            // Stream ECG data and save to PostgreSQL


        }

        public void DeviceAccelerometerDataReceived(object sender, AccelerometerSemMessageEventArgs e)
        {

            object accelerometerData = new
            {
                session_id = this.sessionId,

                vertical_mg = e.Vertical_mG,
                lateral_mg = e.Lateral_mG,
                longitudinal_mg = e.Longitudinal_mG,
                resultant_mg = e.Resultant_mG,
                vertical_raw = e.VerticalRaw,
                lateral_raw = e.LateralRaw,
                longitudinal_raw = e.LongitudinalRaw,
                session_time = correctedSesstionTime(e.SessionTime),
            };
            Console.WriteLine(e);
            Console.WriteLine(accelerometerData);

            this.SaveData("AccelerometerData", accelerometerData);


        }

        public void DeviceRespirationRateDataReceived(object sender, RespirationRateEventArgs e)
        {

            object respirationRateData = new
            {
                BreathsPerMinute = e.BreathsPerMinute,
                SessionTime = correctedSesstionTime(e.SessionTime),
            };
            Console.WriteLine(e);
            Console.WriteLine(respirationRateData);

            SaveData("RespirationRateData", respirationRateData);


        }

        public void DeviceImpedanceRespirationDataReceived(object sender, ImpedanceRespirationEventArgs e)
        {

            object impedanceRespirationData = new
            {
                Impedance = e.Impedance,
                SessionTime = correctedSesstionTime(e.SessionTime),
            };
            Console.WriteLine(e);
            Console.WriteLine(impedanceRespirationData);

            this.SaveData("ImpedanceRespirationData", impedanceRespirationData);


        }

        public void DeviceSkinTemperatureDataReceived(object sender, SkinTemperatureEventArgs e)
        {

            object skinTemperatureData = new
            {
                TemperatureDeg = e.TemperatureDeg,
                SessionTime = correctedSesstionTime(e.SessionTime),
            };
            Console.WriteLine(e);
            Console.WriteLine(skinTemperatureData);

            this.SaveData("SkinTemperatureData", skinTemperatureData);


        }

        public void DeviceRawDataReceived(object sender, SemMessageEventArgs e)
        {
            Console.WriteLine(e);
            this.SaveData("RawData", e);


        }

        public void SaveData(string dataType, object data)
        {
            // Define the log file path
            string logFilePath = @"D:\EquivitalData\EquivitalLog.txt";
            Directory.CreateDirectory(Path.GetDirectoryName(logFilePath)); // Ensure directory exists

            // Append data to the file
            using (StreamWriter sw = new StreamWriter(logFilePath, true))
            {
                sw.WriteLine($"{DateTime.UtcNow:yyyy-MM-dd HH:mm:ss} | {dataType}: {data}");
            }

            Console.WriteLine($"Data appended to {logFilePath}");

            string tableName = null;

            switch (dataType)
            {
                case "ECGData":
                    tableName = "ecg_data";
                    break;
                case "HeartRateData":
                    tableName = "heart_rate_data";
                    break;
                case "AccelerometerData":
                    tableName = "accelerometer_data";
                    break;
                default:
                    break;
            }
            // Save to PostgreSQL database
            try
            {
                if (tableName != null)
                {
                    _dbManager.InsertData(tableName, data); // Access _dbManager directly
                    Console.WriteLine("Data saved to database successfully.");
                }

            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error saving to database: {ex.Message}");
            }
        }

        static DateTime correctedSesstionTime(DateTime sessionTime)
        {

            DateTime sessionStartTime = DateTime.UtcNow; // Actual session start time
            Console.WriteLine($"sessionStartTime {sessionStartTime}, sessionTime: {sessionTime}");

            return sessionStartTime;
        }

        public static string GenerateCustomSessionId(string userSessionId)
        {
            // Validate the input session ID (e.g., ensure it's not null/empty)
            if (string.IsNullOrWhiteSpace(userSessionId))
            {
                throw new ArgumentException("Session ID cannot be null or empty.");
            }

            // Get the current timestamp in a compact format (e.g., YYYYMMDDHHMMSS)
            string timestamp = DateTime.UtcNow.ToString("yyyyMMddHHmmss");

            // Combine the user-provided session ID and timestamp
            string customSessionId = $"{userSessionId}-{timestamp}";

            return customSessionId;
        }


    }
}
