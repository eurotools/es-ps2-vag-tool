using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace PS2_VAG_ENCODER_DECODER
{
    public partial class Frm_Main : Form
    {
        public Frm_Main()
        {
            InitializeComponent();
        }

        private void Button_Search_Encode_Click(object sender, EventArgs e)
        {
            string SelectedFile = FileBrowserDialog("VAG Files (*.VAG)|*.vag", 0, true);
            if (File.Exists(SelectedFile))
            {
                Textbox_File_To_Encode.Text = SelectedFile;
            }
        }

        private void Button_Search_Decode_Click(object sender, EventArgs e)
        {
            string SelectedFile = FileBrowserDialog("VAG Files (*.VAG)|*.vag", 0, true);
            if (File.Exists(SelectedFile))
            {
                Textbox_File_To_Decode.Text = SelectedFile;

                byte[] FileData = File.ReadAllBytes(SelectedFile);
                byte[] PCMData = PS2_VAG_Format.DecodeVAG_ADPCM(FileData, FileData.Length * 2);
                CreateWavFile(22050, 16, 1, PCMData, Application.StartupPath + "\\" + Path.GetFileNameWithoutExtension(SelectedFile) + ".wav");
            }
        }

        //*===============================================================================================
        //* FUNCTIONS
        //*===============================================================================================
        private string FileBrowserDialog(string BrowserFilter, int SelectedIndexFilter, bool ForceSpecifiedFilter)
        {
            string FilePath = string.Empty;

            using (OpenFileDialog FileBrowser = new OpenFileDialog())
            {
                if (ForceSpecifiedFilter)
                {
                    FileBrowser.Filter = BrowserFilter;
                }
                else
                {
                    FileBrowser.Filter = BrowserFilter + "|All files(*.*)|*.*";
                }
                FileBrowser.FilterIndex = SelectedIndexFilter;

                if (FileBrowser.ShowDialog() == DialogResult.OK)
                {
                    FilePath = FileBrowser.FileName;
                }
            }

            return FilePath;
        }

        internal static void CreateWavFile(int Frequency, int BitsPerChannel, int NumberOfChannels, byte[] PCMData, string FilePath)
        {
            using (FileStream WavFile = new FileStream(FilePath, FileMode.Create))
            {
                using (BinaryWriter BWritter = new BinaryWriter(WavFile))
                {
                    //Write WAV Header
                    BWritter.Write(Encoding.UTF8.GetBytes("RIFF")); //Chunk ID
                    BWritter.Write((uint)(36 + PCMData.Length)); //Chunk Size
                    BWritter.Write(Encoding.UTF8.GetBytes("WAVE")); //Format
                    BWritter.Write(Encoding.UTF8.GetBytes("fmt ")); //Subchunk1 ID
                    BWritter.Write((uint)16); //Subchunk1 Size
                    BWritter.Write((ushort)1); //Audio Format
                    BWritter.Write((ushort)NumberOfChannels); //Num Channels
                    BWritter.Write((uint)(Frequency)); //Sample Rate
                    BWritter.Write((uint)((Frequency * NumberOfChannels * BitsPerChannel) / 8)); //Byte Rate
                    BWritter.Write((ushort)((NumberOfChannels * BitsPerChannel) / 8)); //Block Align
                    BWritter.Write((ushort)(BitsPerChannel)); //Bits Per Sample
                    BWritter.Write(Encoding.UTF8.GetBytes("data")); //Subchunk2 ID
                    BWritter.Write((uint)PCMData.Length); //Subchunk2 Size

                    //Write PCM Data
                    for (int i = 0; i < PCMData.Length; i++)
                    {
                        BWritter.Write(PCMData[i]);
                    }

                    //Close Writter
                    BWritter.Close();
                }

                //Close File
                WavFile.Close();
            }
        }
    }
}
