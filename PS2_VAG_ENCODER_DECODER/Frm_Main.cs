using System;
using System.IO;
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
            string selectedFile = FileBrowserDialog("WAV Files (*.wav)|*.wav", 0, true);
            if (File.Exists(selectedFile))
            {
                Textbox_FileToEncode.Text = selectedFile;
            }
        }

        private void Button_Search_Decode_Click(object sender, EventArgs e)
        {
            string selectedFile = FileBrowserDialog("VAG Files (*.VAG)|*.VAG", 0, false);
            if (File.Exists(selectedFile))
            {
                Textbox_FileToDecode.Text = selectedFile;
            }
        }

        private void Button_Decode_Click(object sender, EventArgs e)
        {
            if (!string.IsNullOrEmpty(Textbox_FileToDecode.Text))
            {
                int _frequency = (int)NumericFrequency.Value;
                int _samples = (int)NumericSamples.Value;
                int _channels = (int)NumericChannels.Value;

                // TODO: Allow the user to select the output path
                string fileName = $"{Path.GetFileNameWithoutExtension(Textbox_FileToDecode.Text)}";
                string filePath = $"{Path.GetDirectoryName(Textbox_FileToDecode.Text)}\\{fileName}_Decoded.Wav";

                if (_channels == 2)
                {
                    byte[] LeftChannelData = VAGHandler.VAGDecoder(VAGHandler.SplitVAGChannels(Textbox_FileToDecode.Text, true));
                    byte[] RightChannelData = VAGHandler.VAGDecoder(VAGHandler.SplitVAGChannels(Textbox_FileToDecode.Text, false));
                    WavHandler.CreateWavFileStereo(_frequency, _samples, LeftChannelData, RightChannelData, filePath);
                }
                else
                {
                    byte[] pcmData = VAGHandler.VAGDecoder(File.ReadAllBytes(Textbox_FileToDecode.Text));
                    WavHandler.CreateWavFile(_frequency, _samples, _channels, pcmData, filePath);
                }
                MessageBox.Show($"Exported WAV file to: {filePath}", "PS2 VAG TOOL", MessageBoxButtons.OK, MessageBoxIcon.Information); ;
            }
        }

        private void Button_Encode_Click(object sender, EventArgs e)
        {
            if (!string.IsNullOrEmpty(Textbox_FileToEncode.Text))
            {
                // TODO: Allow the user to select the output path
                string fileName = $"{Path.GetFileNameWithoutExtension(Textbox_FileToEncode.Text)}";
                string filePath = $"{Path.GetDirectoryName(Textbox_FileToEncode.Text)}\\{fileName}_Encoded.VAG";

                byte[] vagData;
                short[] PCMData = WavHandler.ConvertPCMDataToShortArray(WavHandler.GetPCMDataFromWav(Textbox_FileToEncode.Text));

                if (CheckBox_InputIsStereo.Checked)
                {
                    short[] leftChannelData = WavHandler.SplitWavChannels(PCMData, true);
                    short[] rightChannelData = WavHandler.SplitWavChannels(PCMData, false);

                    byte[] encodedDataLeftChannel = VAGHandler.VAGEncoder(leftChannelData, 16, Convert.ToUInt32(Numeric_LoopOffset.Value), CheckBox_LoopOffset.Checked);
                    byte[] encodedDataRightChannel = VAGHandler.VAGEncoder(rightChannelData, 16, Convert.ToUInt32(Numeric_LoopOffset.Value), CheckBox_LoopOffset.Checked);

                    try
                    {
                        vagData = VAGHandler.CombineChannelsVAG(encodedDataLeftChannel, encodedDataRightChannel, 128);
                        File.WriteAllBytes(filePath, vagData);
                        MessageBox.Show($"Exported VAG file to: {filePath}", "PS2 VAG TOOL", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    }
                    catch
                    {
                        MessageBox.Show("An error ocurred while interleaving data, is not possible to interleave the data with the specified value, use a lower value.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                }
                else
                {
                    //Encode WAV to VAG
                    vagData = VAGHandler.VAGEncoder(PCMData, 16, Convert.ToUInt32(Numeric_LoopOffset.Value), CheckBox_LoopOffset.Checked);
                    File.WriteAllBytes(filePath, vagData);
                    MessageBox.Show($"Exported VAG file to: {filePath}", "PS2 VAG TOOL", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
            }
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
