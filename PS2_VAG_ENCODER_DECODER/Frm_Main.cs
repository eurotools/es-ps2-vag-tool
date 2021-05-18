using System;
using System.IO;
using System.Windows.Forms;

namespace PS2_VAG_ENCODER_DECODER
{
    public partial class Frm_Main : Form
    {
        private string _fileToDecode = "";
        private string _fileToEncode = "";

        // These defaults relate to the Sphinx audio files
        private int _frequency = 22050;
        private int _samples = 16;
        private int _channels = 2;
        private int _interleave = 0;

        public Frm_Main()
        {
            InitializeComponent();

            txtFrequency.Text = _frequency.ToString();
            txtSamples.Text = _samples.ToString();
            txtChannels.Text = _channels.ToString();
            txtInterleave.Text = _interleave.ToString();

            Button_Search_Encode.Enabled = false; // TODO: Remove once functionality is completed
            Button_Encode.Enabled = false; // TODO: Remove once functionality is completed
        }

        private void Button_Search_Encode_Click(object sender, EventArgs e)
        {
            string selectedFile = FileBrowserDialog("VAG Files (*.VAG)|*.vag", 0, true);
            if (File.Exists(selectedFile))
            {
                Textbox_File_To_Encode.Text = _fileToEncode = selectedFile;
            }
        }

        private void Button_Search_Decode_Click(object sender, EventArgs e)
        {
            string selectedFile = FileBrowserDialog("VAG Files (*.VAG)|*.VAG", 0, false);
            if (File.Exists(selectedFile))
            {
                Textbox_File_To_Decode.Text = _fileToDecode = selectedFile;
            }
        }

        private void Button_Decode_Click(object sender, EventArgs e)
        {
            _frequency = int.Parse(txtFrequency.Text);
            _samples = int.Parse(txtSamples.Text);
            _channels = int.Parse(txtChannels.Text);
            _interleave = int.Parse(txtInterleave.Text);

            // TODO: Allow the user to select the output path
            string fileName = $"{Path.GetFileNameWithoutExtension(_fileToDecode)}.wav";
            string filePath = $"{Application.StartupPath}\\{fileName}";

            byte[] fileData = File.ReadAllBytes(_fileToDecode);
            byte[] pcmData = VAGHandler.VAGDecoder(fileData);

            WavHandler.CreateWavFile(_frequency, _samples, _channels, _interleave, pcmData, filePath);
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
    }
}
