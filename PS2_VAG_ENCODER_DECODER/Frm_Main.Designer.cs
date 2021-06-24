
namespace PS2_VAG_ENCODER_DECODER
{
    partial class Frm_Main
    {
        /// <summary>
        /// Variable del diseñador necesaria.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Limpiar los recursos que se estén usando.
        /// </summary>
        /// <param name="disposing">true si los recursos administrados se deben desechar; false en caso contrario.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Código generado por el Diseñador de Windows Forms

        /// <summary>
        /// Método necesario para admitir el Diseñador. No se puede modificar
        /// el contenido de este método con el editor de código.
        /// </summary>
        private void InitializeComponent()
        {
            this.groupBox1 = new System.Windows.Forms.GroupBox();
            this.GroupBox_ParamsEncoder = new System.Windows.Forms.GroupBox();
            this.CheckBox_LoopOffset = new System.Windows.Forms.CheckBox();
            this.Numeric_LoopOffset = new System.Windows.Forms.NumericUpDown();
            this.Label_LoopOffset = new System.Windows.Forms.Label();
            this.Button_Encode = new System.Windows.Forms.Button();
            this.Button_Search_Encode = new System.Windows.Forms.Button();
            this.Textbox_FileToEncode = new System.Windows.Forms.TextBox();
            this.Label_FileToEncode = new System.Windows.Forms.Label();
            this.groupBox2 = new System.Windows.Forms.GroupBox();
            this.groupBox3 = new System.Windows.Forms.GroupBox();
            this.NumericChannels = new System.Windows.Forms.NumericUpDown();
            this.NumericSamples = new System.Windows.Forms.NumericUpDown();
            this.NumericFrequency = new System.Windows.Forms.NumericUpDown();
            this.Label_Channels = new System.Windows.Forms.Label();
            this.Label_SamplesBits = new System.Windows.Forms.Label();
            this.Label_Samples = new System.Windows.Forms.Label();
            this.Label_FreqHZ = new System.Windows.Forms.Label();
            this.Label_Frequency = new System.Windows.Forms.Label();
            this.Button_Decode = new System.Windows.Forms.Button();
            this.Button_Search_Decode = new System.Windows.Forms.Button();
            this.Textbox_FileToDecode = new System.Windows.Forms.TextBox();
            this.Label_FileToDecode = new System.Windows.Forms.Label();
            this.groupBox1.SuspendLayout();
            this.GroupBox_ParamsEncoder.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.Numeric_LoopOffset)).BeginInit();
            this.groupBox2.SuspendLayout();
            this.groupBox3.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.NumericChannels)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.NumericSamples)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.NumericFrequency)).BeginInit();
            this.SuspendLayout();
            // 
            // groupBox1
            // 
            this.groupBox1.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.groupBox1.Controls.Add(this.GroupBox_ParamsEncoder);
            this.groupBox1.Controls.Add(this.Button_Encode);
            this.groupBox1.Controls.Add(this.Button_Search_Encode);
            this.groupBox1.Controls.Add(this.Textbox_FileToEncode);
            this.groupBox1.Controls.Add(this.Label_FileToEncode);
            this.groupBox1.Location = new System.Drawing.Point(12, 12);
            this.groupBox1.Name = "groupBox1";
            this.groupBox1.Size = new System.Drawing.Size(475, 127);
            this.groupBox1.TabIndex = 0;
            this.groupBox1.TabStop = false;
            this.groupBox1.Text = "Encode:";
            // 
            // GroupBox_ParamsEncoder
            // 
            this.GroupBox_ParamsEncoder.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.GroupBox_ParamsEncoder.Controls.Add(this.CheckBox_LoopOffset);
            this.GroupBox_ParamsEncoder.Controls.Add(this.Numeric_LoopOffset);
            this.GroupBox_ParamsEncoder.Controls.Add(this.Label_LoopOffset);
            this.GroupBox_ParamsEncoder.Location = new System.Drawing.Point(9, 45);
            this.GroupBox_ParamsEncoder.Name = "GroupBox_ParamsEncoder";
            this.GroupBox_ParamsEncoder.Size = new System.Drawing.Size(379, 76);
            this.GroupBox_ParamsEncoder.TabIndex = 3;
            this.GroupBox_ParamsEncoder.TabStop = false;
            this.GroupBox_ParamsEncoder.Text = "File Parameters";
            // 
            // CheckBox_LoopOffset
            // 
            this.CheckBox_LoopOffset.AutoSize = true;
            this.CheckBox_LoopOffset.Location = new System.Drawing.Point(9, 45);
            this.CheckBox_LoopOffset.Name = "CheckBox_LoopOffset";
            this.CheckBox_LoopOffset.Size = new System.Drawing.Size(106, 17);
            this.CheckBox_LoopOffset.TabIndex = 2;
            this.CheckBox_LoopOffset.Text = "Activate Looping";
            this.CheckBox_LoopOffset.UseVisualStyleBackColor = true;
            // 
            // Numeric_LoopOffset
            // 
            this.Numeric_LoopOffset.Location = new System.Drawing.Point(77, 19);
            this.Numeric_LoopOffset.Maximum = new decimal(new int[] {
            2147483647,
            0,
            0,
            0});
            this.Numeric_LoopOffset.Name = "Numeric_LoopOffset";
            this.Numeric_LoopOffset.Size = new System.Drawing.Size(120, 20);
            this.Numeric_LoopOffset.TabIndex = 1;
            // 
            // Label_LoopOffset
            // 
            this.Label_LoopOffset.AutoSize = true;
            this.Label_LoopOffset.Location = new System.Drawing.Point(6, 21);
            this.Label_LoopOffset.Name = "Label_LoopOffset";
            this.Label_LoopOffset.Size = new System.Drawing.Size(65, 13);
            this.Label_LoopOffset.TabIndex = 0;
            this.Label_LoopOffset.Text = "Loop Offset:";
            // 
            // Button_Encode
            // 
            this.Button_Encode.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.Button_Encode.Location = new System.Drawing.Point(394, 46);
            this.Button_Encode.Name = "Button_Encode";
            this.Button_Encode.Size = new System.Drawing.Size(75, 75);
            this.Button_Encode.TabIndex = 4;
            this.Button_Encode.Text = "Encode";
            this.Button_Encode.UseVisualStyleBackColor = true;
            this.Button_Encode.Click += new System.EventHandler(this.Button_Encode_Click);
            // 
            // Button_Search_Encode
            // 
            this.Button_Search_Encode.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.Button_Search_Encode.Location = new System.Drawing.Point(394, 17);
            this.Button_Search_Encode.Name = "Button_Search_Encode";
            this.Button_Search_Encode.Size = new System.Drawing.Size(75, 23);
            this.Button_Search_Encode.TabIndex = 2;
            this.Button_Search_Encode.Text = "Search";
            this.Button_Search_Encode.UseVisualStyleBackColor = true;
            this.Button_Search_Encode.Click += new System.EventHandler(this.Button_Search_Encode_Click);
            // 
            // Textbox_FileToEncode
            // 
            this.Textbox_FileToEncode.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.Textbox_FileToEncode.BackColor = System.Drawing.SystemColors.ControlLightLight;
            this.Textbox_FileToEncode.Location = new System.Drawing.Point(63, 19);
            this.Textbox_FileToEncode.Name = "Textbox_FileToEncode";
            this.Textbox_FileToEncode.ReadOnly = true;
            this.Textbox_FileToEncode.Size = new System.Drawing.Size(325, 20);
            this.Textbox_FileToEncode.TabIndex = 1;
            // 
            // Label_FileToEncode
            // 
            this.Label_FileToEncode.AutoSize = true;
            this.Label_FileToEncode.Location = new System.Drawing.Point(6, 22);
            this.Label_FileToEncode.Name = "Label_FileToEncode";
            this.Label_FileToEncode.Size = new System.Drawing.Size(51, 13);
            this.Label_FileToEncode.TabIndex = 0;
            this.Label_FileToEncode.Text = "File Path:";
            // 
            // groupBox2
            // 
            this.groupBox2.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.groupBox2.Controls.Add(this.groupBox3);
            this.groupBox2.Controls.Add(this.Button_Decode);
            this.groupBox2.Controls.Add(this.Button_Search_Decode);
            this.groupBox2.Controls.Add(this.Textbox_FileToDecode);
            this.groupBox2.Controls.Add(this.Label_FileToDecode);
            this.groupBox2.Location = new System.Drawing.Point(12, 145);
            this.groupBox2.Name = "groupBox2";
            this.groupBox2.Size = new System.Drawing.Size(475, 123);
            this.groupBox2.TabIndex = 1;
            this.groupBox2.TabStop = false;
            this.groupBox2.Text = "Decode:";
            // 
            // groupBox3
            // 
            this.groupBox3.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.groupBox3.Controls.Add(this.NumericChannels);
            this.groupBox3.Controls.Add(this.NumericSamples);
            this.groupBox3.Controls.Add(this.NumericFrequency);
            this.groupBox3.Controls.Add(this.Label_Channels);
            this.groupBox3.Controls.Add(this.Label_SamplesBits);
            this.groupBox3.Controls.Add(this.Label_Samples);
            this.groupBox3.Controls.Add(this.Label_FreqHZ);
            this.groupBox3.Controls.Add(this.Label_Frequency);
            this.groupBox3.Location = new System.Drawing.Point(9, 45);
            this.groupBox3.Name = "groupBox3";
            this.groupBox3.Size = new System.Drawing.Size(379, 72);
            this.groupBox3.TabIndex = 3;
            this.groupBox3.TabStop = false;
            this.groupBox3.Text = "File Parameters";
            // 
            // NumericChannels
            // 
            this.NumericChannels.Location = new System.Drawing.Point(246, 19);
            this.NumericChannels.Maximum = new decimal(new int[] {
            2,
            0,
            0,
            0});
            this.NumericChannels.Minimum = new decimal(new int[] {
            1,
            0,
            0,
            0});
            this.NumericChannels.Name = "NumericChannels";
            this.NumericChannels.Size = new System.Drawing.Size(78, 20);
            this.NumericChannels.TabIndex = 7;
            this.NumericChannels.Value = new decimal(new int[] {
            1,
            0,
            0,
            0});
            // 
            // NumericSamples
            // 
            this.NumericSamples.Location = new System.Drawing.Point(72, 45);
            this.NumericSamples.Maximum = new decimal(new int[] {
            2147483647,
            0,
            0,
            0});
            this.NumericSamples.Name = "NumericSamples";
            this.NumericSamples.Size = new System.Drawing.Size(76, 20);
            this.NumericSamples.TabIndex = 4;
            this.NumericSamples.Value = new decimal(new int[] {
            16,
            0,
            0,
            0});
            // 
            // NumericFrequency
            // 
            this.NumericFrequency.Location = new System.Drawing.Point(72, 19);
            this.NumericFrequency.Maximum = new decimal(new int[] {
            2147483647,
            0,
            0,
            0});
            this.NumericFrequency.Name = "NumericFrequency";
            this.NumericFrequency.Size = new System.Drawing.Size(76, 20);
            this.NumericFrequency.TabIndex = 1;
            this.NumericFrequency.Value = new decimal(new int[] {
            44100,
            0,
            0,
            0});
            // 
            // Label_Channels
            // 
            this.Label_Channels.AutoSize = true;
            this.Label_Channels.Location = new System.Drawing.Point(186, 21);
            this.Label_Channels.Name = "Label_Channels";
            this.Label_Channels.Size = new System.Drawing.Size(54, 13);
            this.Label_Channels.TabIndex = 6;
            this.Label_Channels.Text = "Channels:";
            // 
            // Label_SamplesBits
            // 
            this.Label_SamplesBits.AutoSize = true;
            this.Label_SamplesBits.Location = new System.Drawing.Point(154, 47);
            this.Label_SamplesBits.Name = "Label_SamplesBits";
            this.Label_SamplesBits.Size = new System.Drawing.Size(23, 13);
            this.Label_SamplesBits.TabIndex = 5;
            this.Label_SamplesBits.Text = "bits";
            // 
            // Label_Samples
            // 
            this.Label_Samples.AutoSize = true;
            this.Label_Samples.Location = new System.Drawing.Point(16, 47);
            this.Label_Samples.Name = "Label_Samples";
            this.Label_Samples.Size = new System.Drawing.Size(50, 13);
            this.Label_Samples.TabIndex = 3;
            this.Label_Samples.Text = "Samples:";
            // 
            // Label_FreqHZ
            // 
            this.Label_FreqHZ.AutoSize = true;
            this.Label_FreqHZ.Location = new System.Drawing.Point(154, 21);
            this.Label_FreqHZ.Name = "Label_FreqHZ";
            this.Label_FreqHZ.Size = new System.Drawing.Size(20, 13);
            this.Label_FreqHZ.TabIndex = 2;
            this.Label_FreqHZ.Text = "Hz";
            // 
            // Label_Frequency
            // 
            this.Label_Frequency.AutoSize = true;
            this.Label_Frequency.Location = new System.Drawing.Point(6, 21);
            this.Label_Frequency.Name = "Label_Frequency";
            this.Label_Frequency.Size = new System.Drawing.Size(60, 13);
            this.Label_Frequency.TabIndex = 0;
            this.Label_Frequency.Text = "Frequency:";
            // 
            // Button_Decode
            // 
            this.Button_Decode.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.Button_Decode.Location = new System.Drawing.Point(394, 46);
            this.Button_Decode.Name = "Button_Decode";
            this.Button_Decode.Size = new System.Drawing.Size(75, 71);
            this.Button_Decode.TabIndex = 4;
            this.Button_Decode.Text = "Decode";
            this.Button_Decode.UseVisualStyleBackColor = true;
            this.Button_Decode.Click += new System.EventHandler(this.Button_Decode_Click);
            // 
            // Button_Search_Decode
            // 
            this.Button_Search_Decode.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.Button_Search_Decode.Location = new System.Drawing.Point(394, 17);
            this.Button_Search_Decode.Name = "Button_Search_Decode";
            this.Button_Search_Decode.Size = new System.Drawing.Size(75, 23);
            this.Button_Search_Decode.TabIndex = 2;
            this.Button_Search_Decode.Text = "Search";
            this.Button_Search_Decode.UseVisualStyleBackColor = true;
            this.Button_Search_Decode.Click += new System.EventHandler(this.Button_Search_Decode_Click);
            // 
            // Textbox_FileToDecode
            // 
            this.Textbox_FileToDecode.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.Textbox_FileToDecode.BackColor = System.Drawing.SystemColors.ControlLightLight;
            this.Textbox_FileToDecode.Location = new System.Drawing.Point(63, 19);
            this.Textbox_FileToDecode.Name = "Textbox_FileToDecode";
            this.Textbox_FileToDecode.ReadOnly = true;
            this.Textbox_FileToDecode.Size = new System.Drawing.Size(325, 20);
            this.Textbox_FileToDecode.TabIndex = 1;
            // 
            // Label_FileToDecode
            // 
            this.Label_FileToDecode.AutoSize = true;
            this.Label_FileToDecode.Location = new System.Drawing.Point(6, 22);
            this.Label_FileToDecode.Name = "Label_FileToDecode";
            this.Label_FileToDecode.Size = new System.Drawing.Size(51, 13);
            this.Label_FileToDecode.TabIndex = 0;
            this.Label_FileToDecode.Text = "File Path:";
            // 
            // Frm_Main
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(499, 280);
            this.Controls.Add(this.groupBox2);
            this.Controls.Add(this.groupBox1);
            this.Name = "Frm_Main";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "PS2 VAG Encoder Decoder";
            this.groupBox1.ResumeLayout(false);
            this.groupBox1.PerformLayout();
            this.GroupBox_ParamsEncoder.ResumeLayout(false);
            this.GroupBox_ParamsEncoder.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.Numeric_LoopOffset)).EndInit();
            this.groupBox2.ResumeLayout(false);
            this.groupBox2.PerformLayout();
            this.groupBox3.ResumeLayout(false);
            this.groupBox3.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.NumericChannels)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.NumericSamples)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.NumericFrequency)).EndInit();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.GroupBox groupBox1;
        private System.Windows.Forms.Button Button_Search_Encode;
        private System.Windows.Forms.TextBox Textbox_FileToEncode;
        private System.Windows.Forms.Label Label_FileToEncode;
        private System.Windows.Forms.GroupBox groupBox2;
        private System.Windows.Forms.Button Button_Search_Decode;
        private System.Windows.Forms.TextBox Textbox_FileToDecode;
        private System.Windows.Forms.Label Label_FileToDecode;
        private System.Windows.Forms.Button Button_Encode;
        private System.Windows.Forms.Button Button_Decode;
        private System.Windows.Forms.GroupBox groupBox3;
        private System.Windows.Forms.Label Label_FreqHZ;
        private System.Windows.Forms.Label Label_Frequency;
        private System.Windows.Forms.Label Label_Channels;
        private System.Windows.Forms.Label Label_SamplesBits;
        private System.Windows.Forms.Label Label_Samples;
        private System.Windows.Forms.GroupBox GroupBox_ParamsEncoder;
        private System.Windows.Forms.CheckBox CheckBox_LoopOffset;
        private System.Windows.Forms.NumericUpDown Numeric_LoopOffset;
        private System.Windows.Forms.Label Label_LoopOffset;
        private System.Windows.Forms.NumericUpDown NumericChannels;
        private System.Windows.Forms.NumericUpDown NumericSamples;
        private System.Windows.Forms.NumericUpDown NumericFrequency;
    }
}

