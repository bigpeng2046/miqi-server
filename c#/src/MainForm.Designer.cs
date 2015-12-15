namespace Miqi
{
    partial class MainForm
    {
        /// <summary>
        /// 必需的设计器变量。
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// 清理所有正在使用的资源。
        /// </summary>
        /// <param name="disposing">如果应释放托管资源，为 true；否则为 false。</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows 窗体设计器生成的代码

        /// <summary>
        /// 设计器支持所需的方法 - 不要
        /// 使用代码编辑器修改此方法的内容。
        /// </summary>
        private void InitializeComponent()
        {
            this.components = new System.ComponentModel.Container();
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(MainForm));
            this.niTray = new System.Windows.Forms.NotifyIcon(this.components);
            this.cmsMenus = new System.Windows.Forms.ContextMenuStrip(this.components);
            this.exitToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.lstLogs = new System.Windows.Forms.ListBox();
            this.clearLogsToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.cmsMenus.SuspendLayout();
            this.SuspendLayout();
            // 
            // niTray
            // 
            this.niTray.ContextMenuStrip = this.cmsMenus;
            this.niTray.Icon = ((System.Drawing.Icon)(resources.GetObject("niTray.Icon")));
            this.niTray.Text = "Miqi Server";
            this.niTray.Visible = true;
            this.niTray.MouseDoubleClick += new System.Windows.Forms.MouseEventHandler(this.niTray_MouseDoubleClick);
            // 
            // cmsMenus
            // 
            this.cmsMenus.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.exitToolStripMenuItem,
            this.clearLogsToolStripMenuItem});
            this.cmsMenus.Name = "cmsMenus";
            this.cmsMenus.Size = new System.Drawing.Size(153, 70);
            // 
            // exitToolStripMenuItem
            // 
            this.exitToolStripMenuItem.Name = "exitToolStripMenuItem";
            this.exitToolStripMenuItem.Size = new System.Drawing.Size(152, 22);
            this.exitToolStripMenuItem.Text = "Exit";
            this.exitToolStripMenuItem.Click += new System.EventHandler(this.exitToolStripMenuItem_Click);
            // 
            // lstLogs
            // 
            this.lstLogs.FormattingEnabled = true;
            this.lstLogs.Location = new System.Drawing.Point(12, 22);
            this.lstLogs.Name = "lstLogs";
            this.lstLogs.Size = new System.Drawing.Size(383, 407);
            this.lstLogs.TabIndex = 1;
            // 
            // clearLogsToolStripMenuItem
            // 
            this.clearLogsToolStripMenuItem.Name = "clearLogsToolStripMenuItem";
            this.clearLogsToolStripMenuItem.Size = new System.Drawing.Size(152, 22);
            this.clearLogsToolStripMenuItem.Text = "Clear logs";
            this.clearLogsToolStripMenuItem.Click += new System.EventHandler(this.clearLogsToolStripMenuItem_Click);
            // 
            // MainForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(407, 438);
            this.Controls.Add(this.lstLogs);
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.MaximizeBox = false;
            this.Name = "MainForm";
            this.ShowInTaskbar = false;
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "Logging Screen";
            this.WindowState = System.Windows.Forms.FormWindowState.Minimized;
            this.Load += new System.EventHandler(this.MainForm_Load);
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.MainForm_FormClosing);
            this.cmsMenus.ResumeLayout(false);
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.NotifyIcon niTray;
        private System.Windows.Forms.ContextMenuStrip cmsMenus;
        private System.Windows.Forms.ToolStripMenuItem exitToolStripMenuItem;
        private System.Windows.Forms.ListBox lstLogs;
        private System.Windows.Forms.ToolStripMenuItem clearLogsToolStripMenuItem;
    }
}