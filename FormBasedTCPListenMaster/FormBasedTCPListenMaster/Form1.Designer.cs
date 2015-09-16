namespace FormBasedTCPListenMaster
{
    partial class Form1
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.btnStartMaster = new System.Windows.Forms.Button();
            this.textBox1 = new System.Windows.Forms.TextBox();
            this.btnWriteData = new System.Windows.Forms.Button();
            this.btnReadData = new System.Windows.Forms.Button();
            this.txtBoxWriteIPAddr = new System.Windows.Forms.TextBox();
            this.label1 = new System.Windows.Forms.Label();
            this.txtBoxReadIPAddr = new System.Windows.Forms.TextBox();
            this.label2 = new System.Windows.Forms.Label();
            this.SuspendLayout();
            // 
            // btnStartMaster
            // 
            this.btnStartMaster.Location = new System.Drawing.Point(128, 32);
            this.btnStartMaster.Name = "btnStartMaster";
            this.btnStartMaster.Size = new System.Drawing.Size(153, 45);
            this.btnStartMaster.TabIndex = 0;
            this.btnStartMaster.Text = "Start Master";
            this.btnStartMaster.UseVisualStyleBackColor = true;
            this.btnStartMaster.Click += new System.EventHandler(this.btnStartMaster_Click);
            // 
            // textBox1
            // 
            this.textBox1.Location = new System.Drawing.Point(12, 365);
            this.textBox1.Multiline = true;
            this.textBox1.Name = "textBox1";
            this.textBox1.Size = new System.Drawing.Size(431, 128);
            this.textBox1.TabIndex = 1;
            // 
            // btnWriteData
            // 
            this.btnWriteData.Location = new System.Drawing.Point(12, 114);
            this.btnWriteData.Name = "btnWriteData";
            this.btnWriteData.Size = new System.Drawing.Size(153, 45);
            this.btnWriteData.TabIndex = 2;
            this.btnWriteData.Text = "Write Data";
            this.btnWriteData.UseVisualStyleBackColor = true;
            this.btnWriteData.Click += new System.EventHandler(this.btnWriteData_Click);
            // 
            // btnReadData
            // 
            this.btnReadData.Location = new System.Drawing.Point(290, 114);
            this.btnReadData.Name = "btnReadData";
            this.btnReadData.Size = new System.Drawing.Size(153, 45);
            this.btnReadData.TabIndex = 3;
            this.btnReadData.Text = "Read Data";
            this.btnReadData.UseVisualStyleBackColor = true;
            // 
            // txtBoxWriteIPAddr
            // 
            this.txtBoxWriteIPAddr.Location = new System.Drawing.Point(12, 175);
            this.txtBoxWriteIPAddr.Multiline = true;
            this.txtBoxWriteIPAddr.Name = "txtBoxWriteIPAddr";
            this.txtBoxWriteIPAddr.Size = new System.Drawing.Size(143, 29);
            this.txtBoxWriteIPAddr.TabIndex = 4;
            this.txtBoxWriteIPAddr.TextChanged += new System.EventHandler(this.textBox2_TextChanged);
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(12, 207);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(145, 17);
            this.label1.TabIndex = 5;
            this.label1.Text = "Outstation IP Address";
            // 
            // txtBoxReadIPAddr
            // 
            this.txtBoxReadIPAddr.Location = new System.Drawing.Point(290, 175);
            this.txtBoxReadIPAddr.Multiline = true;
            this.txtBoxReadIPAddr.Name = "txtBoxReadIPAddr";
            this.txtBoxReadIPAddr.Size = new System.Drawing.Size(143, 29);
            this.txtBoxReadIPAddr.TabIndex = 6;
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(288, 207);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(145, 17);
            this.label2.TabIndex = 7;
            this.label2.Text = "Outstation IP Address";
            // 
            // Form1
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(8F, 16F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(455, 505);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.txtBoxReadIPAddr);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.txtBoxWriteIPAddr);
            this.Controls.Add(this.btnReadData);
            this.Controls.Add(this.btnWriteData);
            this.Controls.Add(this.textBox1);
            this.Controls.Add(this.btnStartMaster);
            this.Name = "Form1";
            this.Text = "DNP Master";
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Button btnStartMaster;
        private System.Windows.Forms.TextBox textBox1;
        private System.Windows.Forms.Button btnWriteData;
        private System.Windows.Forms.Button btnReadData;
        private System.Windows.Forms.TextBox txtBoxWriteIPAddr;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.TextBox txtBoxReadIPAddr;
        private System.Windows.Forms.Label label2;
    }
}

