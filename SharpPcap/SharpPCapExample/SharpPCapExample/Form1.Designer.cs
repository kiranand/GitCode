namespace SharpPCapExample
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
            this.textBoxIPAddr = new System.Windows.Forms.TextBox();
            this.label1 = new System.Windows.Forms.Label();
            this.textBoxEtherAddr = new System.Windows.Forms.TextBox();
            this.label2 = new System.Windows.Forms.Label();
            this.stationConsole = new System.Windows.Forms.TextBox();
            this.buttonInit = new System.Windows.Forms.Button();
            this.SuspendLayout();
            // 
            // button1
            // 
            this.button1.Location = new System.Drawing.Point(133, 99);
            this.button1.Name = "button1";
            this.button1.Size = new System.Drawing.Size(113, 32);
            this.button1.TabIndex = 0;
            this.button1.Text = "Send Packet";
            this.button1.UseVisualStyleBackColor = true;
            this.button1.Click += new System.EventHandler(this.button1_Click);
            // 
            // textBoxIPAddr
            // 
            this.textBoxIPAddr.Location = new System.Drawing.Point(262, 147);
            this.textBoxIPAddr.Multiline = true;
            this.textBoxIPAddr.Name = "textBoxIPAddr";
            this.textBoxIPAddr.Size = new System.Drawing.Size(127, 35);
            this.textBoxIPAddr.TabIndex = 1;
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(311, 185);
            this.label1.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(42, 13);
            this.label1.TabIndex = 2;
            this.label1.Text = "IP Addr";
            // 
            // textBoxEtherAddr
            // 
            this.textBoxEtherAddr.Location = new System.Drawing.Point(12, 147);
            this.textBoxEtherAddr.Multiline = true;
            this.textBoxEtherAddr.Name = "textBoxEtherAddr";
            this.textBoxEtherAddr.Size = new System.Drawing.Size(127, 35);
            this.textBoxEtherAddr.TabIndex = 3;
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(26, 185);
            this.label2.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(72, 13);
            this.label2.TabIndex = 4;
            this.label2.Text = "Ethernet Addr";
            // 
            // stationConsole
            // 
            this.stationConsole.Location = new System.Drawing.Point(29, 214);
            this.stationConsole.Multiline = true;
            this.stationConsole.Name = "stationConsole";
            this.stationConsole.ScrollBars = System.Windows.Forms.ScrollBars.Both;
            this.stationConsole.Size = new System.Drawing.Size(324, 145);
            this.stationConsole.TabIndex = 5;
            this.stationConsole.TextChanged += new System.EventHandler(this.stationConsole_TextChanged);
            // 
            // buttonInit
            // 
            this.buttonInit.Location = new System.Drawing.Point(133, 45);
            this.buttonInit.Name = "buttonInit";
            this.buttonInit.Size = new System.Drawing.Size(113, 32);
            this.buttonInit.TabIndex = 6;
            this.buttonInit.Text = "Initialize";
            this.buttonInit.UseVisualStyleBackColor = true;
            this.buttonInit.Click += new System.EventHandler(this.buttonInit_Click);
            // 
            // Form1
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(401, 371);
            this.Controls.Add(this.buttonInit);
            this.Controls.Add(this.stationConsole);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.textBoxEtherAddr);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.textBoxIPAddr);
            this.Controls.Add(this.button1);
            this.Name = "Form1";
            this.Text = "Form1";
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Button button1;
        private System.Windows.Forms.TextBox textBoxIPAddr;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.TextBox textBoxEtherAddr;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.TextBox stationConsole;
        private System.Windows.Forms.Button buttonInit;
    }
}

