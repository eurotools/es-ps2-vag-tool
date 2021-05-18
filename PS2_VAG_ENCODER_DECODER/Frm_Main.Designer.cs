
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
            this.Button_Encode = new System.Windows.Forms.Button();
            this.Button_Search_Encode = new System.Windows.Forms.Button();
            this.Textbox_File_To_Encode = new System.Windows.Forms.TextBox();
            this.label1 = new System.Windows.Forms.Label();
            this.groupBox2 = new System.Windows.Forms.GroupBox();
            this.groupBox3 = new System.Windows.Forms.GroupBox();
            this.label7 = new System.Windows.Forms.Label();
            this.txtInterleave = new System.Windows.Forms.TextBox();
            this.label9 = new System.Windows.Forms.Label();
            this.txtChannels = new System.Windows.Forms.TextBox();
            this.label8 = new System.Windows.Forms.Label();
            this.label5 = new System.Windows.Forms.Label();
            this.txtSamples = new System.Windows.Forms.TextBox();
            this.label6 = new System.Windows.Forms.Label();
            this.label4 = new System.Windows.Forms.Label();
            this.txtFrequency = new System.Windows.Forms.TextBox();
            this.label3 = new System.Windows.Forms.Label();
            this.Button_Decode = new System.Windows.Forms.Button();
            this.Button_Search_Decode = new System.Windows.Forms.Button();
            this.Textbox_File_To_Decode = new System.Windows.Forms.TextBox();
            this.label2 = new System.Windows.Forms.Label();
            this.groupBox1.SuspendLayout();
            this.groupBox2.SuspendLayout();
            this.groupBox3.SuspendLayout();
            this.SuspendLayout();
            // 
            // groupBox1
            // 
            this.groupBox1.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.groupBox1.Controls.Add(this.Button_Encode);
            this.groupBox1.Controls.Add(this.Button_Search_Encode);
            this.groupBox1.Controls.Add(this.Textbox_File_To_Encode);
            this.groupBox1.Controls.Add(this.label1);
            this.groupBox1.Location = new System.Drawing.Point(12, 12);
            this.groupBox1.Name = "groupBox1";
            this.groupBox1.Size = new System.Drawing.Size(434, 111);
            this.groupBox1.TabIndex = 0;
            this.groupBox1.TabStop = false;
            this.groupBox1.Text = "Encode:";
            // 
            // Button_Encode
            // 
            this.Button_Encode.Location = new System.Drawing.Point(155, 45);
            this.Button_Encode.Name = "Button_Encode";
            this.Button_Encode.Size = new System.Drawing.Size(121, 55);
            this.Button_Encode.TabIndex = 3;
            this.Button_Encode.Text = "Encode";
            this.Button_Encode.UseVisualStyleBackColor = true;
            this.Button_Encode.Click += new System.EventHandler(this.Button_Encode_Click);
            // 
            // Button_Search_Encode
            // 
            this.Button_Search_Encode.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.Button_Search_Encode.Location = new System.Drawing.Point(353, 17);
            this.Button_Search_Encode.Name = "Button_Search_Encode";
            this.Button_Search_Encode.Size = new System.Drawing.Size(75, 23);
            this.Button_Search_Encode.TabIndex = 2;
            this.Button_Search_Encode.Text = "Search";
            this.Button_Search_Encode.UseVisualStyleBackColor = true;
            this.Button_Search_Encode.Click += new System.EventHandler(this.Button_Search_Encode_Click);
            // 
            // Textbox_File_To_Encode
            // 
            this.Textbox_File_To_Encode.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.Textbox_File_To_Encode.BackColor = System.Drawing.SystemColors.ControlLightLight;
            this.Textbox_File_To_Encode.Location = new System.Drawing.Point(63, 19);
            this.Textbox_File_To_Encode.Name = "Textbox_File_To_Encode";
            this.Textbox_File_To_Encode.ReadOnly = true;
            this.Textbox_File_To_Encode.Size = new System.Drawing.Size(284, 20);
            this.Textbox_File_To_Encode.TabIndex = 1;
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(6, 22);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(51, 13);
            this.label1.TabIndex = 0;
            this.label1.Text = "File Path:";
            // 
            // groupBox2
            // 
            this.groupBox2.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.groupBox2.Controls.Add(this.groupBox3);
            this.groupBox2.Controls.Add(this.Button_Decode);
            this.groupBox2.Controls.Add(this.Button_Search_Decode);
            this.groupBox2.Controls.Add(this.Textbox_File_To_Decode);
            this.groupBox2.Controls.Add(this.label2);
            this.groupBox2.Location = new System.Drawing.Point(12, 129);
            this.groupBox2.Name = "groupBox2";
            this.groupBox2.Size = new System.Drawing.Size(434, 122);
            this.groupBox2.TabIndex = 1;
            this.groupBox2.TabStop = false;
            this.groupBox2.Text = "Decode:";
            // 
            // groupBox3
            // 
            this.groupBox3.Controls.Add(this.label7);
            this.groupBox3.Controls.Add(this.txtInterleave);
            this.groupBox3.Controls.Add(this.label9);
            this.groupBox3.Controls.Add(this.txtChannels);
            this.groupBox3.Controls.Add(this.label8);
            this.groupBox3.Controls.Add(this.label5);
            this.groupBox3.Controls.Add(this.txtSamples);
            this.groupBox3.Controls.Add(this.label6);
            this.groupBox3.Controls.Add(this.label4);
            this.groupBox3.Controls.Add(this.txtFrequency);
            this.groupBox3.Controls.Add(this.label3);
            this.groupBox3.Location = new System.Drawing.Point(9, 45);
            this.groupBox3.Name = "groupBox3";
            this.groupBox3.Size = new System.Drawing.Size(338, 71);
            this.groupBox3.TabIndex = 5;
            this.groupBox3.TabStop = false;
            this.groupBox3.Text = "File Parameters";
            // 
            // label7
            // 
            this.label7.AutoSize = true;
            this.label7.Location = new System.Drawing.Point(273, 43);
            this.label7.Name = "label7";
            this.label7.Size = new System.Drawing.Size(33, 13);
            this.label7.TabIndex = 10;
            this.label7.Text = "Bytes";
            // 
            // txtInterleave
            // 
            this.txtInterleave.Location = new System.Drawing.Point(213, 40);
            this.txtInterleave.Name = "txtInterleave";
            this.txtInterleave.Size = new System.Drawing.Size(54, 20);
            this.txtInterleave.TabIndex = 9;
            // 
            // label9
            // 
            this.label9.AutoSize = true;
            this.label9.Location = new System.Drawing.Point(155, 43);
            this.label9.Name = "label9";
            this.label9.Size = new System.Drawing.Size(57, 13);
            this.label9.TabIndex = 8;
            this.label9.Text = "Interleave:";
            // 
            // txtChannels
            // 
            this.txtChannels.Location = new System.Drawing.Point(213, 18);
            this.txtChannels.Name = "txtChannels";
            this.txtChannels.Size = new System.Drawing.Size(54, 20);
            this.txtChannels.TabIndex = 7;
            // 
            // label8
            // 
            this.label8.AutoSize = true;
            this.label8.Location = new System.Drawing.Point(158, 21);
            this.label8.Name = "label8";
            this.label8.Size = new System.Drawing.Size(54, 13);
            this.label8.TabIndex = 6;
            this.label8.Text = "Channels:";
            // 
            // label5
            // 
            this.label5.AutoSize = true;
            this.label5.Location = new System.Drawing.Point(126, 43);
            this.label5.Name = "label5";
            this.label5.Size = new System.Drawing.Size(23, 13);
            this.label5.TabIndex = 5;
            this.label5.Text = "bits";
            // 
            // txtSamples
            // 
            this.txtSamples.Location = new System.Drawing.Point(69, 40);
            this.txtSamples.Name = "txtSamples";
            this.txtSamples.Size = new System.Drawing.Size(54, 20);
            this.txtSamples.TabIndex = 4;
            // 
            // label6
            // 
            this.label6.AutoSize = true;
            this.label6.Location = new System.Drawing.Point(16, 43);
            this.label6.Name = "label6";
            this.label6.Size = new System.Drawing.Size(50, 13);
            this.label6.TabIndex = 3;
            this.label6.Text = "Samples:";
            // 
            // label4
            // 
            this.label4.AutoSize = true;
            this.label4.Location = new System.Drawing.Point(126, 22);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(20, 13);
            this.label4.TabIndex = 2;
            this.label4.Text = "Hz";
            // 
            // txtFrequency
            // 
            this.txtFrequency.Location = new System.Drawing.Point(69, 19);
            this.txtFrequency.Name = "txtFrequency";
            this.txtFrequency.Size = new System.Drawing.Size(54, 20);
            this.txtFrequency.TabIndex = 1;
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(6, 21);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(60, 13);
            this.label3.TabIndex = 0;
            this.label3.Text = "Frequency:";
            // 
            // Button_Decode
            // 
            this.Button_Decode.Location = new System.Drawing.Point(353, 50);
            this.Button_Decode.Name = "Button_Decode";
            this.Button_Decode.Size = new System.Drawing.Size(75, 66);
            this.Button_Decode.TabIndex = 4;
            this.Button_Decode.Text = "Decode";
            this.Button_Decode.UseVisualStyleBackColor = true;
            this.Button_Decode.Click += new System.EventHandler(this.Button_Decode_Click);
            // 
            // Button_Search_Decode
            // 
            this.Button_Search_Decode.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.Button_Search_Decode.Location = new System.Drawing.Point(353, 17);
            this.Button_Search_Decode.Name = "Button_Search_Decode";
            this.Button_Search_Decode.Size = new System.Drawing.Size(75, 23);
            this.Button_Search_Decode.TabIndex = 2;
            this.Button_Search_Decode.Text = "Search";
            this.Button_Search_Decode.UseVisualStyleBackColor = true;
            this.Button_Search_Decode.Click += new System.EventHandler(this.Button_Search_Decode_Click);
            // 
            // Textbox_File_To_Decode
            // 
            this.Textbox_File_To_Decode.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.Textbox_File_To_Decode.BackColor = System.Drawing.SystemColors.ControlLightLight;
            this.Textbox_File_To_Decode.Location = new System.Drawing.Point(63, 19);
            this.Textbox_File_To_Decode.Name = "Textbox_File_To_Decode";
            this.Textbox_File_To_Decode.ReadOnly = true;
            this.Textbox_File_To_Decode.Size = new System.Drawing.Size(284, 20);
            this.Textbox_File_To_Decode.TabIndex = 1;
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(6, 22);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(51, 13);
            this.label2.TabIndex = 0;
            this.label2.Text = "File Path:";
            // 
            // Frm_Main
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(458, 263);
            this.Controls.Add(this.groupBox2);
            this.Controls.Add(this.groupBox1);
            this.Name = "Frm_Main";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "PS2 VAG Encoder Decoder";
            this.groupBox1.ResumeLayout(false);
            this.groupBox1.PerformLayout();
            this.groupBox2.ResumeLayout(false);
            this.groupBox2.PerformLayout();
            this.groupBox3.ResumeLayout(false);
            this.groupBox3.PerformLayout();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.GroupBox groupBox1;
        private System.Windows.Forms.Button Button_Search_Encode;
        private System.Windows.Forms.TextBox Textbox_File_To_Encode;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.GroupBox groupBox2;
        private System.Windows.Forms.Button Button_Search_Decode;
        private System.Windows.Forms.TextBox Textbox_File_To_Decode;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.Button Button_Encode;
        private System.Windows.Forms.Button Button_Decode;
        private System.Windows.Forms.GroupBox groupBox3;
        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.TextBox txtFrequency;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.TextBox txtChannels;
        private System.Windows.Forms.Label label8;
        private System.Windows.Forms.Label label5;
        private System.Windows.Forms.TextBox txtSamples;
        private System.Windows.Forms.Label label6;
        private System.Windows.Forms.Label label7;
        private System.Windows.Forms.TextBox txtInterleave;
        private System.Windows.Forms.Label label9;
    }
}

