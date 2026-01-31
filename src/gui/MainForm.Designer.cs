namespace TikTokRecorderGui
{
    partial class MainForm
    {
        private System.ComponentModel.IContainer components = null;

        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        private void InitializeComponent()
        {
            this.components = new System.ComponentModel.Container();
            this.tabControl = new System.Windows.Forms.TabControl();
            this.tabStatus = new System.Windows.Forms.TabPage();
            this.panelStatusMain = new System.Windows.Forms.Panel();
            this.dataGridStatus = new System.Windows.Forms.DataGridView();
            this.colUsername = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.colState = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.colResolution = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.colFileSize = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.colCurrentFile = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.colUptime = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.colLastHeartbeat = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.colOutputPath = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.panelStatusFooter = new System.Windows.Forms.Panel();
            this.lblSummary = new System.Windows.Forms.Label();
            this.lblDiskSpace = new System.Windows.Forms.Label();
            this.lblRefreshStatus = new System.Windows.Forms.Label();
            this.tabLiveModels = new System.Windows.Forms.TabPage();
            this.panelLiveModelsMain = new System.Windows.Forms.Panel();
            this.flowLayoutModels = new System.Windows.Forms.FlowLayoutPanel();
            this.panelLiveHeader = new System.Windows.Forms.Panel();
            this.lblSnapshotRefresh = new System.Windows.Forms.Label();
            this.cmbRefreshInterval = new System.Windows.Forms.ComboBox();
            this.btnRefreshNow = new System.Windows.Forms.Button();
            this.panelLiveFooter = new System.Windows.Forms.Panel();
            this.lblLiveDiskSpace = new System.Windows.Forms.Label();
            this.panelHeader = new System.Windows.Forms.Panel();
            this.lblTitle = new System.Windows.Forms.Label();
            this.btnSettings = new System.Windows.Forms.Button();
            this.refreshTimer = new System.Windows.Forms.Timer(this.components);
            this.snapshotTimer = new System.Windows.Forms.Timer(this.components);

            // tabControl
            this.tabControl.Controls.Add(this.tabStatus);
            this.tabControl.Controls.Add(this.tabLiveModels);
            this.tabControl.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tabControl.Font = new System.Drawing.Font("Segoe UI", 10F);
            this.tabControl.Location = new System.Drawing.Point(0, 50);
            this.tabControl.Name = "tabControl";
            this.tabControl.SelectedIndex = 0;
            this.tabControl.Size = new System.Drawing.Size(1000, 550);
            this.tabControl.TabIndex = 0;

            // tabStatus
            this.tabStatus.Controls.Add(this.panelStatusMain);
            this.tabStatus.Controls.Add(this.panelStatusFooter);
            this.tabStatus.Location = new System.Drawing.Point(4, 28);
            this.tabStatus.Name = "tabStatus";
            this.tabStatus.Padding = new System.Windows.Forms.Padding(3);
            this.tabStatus.Size = new System.Drawing.Size(992, 518);
            this.tabStatus.TabIndex = 0;
            this.tabStatus.Text = "📊 Status Dashboard";
            this.tabStatus.UseVisualStyleBackColor = true;

            // panelStatusMain
            this.panelStatusMain.Controls.Add(this.dataGridStatus);
            this.panelStatusMain.Dock = System.Windows.Forms.DockStyle.Fill;
            this.panelStatusMain.Location = new System.Drawing.Point(3, 3);
            this.panelStatusMain.Name = "panelStatusMain";
            this.panelStatusMain.Size = new System.Drawing.Size(986, 472);
            this.panelStatusMain.TabIndex = 0;

            // dataGridStatus
            this.dataGridStatus.AllowUserToAddRows = false;
            this.dataGridStatus.AllowUserToDeleteRows = false;
            this.dataGridStatus.AllowUserToResizeRows = false;
            this.dataGridStatus.AutoSizeColumnsMode = System.Windows.Forms.DataGridViewAutoSizeColumnsMode.Fill;
            this.dataGridStatus.BackgroundColor = System.Drawing.Color.FromArgb(30, 30, 35);
            this.dataGridStatus.BorderStyle = System.Windows.Forms.BorderStyle.None;
            this.dataGridStatus.CellBorderStyle = System.Windows.Forms.DataGridViewCellBorderStyle.SingleHorizontal;
            this.dataGridStatus.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.dataGridStatus.Columns.AddRange(new System.Windows.Forms.DataGridViewColumn[] {
                this.colUsername, this.colState, this.colResolution, this.colFileSize,
                this.colCurrentFile, this.colUptime, this.colLastHeartbeat, this.colOutputPath
            });
            this.dataGridStatus.Dock = System.Windows.Forms.DockStyle.Fill;
            this.dataGridStatus.EnableHeadersVisualStyles = false;
            this.dataGridStatus.GridColor = System.Drawing.Color.FromArgb(50, 50, 60);
            this.dataGridStatus.Location = new System.Drawing.Point(0, 0);
            this.dataGridStatus.MultiSelect = false;
            this.dataGridStatus.Name = "dataGridStatus";
            this.dataGridStatus.ReadOnly = true;
            this.dataGridStatus.RowHeadersVisible = false;
            this.dataGridStatus.RowTemplate.Height = 35;
            this.dataGridStatus.SelectionMode = System.Windows.Forms.DataGridViewSelectionMode.FullRowSelect;
            this.dataGridStatus.Size = new System.Drawing.Size(986, 472);
            this.dataGridStatus.TabIndex = 0;

            // Column definitions
            this.colUsername.HeaderText = "Username";
            this.colUsername.Name = "colUsername";
            this.colUsername.ReadOnly = true;
            this.colUsername.FillWeight = 80;

            this.colState.HeaderText = "State";
            this.colState.Name = "colState";
            this.colState.ReadOnly = true;
            this.colState.FillWeight = 60;

            this.colResolution.HeaderText = "Resolution";
            this.colResolution.Name = "colResolution";
            this.colResolution.ReadOnly = true;
            this.colResolution.FillWeight = 60;

            this.colFileSize.HeaderText = "File Size";
            this.colFileSize.Name = "colFileSize";
            this.colFileSize.ReadOnly = true;
            this.colFileSize.FillWeight = 50;

            this.colCurrentFile.HeaderText = "Current File";
            this.colCurrentFile.Name = "colCurrentFile";
            this.colCurrentFile.ReadOnly = true;
            this.colCurrentFile.FillWeight = 100;

            this.colUptime.HeaderText = "Uptime";
            this.colUptime.Name = "colUptime";
            this.colUptime.ReadOnly = true;
            this.colUptime.FillWeight = 50;

            this.colLastHeartbeat.HeaderText = "Last Update";
            this.colLastHeartbeat.Name = "colLastHeartbeat";
            this.colLastHeartbeat.ReadOnly = true;
            this.colLastHeartbeat.FillWeight = 60;

            this.colOutputPath.HeaderText = "Output Path";
            this.colOutputPath.Name = "colOutputPath";
            this.colOutputPath.ReadOnly = true;
            this.colOutputPath.FillWeight = 100;

            // panelStatusFooter
            this.panelStatusFooter.BackColor = System.Drawing.Color.FromArgb(35, 35, 42);
            this.panelStatusFooter.Controls.Add(this.lblSummary);
            this.panelStatusFooter.Controls.Add(this.lblDiskSpace);
            this.panelStatusFooter.Controls.Add(this.lblRefreshStatus);
            this.panelStatusFooter.Dock = System.Windows.Forms.DockStyle.Bottom;
            this.panelStatusFooter.Location = new System.Drawing.Point(3, 475);
            this.panelStatusFooter.Name = "panelStatusFooter";
            this.panelStatusFooter.Size = new System.Drawing.Size(986, 40);
            this.panelStatusFooter.TabIndex = 1;

            // lblSummary
            this.lblSummary.AutoSize = true;
            this.lblSummary.Font = new System.Drawing.Font("Segoe UI", 9F);
            this.lblSummary.ForeColor = System.Drawing.Color.LightGray;
            this.lblSummary.Location = new System.Drawing.Point(10, 12);
            this.lblSummary.Name = "lblSummary";
            this.lblSummary.Size = new System.Drawing.Size(200, 15);
            this.lblSummary.TabIndex = 0;
            this.lblSummary.Text = "Loading...";

            // lblDiskSpace
            this.lblDiskSpace.AutoSize = true;
            this.lblDiskSpace.Font = new System.Drawing.Font("Segoe UI", 9F);
            this.lblDiskSpace.ForeColor = System.Drawing.Color.FromArgb(100, 200, 150);
            this.lblDiskSpace.Location = new System.Drawing.Point(550, 12);
            this.lblDiskSpace.Name = "lblDiskSpace";
            this.lblDiskSpace.Size = new System.Drawing.Size(150, 15);
            this.lblDiskSpace.TabIndex = 2;
            this.lblDiskSpace.Text = "💾 Free: --";

            // lblRefreshStatus
            this.lblRefreshStatus.Anchor = System.Windows.Forms.AnchorStyles.Right;
            this.lblRefreshStatus.Font = new System.Drawing.Font("Segoe UI", 9F);
            this.lblRefreshStatus.ForeColor = System.Drawing.Color.Gray;
            this.lblRefreshStatus.Location = new System.Drawing.Point(786, 12);
            this.lblRefreshStatus.Name = "lblRefreshStatus";
            this.lblRefreshStatus.Size = new System.Drawing.Size(190, 15);
            this.lblRefreshStatus.TabIndex = 1;
            this.lblRefreshStatus.Text = "Auto-refresh in 5s";
            this.lblRefreshStatus.TextAlign = System.Drawing.ContentAlignment.MiddleRight;

            // tabLiveModels
            this.tabLiveModels.Controls.Add(this.panelLiveModelsMain);
            this.tabLiveModels.Controls.Add(this.panelLiveFooter);
            this.tabLiveModels.Controls.Add(this.panelLiveHeader);
            this.tabLiveModels.Location = new System.Drawing.Point(4, 28);
            this.tabLiveModels.Name = "tabLiveModels";
            this.tabLiveModels.Padding = new System.Windows.Forms.Padding(3);
            this.tabLiveModels.Size = new System.Drawing.Size(992, 518);
            this.tabLiveModels.TabIndex = 1;
            this.tabLiveModels.Text = "📺 Live Models";
            this.tabLiveModels.UseVisualStyleBackColor = true;

            // panelLiveModelsMain
            this.panelLiveModelsMain.Controls.Add(this.flowLayoutModels);
            this.panelLiveModelsMain.Dock = System.Windows.Forms.DockStyle.Fill;
            this.panelLiveModelsMain.Location = new System.Drawing.Point(3, 48);
            this.panelLiveModelsMain.Name = "panelLiveModelsMain";
            this.panelLiveModelsMain.Size = new System.Drawing.Size(986, 427);
            this.panelLiveModelsMain.TabIndex = 0;

            // flowLayoutModels
            this.flowLayoutModels.AutoScroll = true;
            this.flowLayoutModels.BackColor = System.Drawing.Color.FromArgb(25, 25, 30);
            this.flowLayoutModels.Dock = System.Windows.Forms.DockStyle.Fill;
            this.flowLayoutModels.Location = new System.Drawing.Point(0, 0);
            this.flowLayoutModels.Name = "flowLayoutModels";
            this.flowLayoutModels.Padding = new System.Windows.Forms.Padding(10);
            this.flowLayoutModels.Size = new System.Drawing.Size(986, 467);
            this.flowLayoutModels.TabIndex = 0;

            // panelLiveHeader
            this.panelLiveHeader.BackColor = System.Drawing.Color.FromArgb(35, 35, 42);
            this.panelLiveHeader.Controls.Add(this.lblSnapshotRefresh);
            this.panelLiveHeader.Controls.Add(this.cmbRefreshInterval);
            this.panelLiveHeader.Controls.Add(this.btnRefreshNow);
            this.panelLiveHeader.Dock = System.Windows.Forms.DockStyle.Top;
            this.panelLiveHeader.Location = new System.Drawing.Point(3, 3);
            this.panelLiveHeader.Name = "panelLiveHeader";
            this.panelLiveHeader.Size = new System.Drawing.Size(986, 45);
            this.panelLiveHeader.TabIndex = 1;

            // lblSnapshotRefresh
            this.lblSnapshotRefresh.AutoSize = true;
            this.lblSnapshotRefresh.Font = new System.Drawing.Font("Segoe UI", 9F);
            this.lblSnapshotRefresh.ForeColor = System.Drawing.Color.LightGray;
            this.lblSnapshotRefresh.Location = new System.Drawing.Point(10, 14);
            this.lblSnapshotRefresh.Name = "lblSnapshotRefresh";
            this.lblSnapshotRefresh.Size = new System.Drawing.Size(110, 15);
            this.lblSnapshotRefresh.TabIndex = 0;
            this.lblSnapshotRefresh.Text = "Refresh snapshots:";

            // cmbRefreshInterval
            this.cmbRefreshInterval.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cmbRefreshInterval.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.cmbRefreshInterval.Font = new System.Drawing.Font("Segoe UI", 9F);
            this.cmbRefreshInterval.Items.AddRange(new object[] { "15 seconds", "30 seconds", "60 seconds", "120 seconds", "Manual" });
            this.cmbRefreshInterval.Location = new System.Drawing.Point(130, 10);
            this.cmbRefreshInterval.Name = "cmbRefreshInterval";
            this.cmbRefreshInterval.Size = new System.Drawing.Size(120, 23);
            this.cmbRefreshInterval.TabIndex = 1;

            // btnRefreshNow
            this.btnRefreshNow.BackColor = System.Drawing.Color.FromArgb(50, 130, 200);
            this.btnRefreshNow.FlatAppearance.BorderSize = 0;
            this.btnRefreshNow.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnRefreshNow.Font = new System.Drawing.Font("Segoe UI", 9F);
            this.btnRefreshNow.ForeColor = System.Drawing.Color.White;
            this.btnRefreshNow.Location = new System.Drawing.Point(265, 8);
            this.btnRefreshNow.Name = "btnRefreshNow";
            this.btnRefreshNow.Size = new System.Drawing.Size(100, 28);
            this.btnRefreshNow.TabIndex = 2;
            this.btnRefreshNow.Text = "🔄 Refresh";
            this.btnRefreshNow.UseVisualStyleBackColor = false;
            this.btnRefreshNow.Click += new System.EventHandler(this.btnRefreshNow_Click);

            // panelLiveFooter
            this.panelLiveFooter.BackColor = System.Drawing.Color.FromArgb(35, 35, 42);
            this.panelLiveFooter.Controls.Add(this.lblLiveDiskSpace);
            this.panelLiveFooter.Dock = System.Windows.Forms.DockStyle.Bottom;
            this.panelLiveFooter.Location = new System.Drawing.Point(3, 475);
            this.panelLiveFooter.Name = "panelLiveFooter";
            this.panelLiveFooter.Size = new System.Drawing.Size(986, 40);
            this.panelLiveFooter.TabIndex = 2;

            // lblLiveDiskSpace
            this.lblLiveDiskSpace.AutoSize = true;
            this.lblLiveDiskSpace.Font = new System.Drawing.Font("Segoe UI", 9F);
            this.lblLiveDiskSpace.ForeColor = System.Drawing.Color.FromArgb(100, 200, 150);
            this.lblLiveDiskSpace.Location = new System.Drawing.Point(10, 12);
            this.lblLiveDiskSpace.Name = "lblLiveDiskSpace";
            this.lblLiveDiskSpace.Size = new System.Drawing.Size(150, 15);
            this.lblLiveDiskSpace.TabIndex = 0;
            this.lblLiveDiskSpace.Text = "💾 Free: --";

            // panelHeader
            this.panelHeader.BackColor = System.Drawing.Color.FromArgb(40, 40, 48);
            this.panelHeader.Controls.Add(this.lblTitle);
            this.panelHeader.Controls.Add(this.btnSettings);
            this.panelHeader.Dock = System.Windows.Forms.DockStyle.Top;
            this.panelHeader.Location = new System.Drawing.Point(0, 0);
            this.panelHeader.Name = "panelHeader";
            this.panelHeader.Size = new System.Drawing.Size(1000, 50);
            this.panelHeader.TabIndex = 1;

            // lblTitle
            this.lblTitle.AutoSize = true;
            this.lblTitle.Font = new System.Drawing.Font("Segoe UI Semibold", 14F, System.Drawing.FontStyle.Bold);
            this.lblTitle.ForeColor = System.Drawing.Color.White;
            this.lblTitle.Location = new System.Drawing.Point(15, 12);
            this.lblTitle.Name = "lblTitle";
            this.lblTitle.Size = new System.Drawing.Size(280, 25);
            this.lblTitle.TabIndex = 0;
            this.lblTitle.Text = "🎬 TikTok Live Recorder Monitor";

            // btnSettings
            this.btnSettings.Anchor = System.Windows.Forms.AnchorStyles.Right;
            this.btnSettings.BackColor = System.Drawing.Color.FromArgb(60, 60, 70);
            this.btnSettings.FlatAppearance.BorderSize = 0;
            this.btnSettings.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnSettings.Font = new System.Drawing.Font("Segoe UI", 10F);
            this.btnSettings.ForeColor = System.Drawing.Color.White;
            this.btnSettings.Location = new System.Drawing.Point(900, 10);
            this.btnSettings.Name = "btnSettings";
            this.btnSettings.Size = new System.Drawing.Size(85, 30);
            this.btnSettings.TabIndex = 1;
            this.btnSettings.Text = "⚙️ Settings";
            this.btnSettings.UseVisualStyleBackColor = false;
            this.btnSettings.Click += new System.EventHandler(this.btnSettings_Click);

            // refreshTimer
            this.refreshTimer.Interval = 5000;
            this.refreshTimer.Tick += new System.EventHandler(this.refreshTimer_Tick);

            // snapshotTimer
            this.snapshotTimer.Interval = 30000;
            this.snapshotTimer.Tick += new System.EventHandler(this.snapshotTimer_Tick);

            // MainForm
            this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.Color.FromArgb(25, 25, 30);
            this.ClientSize = new System.Drawing.Size(1000, 600);
            this.Controls.Add(this.tabControl);
            this.Controls.Add(this.panelHeader);
            this.Font = new System.Drawing.Font("Segoe UI", 9F);
            this.ForeColor = System.Drawing.Color.White;
            this.MinimumSize = new System.Drawing.Size(800, 500);
            this.Name = "MainForm";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "TikTok Live Recorder Monitor";
            this.Load += new System.EventHandler(this.MainForm_Load);
            this.tabControl.ResumeLayout(false);
            this.tabStatus.ResumeLayout(false);
            this.panelStatusMain.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.dataGridStatus)).EndInit();
            this.panelStatusFooter.ResumeLayout(false);
            this.panelStatusFooter.PerformLayout();
            this.tabLiveModels.ResumeLayout(false);
            this.panelLiveModelsMain.ResumeLayout(false);
            this.panelLiveHeader.ResumeLayout(false);
            this.panelLiveHeader.PerformLayout();
            this.panelHeader.ResumeLayout(false);
            this.panelHeader.PerformLayout();
            this.ResumeLayout(false);
        }

        #endregion

        private System.Windows.Forms.TabControl tabControl;
        private System.Windows.Forms.TabPage tabStatus;
        private System.Windows.Forms.TabPage tabLiveModels;
        private System.Windows.Forms.Panel panelHeader;
        private System.Windows.Forms.Label lblTitle;
        private System.Windows.Forms.Button btnSettings;
        private System.Windows.Forms.Panel panelStatusMain;
        private System.Windows.Forms.DataGridView dataGridStatus;
        private System.Windows.Forms.Panel panelStatusFooter;
        private System.Windows.Forms.Label lblSummary;
        private System.Windows.Forms.Label lblDiskSpace;
        private System.Windows.Forms.Label lblRefreshStatus;
        private System.Windows.Forms.DataGridViewTextBoxColumn colUsername;
        private System.Windows.Forms.DataGridViewTextBoxColumn colState;
        private System.Windows.Forms.DataGridViewTextBoxColumn colResolution;
        private System.Windows.Forms.DataGridViewTextBoxColumn colFileSize;
        private System.Windows.Forms.DataGridViewTextBoxColumn colCurrentFile;
        private System.Windows.Forms.DataGridViewTextBoxColumn colUptime;
        private System.Windows.Forms.DataGridViewTextBoxColumn colLastHeartbeat;
        private System.Windows.Forms.DataGridViewTextBoxColumn colOutputPath;
        private System.Windows.Forms.Panel panelLiveModelsMain;
        private System.Windows.Forms.FlowLayoutPanel flowLayoutModels;
        private System.Windows.Forms.Panel panelLiveHeader;
        private System.Windows.Forms.Label lblSnapshotRefresh;
        private System.Windows.Forms.ComboBox cmbRefreshInterval;
        private System.Windows.Forms.Button btnRefreshNow;
        private System.Windows.Forms.Panel panelLiveFooter;
        private System.Windows.Forms.Label lblLiveDiskSpace;
        private System.Windows.Forms.Timer refreshTimer;
        private System.Windows.Forms.Timer snapshotTimer;
    }
}
