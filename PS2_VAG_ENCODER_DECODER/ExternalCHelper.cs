using System;
using System.Runtime.InteropServices;

namespace PS2_VAG_ENCODER_DECODER
{
    class ExternalCHelper
    {
        [DllImport("Includes\\psxsdk.dll", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.Cdecl)]
        public static extern IntPtr get_vag_file([MarshalAs(UnmanagedType.LPStr)] string in_file, [MarshalAs(UnmanagedType.LPStr)] string out_file, int in_sample_freq);
    }
}
