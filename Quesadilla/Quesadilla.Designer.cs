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
            this.alertsOutstanding = new System.Windows.Forms.TextBox();
            this.ChatServerDown = new System.Windows.Forms.Label();
            this.CombatFlag = new System.Windows.Forms.Label();
            this.combatOutstanding = new System.Windows.Forms.TextBox();
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
            this.richTextBoxLogControl1.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.richTextBoxLogControl1.Dock = System.Windows.Forms.DockStyle.Bottom;
            this.richTextBoxLogControl1.ForContext = "";
            this.richTextBoxLogControl1.Location = new System.Drawing.Point(0, 352);
            this.richTextBoxLogControl1.Name = "richTextBoxLogControl1";
            this.richTextBoxLogControl1.Size = new System.Drawing.Size(900, 210);
            this.richTextBoxLogControl1.TabIndex = 0;
            this.richTextBoxLogControl1.Text = "";
            // 
            // alertsOutstanding
            // 
            this.alertsOutstanding.BackColor = System.Drawing.SystemColors.Control;
            this.alertsOutstanding.BorderStyle = System.Windows.Forms.BorderStyle.None;
            this.alertsOutstanding.Font = new System.Drawing.Font("Microsoft Sans Serif", 26.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.alertsOutstanding.ForeColor = System.Drawing.Color.Crimson;
            this.alertsOutstanding.Location = new System.Drawing.Point(0, -2);
            this.alertsOutstanding.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.alertsOutstanding.Name = "alertsOutstanding";
            this.alertsOutstanding.Size = new System.Drawing.Size(66, 60);
            this.alertsOutstanding.TabIndex = 2;
            // 
            // ChatServerDown
            // 
            this.ChatServerDown.AutoSize = true;
            this.ChatServerDown.Font = new System.Drawing.Font("Microsoft Sans Serif", 24F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.ChatServerDown.ForeColor = System.Drawing.Color.Red;
            this.ChatServerDown.Location = new System.Drawing.Point(92, -2);
            this.ChatServerDown.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.ChatServerDown.Name = "ChatServerDown";
            this.ChatServerDown.Size = new System.Drawing.Size(434, 55);
            this.ChatServerDown.TabIndex = 3;
            this.ChatServerDown.Text = "Chat Server Down";
            this.ChatServerDown.Visible = false;
            // 
            // CombatFlag
            // 
            this.CombatFlag.AutoSize = true;
            this.CombatFlag.Font = new System.Drawing.Font("Microsoft Sans Serif", 24F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.CombatFlag.ForeColor = System.Drawing.Color.Red;
            this.CombatFlag.Location = new System.Drawing.Point(92, 55);
            this.CombatFlag.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.CombatFlag.Name = "CombatFlag";
            this.CombatFlag.Size = new System.Drawing.Size(255, 55);
            this.CombatFlag.TabIndex = 4;
            this.CombatFlag.Text = "In Combat";
            this.CombatFlag.Visible = false;
            // 
            // combatOutstanding
            // 
            this.combatOutstanding.BackColor = System.Drawing.SystemColors.Control;
            this.combatOutstanding.BorderStyle = System.Windows.Forms.BorderStyle.None;
            this.combatOutstanding.Font = new System.Drawing.Font("Microsoft Sans Serif", 26.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.combatOutstanding.ForeColor = System.Drawing.Color.Blue;
            this.combatOutstanding.Location = new System.Drawing.Point(0, 68);
            this.combatOutstanding.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.combatOutstanding.Name = "combatOutstanding";
            this.combatOutstanding.Size = new System.Drawing.Size(66, 60);
            this.combatOutstanding.TabIndex = 5;
            // 
            // Quesadilla
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(9F, 20F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(900, 562);
            this.Controls.Add(this.combatOutstanding);
            this.Controls.Add(this.CombatFlag);
            this.Controls.Add(this.ChatServerDown);
            this.Controls.Add(this.alertsOutstanding);
            this.Controls.Add(this.richTextBoxLogControl1);
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.Margin = new System.Windows.Forms.Padding(3, 5, 3, 5);
            this.Name = "Quesadilla";
            this.Text = "Quesadilla";
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.Quesadilla_FormClosing);
            this.Load += new System.EventHandler(this.Quesadilla_Load);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Timer mainTimer;
        private Serilog.Sinks.WinForms.RichTextBoxLogControl richTextBoxLogControl1;
        private System.Windows.Forms.TextBox alertsOutstanding;
        private System.Windows.Forms.Label ChatServerDown;
        private System.Windows.Forms.Label CombatFlag;
        private System.Windows.Forms.TextBox combatOutstanding;
    }
}

