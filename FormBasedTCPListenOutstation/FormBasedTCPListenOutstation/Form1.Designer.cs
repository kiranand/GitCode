namespace FormBasedTCPListenOutstation
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
            this.button1 = new System.Windows.Forms.Button();
            this.textBox1 = new System.Windows.Forms.TextBox();
            this.label1 = new System.Windows.Forms.Label();
            this.radioButton1 = new System.Windows.Forms.RadioButton();
            this.radioButton2 = new System.Windows.Forms.RadioButton();
            this.radioButton3 = new System.Windows.Forms.RadioButton();
            this.button2 = new System.Windows.Forms.Button();
            this.textBoxIpAddrDS = new System.Windows.Forms.TextBox();
            this.label2 = new System.Windows.Forms.Label();
            this.txtBxSplitClientIP = new System.Windows.Forms.TextBox();
            this.label3 = new System.Windows.Forms.Label();
            this.textBoxAddresses = new System.Windows.Forms.TextBox();
            this.textBoxHWAddress = new System.Windows.Forms.TextBox();
            this.label4 = new System.Windows.Forms.Label();
            this.label5 = new System.Windows.Forms.Label();
            this.txtBoxCPULoad = new System.Windows.Forms.TextBox();
            this.SuspendLayout();
            // 
            // button1
            // 
            this.button1.Location = new System.Drawing.Point(22, 11);
            this.button1.Margin = new System.Windows.Forms.Padding(2);
            this.button1.Name = "button1";
            this.button1.Size = new System.Drawing.Size(95, 33);
            this.button1.TabIndex = 0;
            this.button1.Text = "Start Outstation";
            this.button1.UseVisualStyleBackColor = true;
            this.button1.Click += new System.EventHandler(this.button1_Click);
            // 
            // textBox1
            // 
            this.textBox1.Location = new System.Drawing.Point(9, 249);
            this.textBox1.Margin = new System.Windows.Forms.Padding(2);
            this.textBox1.Multiline = true;
            this.textBox1.Name = "textBox1";
            this.textBox1.ScrollBars = System.Windows.Forms.ScrollBars.Both;
            this.textBox1.Size = new System.Drawing.Size(414, 199);
            this.textBox1.TabIndex = 1;
            this.textBox1.TextChanged += new System.EventHandler(this.textBox1_TextChanged);
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(169, 450);
            this.label1.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(58, 13);
            this.label1.TabIndex = 2;
            this.label1.Text = "CONSOLE";
            // 
            // radioButton1
            // 
            this.radioButton1.AutoSize = true;
            this.radioButton1.Location = new System.Drawing.Point(30, 66);
            this.radioButton1.Margin = new System.Windows.Forms.Padding(2);
            this.radioButton1.Name = "radioButton1";
            this.radioButton1.Size = new System.Drawing.Size(85, 17);
            this.radioButton1.TabIndex = 3;
            this.radioButton1.Text = "radioButton1";
            this.radioButton1.UseVisualStyleBackColor = true;
            // 
            // radioButton2
            // 
            this.radioButton2.AutoSize = true;
            this.radioButton2.Location = new System.Drawing.Point(30, 96);
            this.radioButton2.Margin = new System.Windows.Forms.Padding(2);
            this.radioButton2.Name = "radioButton2";
            this.radioButton2.Size = new System.Drawing.Size(85, 17);
            this.radioButton2.TabIndex = 4;
            this.radioButton2.Text = "radioButton2";
            this.radioButton2.UseVisualStyleBackColor = true;
            // 
            // radioButton3
            // 
            this.radioButton3.AutoSize = true;
            this.radioButton3.Location = new System.Drawing.Point(32, 129);
            this.radioButton3.Margin = new System.Windows.Forms.Padding(2);
            this.radioButton3.Name = "radioButton3";
            this.radioButton3.Size = new System.Drawing.Size(85, 17);
            this.radioButton3.TabIndex = 5;
            this.radioButton3.Text = "radioButton3";
            this.radioButton3.UseVisualStyleBackColor = true;
            // 
            // button2
            // 
            this.button2.Location = new System.Drawing.Point(308, 11);
            this.button2.Margin = new System.Windows.Forms.Padding(2);
            this.button2.Name = "button2";
            this.button2.Size = new System.Drawing.Size(95, 33);
            this.button2.TabIndex = 6;
            this.button2.Text = "Set Data Server";
            this.button2.UseVisualStyleBackColor = true;
            this.button2.Click += new System.EventHandler(this.button2_Click);
            // 
            // textBoxIpAddrDS
            // 
            this.textBoxIpAddrDS.Location = new System.Drawing.Point(292, 48);
            this.textBoxIpAddrDS.Margin = new System.Windows.Forms.Padding(2);
            this.textBoxIpAddrDS.Multiline = true;
            this.textBoxIpAddrDS.Name = "textBoxIpAddrDS";
            this.textBoxIpAddrDS.ScrollBars = System.Windows.Forms.ScrollBars.Both;
            this.textBoxIpAddrDS.Size = new System.Drawing.Size(137, 28);
            this.textBoxIpAddrDS.TabIndex = 7;
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(305, 78);
            this.label2.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(118, 13);
            this.label2.TabIndex = 8;
            this.label2.Text = "Data Server IP Address";
            // 
            // txtBxSplitClientIP
            // 
            this.txtBxSplitClientIP.Location = new System.Drawing.Point(292, 107);
            this.txtBxSplitClientIP.Margin = new System.Windows.Forms.Padding(2);
            this.txtBxSplitClientIP.Multiline = true;
            this.txtBxSplitClientIP.Name = "txtBxSplitClientIP";
            this.txtBxSplitClientIP.ScrollBars = System.Windows.Forms.ScrollBars.Both;
            this.txtBxSplitClientIP.Size = new System.Drawing.Size(137, 28);
            this.txtBxSplitClientIP.TabIndex = 10;
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(305, 137);
            this.label3.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(110, 13);
            this.label3.TabIndex = 11;
            this.label3.Text = "Split Client IP Address";
            // 
            // textBoxAddresses
            // 
            this.textBoxAddresses.Location = new System.Drawing.Point(121, 11);
            this.textBoxAddresses.Margin = new System.Windows.Forms.Padding(2);
            this.textBoxAddresses.Multiline = true;
            this.textBoxAddresses.Name = "textBoxAddresses";
            this.textBoxAddresses.ScrollBars = System.Windows.Forms.ScrollBars.Both;
            this.textBoxAddresses.Size = new System.Drawing.Size(167, 65);
            this.textBoxAddresses.TabIndex = 12;
            // 
            // textBoxHWAddress
            // 
            this.textBoxHWAddress.Location = new System.Drawing.Point(292, 172);
            this.textBoxHWAddress.Margin = new System.Windows.Forms.Padding(2);
            this.textBoxHWAddress.Multiline = true;
            this.textBoxHWAddress.Name = "textBoxHWAddress";
            this.textBoxHWAddress.ScrollBars = System.Windows.Forms.ScrollBars.Both;
            this.textBoxHWAddress.Size = new System.Drawing.Size(137, 28);
            this.textBoxHWAddress.TabIndex = 13;
            // 
            // label4
            // 
            this.label4.AutoSize = true;
            this.label4.Location = new System.Drawing.Point(305, 202);
            this.label4.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(119, 13);
            this.label4.TabIndex = 14;
            this.label4.Text = "Split Client HW Address";
            // 
            // label5
            // 
            this.label5.AutoSize = true;
            this.label5.Location = new System.Drawing.Point(59, 184);
            this.label5.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
            this.label5.Name = "label5";
            this.label5.Size = new System.Drawing.Size(56, 13);
            this.label5.TabIndex = 16;
            this.label5.Text = "CPU Load";
            // 
            // txtBoxCPULoad
            // 
            this.txtBoxCPULoad.Location = new System.Drawing.Point(22, 199);
            this.txtBoxCPULoad.Margin = new System.Windows.Forms.Padding(2);
            this.txtBoxCPULoad.Multiline = true;
            this.txtBoxCPULoad.Name = "txtBoxCPULoad";
            this.txtBoxCPULoad.ScrollBars = System.Windows.Forms.ScrollBars.Both;
            this.txtBoxCPULoad.Size = new System.Drawing.Size(137, 28);
            this.txtBoxCPULoad.TabIndex = 17;
            // 
            // Form1
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(453, 476);
            this.Controls.Add(this.txtBoxCPULoad);
            this.Controls.Add(this.label5);
            this.Controls.Add(this.label4);
            this.Controls.Add(this.textBoxHWAddress);
            this.Controls.Add(this.textBoxAddresses);
            this.Controls.Add(this.label3);
            this.Controls.Add(this.txtBxSplitClientIP);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.textBoxIpAddrDS);
            this.Controls.Add(this.button2);
            this.Controls.Add(this.radioButton3);
            this.Controls.Add(this.radioButton2);
            this.Controls.Add(this.radioButton1);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.textBox1);
            this.Controls.Add(this.button1);
            this.Margin = new System.Windows.Forms.Padding(2);
            this.Name = "Form1";
            this.Text = "Form1";
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.Form1_FormClosing);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Button button1;
        private System.Windows.Forms.TextBox textBox1;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.RadioButton radioButton1;
        private System.Windows.Forms.RadioButton radioButton2;
        private System.Windows.Forms.RadioButton radioButton3;
        private System.Windows.Forms.Button button2;
        private System.Windows.Forms.TextBox textBoxIpAddrDS;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.TextBox txtBxSplitClientIP;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.TextBox textBoxAddresses;
        private System.Windows.Forms.TextBox textBoxHWAddress;
        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.Label label5;
        private System.Windows.Forms.TextBox txtBoxCPULoad;
    }
}

