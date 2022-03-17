using System.IO;
using System.Text;

namespace PS2VagTool.Vag_Functions
{
    //-------------------------------------------------------------------------------------------------------------------------------
    //-------------------------------------------------------------------------------------------------------------------------------
    //-------------------------------------------------------------------------------------------------------------------------------
    public static partial class SonyVag
    {
        //-------------------------------------------------------------------------------------------------------------------------------
        internal static void WriteVagFile(byte[] vagData, string outputFilePath, int numOfChannels, int samplingFrequency)
        {
            using (BinaryWriter BinWriter = new BinaryWriter(File.Open(outputFilePath, FileMode.Create, FileAccess.Write, FileShare.Read), Encoding.ASCII))
            {
                //Magic 
                BinWriter.Write(Encoding.ASCII.GetBytes("VAGp"));
                //Version
                BinWriter.Write(ProgramFunctions.FlipInt32(32));
                //Reserved area
                BinWriter.Write(0);
                //Waveform data size (bytes)
                BinWriter.Write(ProgramFunctions.FlipInt32(vagData.Length + 16));
                //Sampling Frequency (Hz)
                BinWriter.Write(ProgramFunctions.FlipInt32(samplingFrequency));
                //Reserved area 
                BinWriter.Write(new byte[10]);
                //Number of channels
                BinWriter.Write((byte)((numOfChannels > 1) ? 2 : 0));
                //Reserved area
                BinWriter.Write((byte)0);
                //BinWriter.Write(0);
                BinWriter.Write(Encoding.ASCII.GetBytes(outputFilePath.Truncate(16)));
                BinWriter.Write(new byte[16]);
                BinWriter.Write(vagData);
            }
        }
    }

    //-------------------------------------------------------------------------------------------------------------------------------
}
