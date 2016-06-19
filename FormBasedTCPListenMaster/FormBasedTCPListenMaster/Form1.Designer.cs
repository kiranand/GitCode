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
            this.textBoxAddresses = new System.Windows.Forms.TextBox();
            this.SuspendLayout();
            // 
            // btnStartMaster
            // 
            this.btnStartMaster.Location = new System.Drawing.Point(103, -1);
            this.btnStartMaster.Margin = new System.Windows.Forms.Padding(2);
            this.btnStartMaster.Name = "btnStartMaster";
            this.btnStartMaster.Size = new System.Drawing.Size(115, 37);
            this.btnStartMaster.TabIndex = 0;
            this.btnStartMaster.Text = "Start Master";
            this.btnStartMaster.UseVisualStyleBackColor = true;
            this.btnStartMaster.Click += new System.EventHandler(this.btnStartMaster_Click);
            // 
            // textBox1
            // 
            this.textBox1.Location = new System.Drawing.Point(9, 209);
            this.textBox1.Margin = new System.Windows.Forms.Padding(2);
            this.textBox1.Multiline = true;
            this.textBox1.Name = "textBox1";
            this.textBox1.ScrollBars = System.Windows.Forms.ScrollBars.Both;
            this.textBox1.Size = new System.Drawing.Size(324, 193);
            this.textBox1.TabIndex = 1;
            this.textBox1.TextChanged += new System.EventHandler(this.textBox1_TextChanged);
            // 
            // btnWriteData
            // 
            this.btnWriteData.Location = new System.Drawing.Point(2, 116);
            this.btnWriteData.Margin = new System.Windows.Forms.Padding(2);
            this.btnWriteData.Name = "btnWriteData";
            this.btnWriteData.Size = new System.Drawing.Size(115, 37);
            this.btnWriteData.TabIndex = 2;
            this.btnWriteData.Text = "Write Data";
            this.btnWriteData.UseVisualStyleBackColor = true;
            this.btnWriteData.Click += new System.EventHandler(this.btnWriteData_Click);
            // 
            // btnReadData
            // 
            this.btnReadData.Location = new System.Drawing.Point(215, 116);
            this.btnReadData.Margin = new System.Windows.Forms.Padding(2);
            this.btnReadData.Name = "btnReadData";
            this.btnReadData.Size = new System.Drawing.Size(115, 37);
            this.btnReadData.TabIndex = 3;
            this.btnReadData.Text = "Read Data";
            this.btnReadData.UseVisualStyleBackColor = true;
            this.btnReadData.Click += new System.EventHandler(this.btnReadData_Click);
            // 
            // txtBoxWriteIPAddr
            // 
            this.txtBoxWriteIPAddr.Location = new System.Drawing.Point(9, 157);
            this.txtBoxWriteIPAddr.Margin = new System.Windows.Forms.Padding(2);
            this.txtBoxWriteIPAddr.Multiline = true;
            this.txtBoxWriteIPAddr.Name = "txtBoxWriteIPAddr";
            this.txtBoxWriteIPAddr.Size = new System.Drawing.Size(108, 24);
            this.txtBoxWriteIPAddr.TabIndex = 4;
            this.txtBoxWriteIPAddr.TextChanged += new System.EventHandler(this.textBox2_TextChanged);
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(6, 194);
            this.label1.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(109, 13);
            this.label1.TabIndex = 5;
            this.label1.Text = "Outstation IP Address";
            // 
            // txtBoxReadIPAddr
            // 
            this.txtBoxReadIPAddr.Location = new System.Drawing.Point(218, 168);
            this.txtBoxReadIPAddr.Margin = new System.Windows.Forms.Padding(2);
            this.txtBoxReadIPAddr.Multiline = true;
            this.txtBoxReadIPAddr.Name = "txtBoxReadIPAddr";
            this.txtBoxReadIPAddr.Size = new System.Drawing.Size(108, 24);
            this.txtBoxReadIPAddr.TabIndex = 6;
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(215, 194);
            this.label2.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(109, 13);
            this.label2.TabIndex = 7;
            this.label2.Text = "Outstation IP Address";
            // 
            // textBoxAddresses
            // 
            this.textBoxAddresses.Location = new System.Drawing.Point(45, 40);
            this.textBoxAddresses.Margin = new System.Windows.Forms.Padding(2);
            this.textBoxAddresses.Multiline = true;
            this.textBoxAddresses.Name = "textBoxAddresses";
            this.textBoxAddresses.Size = new System.Drawing.Size(252, 50);
            this.textBoxAddresses.TabIndex = 8;
            // 
            // Form1
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(341, 410);
            this.Controls.Add(this.textBoxAddresses);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.txtBoxReadIPAddr);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.txtBoxWriteIPAddr);
            this.Controls.Add(this.btnReadData);
            this.Controls.Add(this.btnWriteData);
            this.Controls.Add(this.textBox1);
            this.Controls.Add(this.btnStartMaster);
            this.Margin = new System.Windows.Forms.Padding(2);
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
        private System.Windows.Forms.TextBox textBoxAddresses;
    }
}

