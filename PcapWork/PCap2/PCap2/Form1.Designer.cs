namespace PCap2
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
            this.btnSendPkt = new System.Windows.Forms.Button();
            this.SuspendLayout();
            // 
            // btnSendPkt
            // 
            this.btnSendPkt.Location = new System.Drawing.Point(79, 58);
            this.btnSendPkt.Name = "btnSendPkt";
            this.btnSendPkt.Size = new System.Drawing.Size(111, 37);
            this.btnSendPkt.TabIndex = 0;
            this.btnSendPkt.Text = "Send Packet";
            this.btnSendPkt.UseVisualStyleBackColor = true;
            this.btnSendPkt.Click += new System.EventHandler(this.btnSendPkt_Click);
            // 
            // Form1
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(284, 261);
            this.Controls.Add(this.btnSendPkt);
            this.Name = "Form1";
            this.Text = "Form1";
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.Button btnSendPkt;
    }
}

