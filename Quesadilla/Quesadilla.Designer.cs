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
            this.intelLog = new Serilog.Sinks.WinForms.RichTextBoxLogControl();
            this.alertsOutstanding = new System.Windows.Forms.TextBox();
            this.ChatServerDown = new System.Windows.Forms.Label();
            this.CombatFlag = new System.Windows.Forms.Label();
            this.buttonIntelSize = new System.Windows.Forms.Button();
            this.lblCurrentClient = new System.Windows.Forms.Label();
            this.SuspendLayout();
            // 
            // mainTimer
            // 
            this.mainTimer.Enabled = true;
            this.mainTimer.Interval = 250;
            this.mainTimer.Tick += new System.EventHandler(this.mainTimer_Tick);
            // 
            // intelLog
            // 
            this.intelLog.BackColor = System.Drawing.SystemColors.Info;
            this.intelLog.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.intelLog.Dock = System.Windows.Forms.DockStyle.Bottom;
            this.intelLog.Enabled = false;
            this.intelLog.ForContext = "";
            this.intelLog.Location = new System.Drawing.Point(0, 220);
            this.intelLog.Margin = new System.Windows.Forms.Padding(3, 2, 3, 2);
            this.intelLog.Name = "intelLog";
            this.intelLog.Size = new System.Drawing.Size(992, 229);
            this.intelLog.TabIndex = 0;
            this.intelLog.Text = "";
            this.intelLog.TextChanged += new System.EventHandler(this.intelLog_TextChanged);
            // 
            // alertsOutstanding
            // 
            this.alertsOutstanding.BackColor = System.Drawing.SystemColors.Control;
            this.alertsOutstanding.BorderStyle = System.Windows.Forms.BorderStyle.None;
            this.alertsOutstanding.Font = new System.Drawing.Font("Microsoft Sans Serif", 26.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.alertsOutstanding.ForeColor = System.Drawing.Color.Crimson;
            this.alertsOutstanding.Location = new System.Drawing.Point(0, -1);
            this.alertsOutstanding.Margin = new System.Windows.Forms.Padding(4);
            this.alertsOutstanding.Name = "alertsOutstanding";
            this.alertsOutstanding.Size = new System.Drawing.Size(59, 40);
            this.alertsOutstanding.TabIndex = 2;
            // 
            // ChatServerDown
            // 
            this.ChatServerDown.AutoSize = true;
            this.ChatServerDown.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.ChatServerDown.ForeColor = System.Drawing.Color.Red;
            this.ChatServerDown.Location = new System.Drawing.Point(12, 80);
            this.ChatServerDown.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.ChatServerDown.Name = "ChatServerDown";
            this.ChatServerDown.Size = new System.Drawing.Size(154, 20);
            this.ChatServerDown.TabIndex = 3;
            this.ChatServerDown.Text = "Chat Server Down";
            this.ChatServerDown.Visible = false;
            // 
            // CombatFlag
            // 
            this.CombatFlag.AutoSize = true;
            this.CombatFlag.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.CombatFlag.ForeColor = System.Drawing.Color.Red;
            this.CombatFlag.Location = new System.Drawing.Point(831, 191);
            this.CombatFlag.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.CombatFlag.Name = "CombatFlag";
            this.CombatFlag.Size = new System.Drawing.Size(92, 20);
            this.CombatFlag.TabIndex = 4;
            this.CombatFlag.Text = "In Combat";
            this.CombatFlag.Visible = false;
            // 
            // buttonIntelSize
            // 
            this.buttonIntelSize.BackColor = System.Drawing.Color.Transparent;
            this.buttonIntelSize.Font = new System.Drawing.Font("Arial", 8F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.buttonIntelSize.Location = new System.Drawing.Point(969, 219);
            this.buttonIntelSize.Margin = new System.Windows.Forms.Padding(3, 2, 3, 2);
            this.buttonIntelSize.Name = "buttonIntelSize";
            this.buttonIntelSize.Size = new System.Drawing.Size(23, 22);
            this.buttonIntelSize.TabIndex = 6;
            this.buttonIntelSize.Text = "▼";
            this.buttonIntelSize.UseVisualStyleBackColor = false;
            this.buttonIntelSize.Click += new System.EventHandler(this.buttonIntelSize_Click);
            // 
            // lblCurrentClient
            // 
            this.lblCurrentClient.AutoSize = true;
            this.lblCurrentClient.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblCurrentClient.ForeColor = System.Drawing.Color.Red;
            this.lblCurrentClient.Location = new System.Drawing.Point(13, 100);
            this.lblCurrentClient.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.lblCurrentClient.Name = "lblCurrentClient";
            this.lblCurrentClient.Size = new System.Drawing.Size(120, 20);
            this.lblCurrentClient.TabIndex = 7;
            this.lblCurrentClient.Text = "Current Client";
            this.lblCurrentClient.Visible = false;
            // 
            // Quesadilla
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(8F, 16F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(992, 449);
            this.Controls.Add(this.lblCurrentClient);
            this.Controls.Add(this.buttonIntelSize);
            this.Controls.Add(this.CombatFlag);
            this.Controls.Add(this.ChatServerDown);
            this.Controls.Add(this.alertsOutstanding);
            this.Controls.Add(this.intelLog);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle;
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
            this.MaximizeBox = false;
            this.Name = "Quesadilla";
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.Quesadilla_FormClosing);
            this.Load += new System.EventHandler(this.Quesadilla_Load);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Timer mainTimer;
        private Serilog.Sinks.WinForms.RichTextBoxLogControl intelLog;
        private System.Windows.Forms.TextBox alertsOutstanding;
        private System.Windows.Forms.Label ChatServerDown;
        private System.Windows.Forms.Label CombatFlag;
        private System.Windows.Forms.Button buttonIntelSize;
        private System.Windows.Forms.Label lblCurrentClient;
    }
}

