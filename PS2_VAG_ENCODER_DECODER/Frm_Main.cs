using System;
using System.IO;
using System.Text;
using System.Windows.Forms;

namespace PS2_VAG_ENCODER_DECODER
{
    public partial class Frm_Main : Form
    {
        private string _fileToDecode = "";
        private string _fileToEncode = "";

        public Frm_Main()
        {
            InitializeComponent();
            Button_Search_Encode.Enabled = false; // TODO: Remove once functionality is completed
            Button_Encode.Enabled = false; // TODO: Remove once functionality is completed
        }

        private void Button_Search_Encode_Click(object sender, EventArgs e)
        {
            string selectedFile = FileBrowserDialog("VAG Files (*.VAG)|*.vag", 0, true);
            if (File.Exists(selectedFile))
                Textbox_File_To_Encode.Text = _fileToEncode = selectedFile;
        }

        private void Button_Search_Decode_Click(object sender, EventArgs e)
        {
            string selectedFile = FileBrowserDialog("VAG Files (*.VAG)|*.VAG", 0, true);
            if (File.Exists(selectedFile))
                Textbox_File_To_Decode.Text = _fileToDecode = selectedFile;
        }

        private void Button_Decode_Click(object sender, EventArgs e)
        {
            var fileName = $"{Path.GetFileNameWithoutExtension(_fileToDecode)}.wav";
            var filePath = $"{Application.StartupPath}\\{fileName}";
            byte[] fileData = File.ReadAllBytes(_fileToDecode);
            byte[] pcmData = PS2_VAG_Format.DecodeVAG_ADPCM(fileData, 1);
            CreateWavFile(22050, 16, 1, pcmData, filePath);
            MessageBox.Show($"Exported WAV file to: {filePath}");
        }

        private void Button_Encode_Click(object sender, EventArgs e)
        {
            // TODO!
        }

        //*===============================================================================================
        //* FUNCTIONS
        //*===============================================================================================
        private string FileBrowserDialog(string browserFilter, int selectedIndexFilter, bool forceSpecifiedFilter)
        {
            string filePath = string.Empty;

            using (OpenFileDialog fileBrowser = new OpenFileDialog())
            {
                if (forceSpecifiedFilter)
                {
                    fileBrowser.Filter = browserFilter;
                }
                else
                {
                    fileBrowser.Filter = browserFilter + "|All files(*.*)|*.*";
                }
                fileBrowser.FilterIndex = selectedIndexFilter;

                if (fileBrowser.ShowDialog() == DialogResult.OK)
                {
                    filePath = fileBrowser.FileName;
                }
            }

            return filePath;
        }

        internal static void CreateWavFile(int frequency, int bitsPerChannel, int numberOfChannels, byte[] PCMData, string filePath)
        {
            using (FileStream wavFile = new FileStream(filePath, FileMode.Create))
            using (BinaryWriter binaryWriter = new BinaryWriter(wavFile))
            {
                //Write WAV Header
                binaryWriter.Write(Encoding.UTF8.GetBytes("RIFF")); //Chunk ID
                binaryWriter.Write((uint)(36 + PCMData.Length)); //Chunk Size
                binaryWriter.Write(Encoding.UTF8.GetBytes("WAVE")); //Format
                binaryWriter.Write(Encoding.UTF8.GetBytes("fmt ")); //Subchunk1 ID
                binaryWriter.Write((uint)16); //Subchunk1 Size
                binaryWriter.Write((ushort)1); //Audio Format
                binaryWriter.Write((ushort)numberOfChannels); //Num Channels
                binaryWriter.Write((uint)(frequency)); //Sample Rate
                binaryWriter.Write((uint)((frequency * numberOfChannels * bitsPerChannel) / 8)); //Byte Rate
                binaryWriter.Write((ushort)((numberOfChannels * bitsPerChannel) / 8)); //Block Align
                binaryWriter.Write((ushort)(bitsPerChannel)); //Bits Per Sample
                binaryWriter.Write(Encoding.UTF8.GetBytes("data")); //Subchunk2 ID
                binaryWriter.Write((uint)PCMData.Length); //Subchunk2 Size

                //Write PCM Data
                for (int i = 0; i < PCMData.Length; i++)
                {
                    binaryWriter.Write(PCMData[i]);
                }

                //Close Writter
                binaryWriter.Close();
                //Close File
                wavFile.Close();
            }
        }
    }
}
