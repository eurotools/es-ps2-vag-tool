using NAudio.Wave;
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

        //*===============================================================================================
        //* DECODE
        //*===============================================================================================
        private void Button_Search_Decode_Click(object sender, EventArgs e)
        {
            //Open files explorer
            OpenFileDialog.Filter = "PlayStation Compressed Sound File (*.vag)|*.vag";
            DialogResult openImaFile = OpenFileDialog.ShowDialog();
            if (openImaFile == DialogResult.OK)
            {
                //Get the selected file 
                Textbox_FileToDecode.Text = OpenFileDialog.FileName;
            }
        }

        private void Button_Decode_Click(object sender, EventArgs e)
        {
            if (File.Exists(Textbox_FileToDecode.Text))
            {
                byte[] imaData = File.ReadAllBytes(Textbox_FileToDecode.Text);

                //Save File
                SaveFileDialog.Filter = "Wave Audio File (*.wav)|*.wav";
                SaveFileDialog.FileName = string.Join("", Path.GetFileNameWithoutExtension(Textbox_FileToDecode.Text), "_Decoded");
                DialogResult saveFileDialog = SaveFileDialog.ShowDialog();
                if (saveFileDialog == DialogResult.OK)
                {
                    if (NumericChannels.Value == 2)
                    {
                        byte[][] splittedData = VAGHandler.SplitChannels(imaData, 2);
                        byte[] decodedDataLeftChannel = VAGHandler.VAGDecoder(splittedData[0]);
                        byte[] decodedDataRightChannel = VAGHandler.VAGDecoder(splittedData[1]);
                        WavFunctions.CreateStereoWavFile(SaveFileDialog.FileName,decodedDataLeftChannel, decodedDataRightChannel, (int)NumericFrequency.Value);
                    }
                    else
                    {
                        byte[] decodedData = VAGHandler.VAGDecoder(imaData);
                        WavFunctions.CreateMonoWavFile(SaveFileDialog.FileName, decodedData, (int)NumericFrequency.Value, 16);
                    }
                }
            }
        }

        //*===============================================================================================
        //* ENCODE
        //*===============================================================================================
        private void Button_Search_Encode_Click(object sender, EventArgs e)
        {
            //Open files explorer
            OpenFileDialog.Filter = "Wave Audio File (*.wav)|*.wav";
            DialogResult openWaveFile = OpenFileDialog.ShowDialog();
            if (openWaveFile == DialogResult.OK)
            {
                //Get the selected file 
                Textbox_FileToEncode.Text = OpenFileDialog.FileName;
            }
        }

        private void Button_Encode_Click(object sender, EventArgs e)
        {
            //Ensure that the file still exists
            if (File.Exists(Textbox_FileToEncode.Text))
            {
                using (WaveFileReader fileReader = new WaveFileReader(Textbox_FileToEncode.Text))
                {
                    if (fileReader.WaveFormat.BitsPerSample == 16 && fileReader.WaveFormat.Encoding == WaveFormatEncoding.Pcm)
                    {
                        //Get PCM Data (in bytes)
                        byte[] pcmData = new byte[fileReader.Length];
                        fileReader.Read(pcmData, 0, (int)fileReader.Length);

                        //Parse PCM Data to a short array
                        short[] samplesShort = new short[pcmData.Length / 2];
                        WaveBuffer sourceWaveBuffer = new WaveBuffer(pcmData);
                        for (int i = 0; i < samplesShort.Length; i++)
                        {
                            samplesShort[i] = sourceWaveBuffer.ShortBuffer[i];
                        }

                        //Save File
                        SaveFileDialog.Filter = "PlayStation Compressed Sound File (*.vag)|*.vag";
                        SaveFileDialog.FileName = string.Join("", Path.GetFileNameWithoutExtension(Textbox_FileToEncode.Text), "_Encoded");
                        DialogResult saveFileDialog = SaveFileDialog.ShowDialog();
                        if (saveFileDialog == DialogResult.OK)
                        {
                            //Encode stereo
                            byte[] encodedData;
                            if (fileReader.WaveFormat.Channels == 2)
                            {
                                short[][] splittedData = WavFunctions.SplitChannels(samplesShort, 2);
                                byte[] encodedDataLeftChannel = VAGHandler.VAGEncoder(splittedData[0], fileReader.WaveFormat.SampleRate, (uint)Numeric_LoopOffset.Value, CheckBox_LoopOffset.Checked);
                                byte[] encodedDataRightChannel = VAGHandler.VAGEncoder(splittedData[1], fileReader.WaveFormat.SampleRate, (uint)Numeric_LoopOffset.Value, CheckBox_LoopOffset.Checked);

                                encodedData = VAGHandler.CombineChannelsVAG(encodedDataLeftChannel, encodedDataRightChannel);
                            }
                            //Encode mono
                            else
                            {
                                encodedData = VAGHandler.VAGEncoder(samplesShort, fileReader.WaveFormat.SampleRate, (uint)Numeric_LoopOffset.Value, CheckBox_LoopOffset.Checked);
                            }
                            File.WriteAllBytes(SaveFileDialog.FileName, encodedData);
                        }
                    }
                    else
                    {
                        MessageBox.Show("Format not supported", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                }
            }
        }
    }
}
