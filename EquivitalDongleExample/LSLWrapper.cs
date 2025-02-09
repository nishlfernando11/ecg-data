using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace ECGDataStream
{
    public class LSLWrapper
    {

        // Declare the P/Invoke function for LSL
        [DllImport("lsl.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern IntPtr lsl_create_streaminfo(string name, string type, int channel_count, double nominal_srate, int channel_format, string source_id);

        [DllImport("lsl.dll", EntryPoint = "lsl_push_sample_d", CallingConvention = CallingConvention.Cdecl)]
        public static extern void lsl_push_sample(IntPtr outlet, double[] sample);

        [DllImport("lsl.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern IntPtr lsl_create_outlet(IntPtr stream_info, int buffer_size);


   
        private IntPtr CreateStream(string name, string type, int channel_count, double nominal_srate, int channel_format, string source_id)
        {
            // Create a StreamInfo for LSL
            IntPtr streamInfo = lsl_create_streaminfo(name, type, channel_count, nominal_srate, channel_format, source_id);

            // Additional logic for working with LSL (e.g., sending data, streaming)
            Console.WriteLine("StreamInfo created successfully.");
            return streamInfo;
        }
        //

        public IntPtr CreateECGStream()
        {
            string name = "EQ_ECG_Stream", type = "ECG";
            int channel_count = 1;
            double nominal_srate = 256;
            int channel_format = 2;

            //0: float32(32 - bit floating - point numbers) — Most commonly used for sensor data like ECG, HR, etc.
            //1: int32(32 - bit integers)
            //2: double(64 - bit floating - point numbers)

            string source_id = "Equivital";

            return CreateStream(name, type, channel_count, nominal_srate, channel_format, source_id);
        }

        public IntPtr CreateHRStream()
        {
            string name = "EQ_HR_Stream", type = "HR";
            int channel_count = 1;
            double nominal_srate = 0.2;
            int channel_format = 2;

            //0: float32(32 - bit floating - point numbers) — Most commonly used for sensor data like ECG, HR, etc.
            //1: int32(32 - bit integers)
            //2: double(64 - bit floating - point numbers)

            string source_id = "Equivital";

            return CreateStream(name, type, channel_count, nominal_srate, channel_format, source_id);
        }

        public IntPtr CreateOutlet(IntPtr streamInfo)
        {
            IntPtr outlet = lsl_create_outlet(streamInfo, 0);
            return outlet;
        }

        public void PushSample(IntPtr outlet, double[] data)
        {
            lsl_push_sample(outlet, data);
        }
    }
}
