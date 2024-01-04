namespace Quesadilla
{
    partial class Quesadilla
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
            this.components = new System.ComponentModel.Container();
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(Quesadilla));
            this.mainTimer = new System.Windows.Forms.Timer(this.components);
            this.richTextBoxLogControl1 = new Serilog.Sinks.WinForms.RichTextBoxLogControl();
            this.jsonLogTextBox1 = new Serilog.Sinks.WinForms.JsonLogTextBox();
            this.SuspendLayout();
            // 
            // mainTimer
            // 
            this.mainTimer.Enabled = true;
            this.mainTimer.Interval = 250;
            this.mainTimer.Tick += new System.EventHandler(this.mainTimer_Tick);
            // 
            // richTextBoxLogControl1
            // 
            this.richTextBoxLogControl1.BackColor = System.Drawing.SystemColors.Info;
            this.richTextBoxLogControl1.Dock = System.Windows.Forms.DockStyle.Bottom;
            this.richTextBoxLogControl1.ForContext = "";
            this.richTextBoxLogControl1.Location = new System.Drawing.Point(0, 242);
            this.richTextBoxLogControl1.Margin = new System.Windows.Forms.Padding(2, 2, 2, 2);
            this.richTextBoxLogControl1.Name = "richTextBoxLogControl1";
            this.richTextBoxLogControl1.Size = new System.Drawing.Size(600, 123);
            this.richTextBoxLogControl1.TabIndex = 0;
            this.richTextBoxLogControl1.Text = "";
            // 
            // jsonLogTextBox1
            // 
            this.jsonLogTextBox1.ForContext = "";
            this.jsonLogTextBox1.Location = new System.Drawing.Point(373, 279);
            this.jsonLogTextBox1.LogBorderStyle = System.Windows.Forms.BorderStyle.Fixed3D;
            this.jsonLogTextBox1.LogPadding = new System.Windows.Forms.Padding(3);
            this.jsonLogTextBox1.Name = "jsonLogTextBox1";
            this.jsonLogTextBox1.ReadOnly = false;
            this.jsonLogTextBox1.ScrollBars = System.Windows.Forms.ScrollBars.None;
            this.jsonLogTextBox1.Size = new System.Drawing.Size(5, 5);
            this.jsonLogTextBox1.TabIndex = 1;
            // 
            // Quesadilla
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(600, 365);
            this.Controls.Add(this.jsonLogTextBox1);
            this.Controls.Add(this.richTextBoxLogControl1);
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.Margin = new System.Windows.Forms.Padding(2, 3, 2, 3);
            this.Name = "Quesadilla";
            this.Text = "Quesadilla";
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.Quesadilla_FormClosing);
            this.Load += new System.EventHandler(this.Quesadilla_Load);
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.Timer mainTimer;
        private Serilog.Sinks.WinForms.RichTextBoxLogControl richTextBoxLogControl1;
        private Serilog.Sinks.WinForms.JsonLogTextBox jsonLogTextBox1;
    }
}

