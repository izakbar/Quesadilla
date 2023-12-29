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
            this.richTextBoxLogControl1.ForContext = "";
            this.richTextBoxLogControl1.Location = new System.Drawing.Point(2, 363);
            this.richTextBoxLogControl1.Name = "richTextBoxLogControl1";
            this.richTextBoxLogControl1.Size = new System.Drawing.Size(886, 187);
            this.richTextBoxLogControl1.TabIndex = 0;
            this.richTextBoxLogControl1.Text = "";
            // 
            // jsonLogTextBox1
            // 
            this.jsonLogTextBox1.ForContext = "";
            this.jsonLogTextBox1.Location = new System.Drawing.Point(559, 429);
            this.jsonLogTextBox1.LogBorderStyle = System.Windows.Forms.BorderStyle.Fixed3D;
            this.jsonLogTextBox1.LogPadding = new System.Windows.Forms.Padding(3);
            this.jsonLogTextBox1.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.jsonLogTextBox1.Name = "jsonLogTextBox1";
            this.jsonLogTextBox1.ReadOnly = false;
            this.jsonLogTextBox1.ScrollBars = System.Windows.Forms.ScrollBars.None;
            this.jsonLogTextBox1.Size = new System.Drawing.Size(8, 8);
            this.jsonLogTextBox1.TabIndex = 1;
            // 
            // Quesadilla
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(9F, 20F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(900, 562);
            this.Controls.Add(this.jsonLogTextBox1);
            this.Controls.Add(this.richTextBoxLogControl1);
            this.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
            this.Name = "Quesadilla";
            this.Text = "Form1";
            this.Load += new System.EventHandler(this.Quesadilla_Load);
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.Timer mainTimer;
        private Serilog.Sinks.WinForms.RichTextBoxLogControl richTextBoxLogControl1;
        private Serilog.Sinks.WinForms.JsonLogTextBox jsonLogTextBox1;
    }
}

