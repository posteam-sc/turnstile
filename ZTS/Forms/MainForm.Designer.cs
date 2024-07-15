namespace ZTS.Forms
{
    partial class MainForm
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
            this.menuStrip1 = new System.Windows.Forms.MenuStrip();
            this.RegistertoolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.registerWatchServerToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.registerTurnstileToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.turnstileControlToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.functionTestToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.menuStrip1.SuspendLayout();
            this.SuspendLayout();
            // 
            // menuStrip1
            // 
            this.menuStrip1.BackColor = System.Drawing.Color.White;
            this.menuStrip1.GripStyle = System.Windows.Forms.ToolStripGripStyle.Visible;
            this.menuStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.RegistertoolStripMenuItem,
            this.turnstileControlToolStripMenuItem,
            this.functionTestToolStripMenuItem});
            this.menuStrip1.Location = new System.Drawing.Point(0, 0);
            this.menuStrip1.Name = "menuStrip1";
            this.menuStrip1.Size = new System.Drawing.Size(766, 28);
            this.menuStrip1.TabIndex = 0;
            this.menuStrip1.Text = "menuStrip1";
            // 
            // RegistertoolStripMenuItem
            // 
            this.RegistertoolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.registerWatchServerToolStripMenuItem,
            this.registerTurnstileToolStripMenuItem});
            this.RegistertoolStripMenuItem.Name = "RegistertoolStripMenuItem";
            this.RegistertoolStripMenuItem.Size = new System.Drawing.Size(57, 24);
            this.RegistertoolStripMenuItem.Text = "Master";
            // 
            // registerWatchServerToolStripMenuItem
            // 
            this.registerWatchServerToolStripMenuItem.Name = "registerWatchServerToolStripMenuItem";
            this.registerWatchServerToolStripMenuItem.Size = new System.Drawing.Size(197, 24);
            this.registerWatchServerToolStripMenuItem.Text = "Register WatchServer";
            this.registerWatchServerToolStripMenuItem.Click += new System.EventHandler(this.registerWatchServerToolStripMenuItem_Click);
            // 
            // registerTurnstileToolStripMenuItem
            // 
            this.registerTurnstileToolStripMenuItem.Name = "registerTurnstileToolStripMenuItem";
            this.registerTurnstileToolStripMenuItem.Size = new System.Drawing.Size(197, 24);
            this.registerTurnstileToolStripMenuItem.Text = "Register Turnstile";
            this.registerTurnstileToolStripMenuItem.Click += new System.EventHandler(this.registerTurnstileToolStripMenuItem_Click);
            // 
            // turnstileControlToolStripMenuItem
            // 
            this.turnstileControlToolStripMenuItem.Name = "turnstileControlToolStripMenuItem";
            this.turnstileControlToolStripMenuItem.Size = new System.Drawing.Size(116, 24);
            this.turnstileControlToolStripMenuItem.Text = "Turnstiles Control";
            this.turnstileControlToolStripMenuItem.Click += new System.EventHandler(this.turnstileControlToolStripMenuItem_Click);
            // 
            // functionTestToolStripMenuItem
            // 
            this.functionTestToolStripMenuItem.Name = "functionTestToolStripMenuItem";
            this.functionTestToolStripMenuItem.Size = new System.Drawing.Size(93, 24);
            this.functionTestToolStripMenuItem.Text = "FunctionTest";
            this.functionTestToolStripMenuItem.Click += new System.EventHandler(this.functionTestToolStripMenuItem_Click);
            // 
            // MainForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.Color.White;
            this.ClientSize = new System.Drawing.Size(766, 446);
            this.Controls.Add(this.menuStrip1);
            this.IsMdiContainer = true;
            this.MainMenuStrip = this.menuStrip1;
            this.Name = "MainForm";
            this.Text = "MainForm";
            this.menuStrip1.ResumeLayout(false);
            this.menuStrip1.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.MenuStrip menuStrip1;
        private System.Windows.Forms.ToolStripMenuItem RegistertoolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem turnstileControlToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem registerTurnstileToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem registerWatchServerToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem functionTestToolStripMenuItem;
    }
}