using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PS2_VAG_ENCODER_DECODER
{
    internal static class PS2_VAG_Format
    {
        public static byte[] DecodeVAG_ADPCM(byte[] VagFileData, int NumSamples)
        {
            byte[] outp;

            using (MemoryStream DecodedData = new MemoryStream())
            {
                using (BinaryWriter BWriter = new BinaryWriter(DecodedData))
                {
                    int[] unpacked_nibbles = new int[NumSamples * 2];

                    for (int i = 0; i < NumSamples; i++)
                    {
                        short sample_byte = VagFileData[i];

                        unpacked_nibbles[i * 2] = (sample_byte & 0x0F) >> 0;
                        unpacked_nibbles[i * 2 + 1] = (sample_byte & 0xF0) >> 4;
                    }

                    for (int j = 0; j < 28; j++)
                    {
                        int s = unpacked_nibbles[j] << 12;

                        if (Convert.ToBoolean(s & 0x8000))
                        {
                            s = (int)(s | 0xFFFF0000);
                        }
                    }
                }
                outp = DecodedData.ToArray();
            }
            return outp;
        }
    }
}
