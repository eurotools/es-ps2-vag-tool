using System;
using System.Runtime.InteropServices;

namespace PS2_VAG_ENCODER_DECODER
{
    class ExternalCHelper
    {
        [DllImport("Includes\\psxsdk.dll")]
        public static extern IntPtr get_vag_file(string in_file, string out_file, int in_sample_freq);

        [DllImport("Includes\\psxsdk.dll")]
        public static extern void find_predict(ref ushort samples, ref double[] d_samples, ref uint predict_nr, ref uint shift_factor);

        [DllImport("Includes\\psxsdk.dll")]
        public static extern void pack(ref double[] d_samples, ref ushort[] four_bit, uint predict_nr, uint shift_factor);
    }
}
