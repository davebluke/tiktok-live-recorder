using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Windows.Forms;
using TikTokRecorderGui.Controls;
using TikTokRecorderGui.Models;
using TikTokRecorderGui.Services;

namespace TikTokRecorderGui
{
    public partial class MainForm : Form
    {
        private StatusService _statusService;
        private TikTokApiService _tikTokApi;
        private string _statusDirectory;
        private int _refreshCountdown;
        private int _statusRefreshInterval = 5; // seconds
        private int _snapshotRefreshInterval = 30; // seconds
        private BackgroundWorker _liveModelsWorker;

        // Track live model cards
        private Dictionary<string, LiveModelCard> _modelCards = new Dictionary<string, LiveModelCard>();

        public MainForm()
        {
            InitializeComponent();
            ApplyDarkTheme();
            InitializeBackgroundWorker();
        }

        private void InitializeBackgroundWorker()
        {
            _liveModelsWorker = new BackgroundWorker();
            _liveModelsWorker.WorkerReportsProgress = true;
            _liveModelsWorker.DoWork += LiveModelsWorker_DoWork;
            _liveModelsWorker.ProgressChanged += LiveModelsWorker_ProgressChanged;
            _liveModelsWorker.RunWorkerCompleted += LiveModelsWorker_RunWorkerCompleted;
        }

        private void MainForm_Load(object sender, EventArgs e)
        {
            // Default status directory - look in src folder relative to exe or use config
            _statusDirectory = FindStatusDirectory();
            
            _statusService = new StatusService(_statusDirectory);
            _tikTokApi = new TikTokApiService();

            // Set default combo selection
            cmbRefreshInterval.SelectedIndex = 1; // 30 seconds
            cmbRefreshInterval.SelectedIndexChanged += CmbRefreshInterval_SelectedIndexChanged;

            // Start timers
            _refreshCountdown = _statusRefreshInterval;
            refreshTimer.Interval = 1000; // 1 second for countdown
            refreshTimer.Start();

            snapshotTimer.Interval = _snapshotRefreshInterval * 1000;
            snapshotTimer.Start();

            // Initial load
            RefreshStatusGrid();
            UpdateDiskSpace();
            LoadLiveModels();
        }

        private string FindStatusDirectory()
        {
            // Try several locations
            var candidates = new[]
            {
                Path.Combine(Application.StartupPath, ".tiktok_status"),
                Path.Combine(Application.StartupPath, "..", ".tiktok_status"),
                Path.Combine(Application.StartupPath, "..", "..", ".tiktok_status"),
                Path.Combine(Environment.CurrentDirectory, ".tiktok_status"),
                @"C:\Amer2024\github-clones\tiktok4e\tiktok-live-recorder\src\.tiktok_status"
            };

            foreach (var path in candidates)
            {
                if (Directory.Exists(path))
                    return Path.GetFullPath(path);
            }

            // Default fallback
            return Path.Combine(Application.StartupPath, ".tiktok_status");
        }

        private void ApplyDarkTheme()
        {
            // Apply dark theme to DataGridView
            dataGridStatus.DefaultCellStyle.BackColor = Color.FromArgb(30, 30, 35);
            dataGridStatus.DefaultCellStyle.ForeColor = Color.White;
            dataGridStatus.DefaultCellStyle.SelectionBackColor = Color.FromArgb(60, 90, 140);
            dataGridStatus.DefaultCellStyle.SelectionForeColor = Color.White;
            dataGridStatus.DefaultCellStyle.Font = new Font("Segoe UI", 9F);

            dataGridStatus.ColumnHeadersDefaultCellStyle.BackColor = Color.FromArgb(45, 45, 55);
            dataGridStatus.ColumnHeadersDefaultCellStyle.ForeColor = Color.LightGray;
            dataGridStatus.ColumnHeadersDefaultCellStyle.Font = new Font("Segoe UI Semibold", 9F, FontStyle.Bold);
            dataGridStatus.ColumnHeadersDefaultCellStyle.Padding = new Padding(5);

            dataGridStatus.AlternatingRowsDefaultCellStyle.BackColor = Color.FromArgb(35, 35, 40);
        }

        private void RefreshStatusGrid()
        {
            try
            {
                var statuses = _statusService.GetAllStatuses();
                var summary = _statusService.GetSummary();

                // Update grid
                dataGridStatus.Rows.Clear();

                foreach (var status in statuses)
                {
                    // Skip very stale entries (over 1 hour old)
                    if (status.AgeSeconds > 3600)
                        continue;

                    var rowIndex = dataGridStatus.Rows.Add(
                        status.Username,
                        status.StateDisplay,
                        status.Resolution ?? "-",
                        status.FileSizeDisplay,
                        Path.GetFileName(status.CurrentFile) ?? "-",
                        status.UptimeDisplay,
                        status.LastHeartbeatDisplay,
                        status.OutputPath ?? "-"
                    );

                    // Color code the state cell
                    var row = dataGridStatus.Rows[rowIndex];
                    var stateCell = row.Cells["colState"];

                    switch (status.State)
                    {
                        case "RECORDING":
                            stateCell.Style.ForeColor = Color.LimeGreen;
                            stateCell.Style.Font = new Font("Segoe UI", 9F, FontStyle.Bold);
                            break;
                        case "WAITING":
                            stateCell.Style.ForeColor = Color.Orange;
                            break;
                        case "STOPPED":
                            stateCell.Style.ForeColor = Color.Gray;
                            break;
                        case "STALE":
                            row.DefaultCellStyle.ForeColor = Color.DarkGray;
                            break;
                    }
                }

                // Update summary
                lblSummary.Text = summary.SummaryText;
                
                // Update disk space
                UpdateDiskSpace();
            }
            catch (Exception ex)
            {
                lblSummary.Text = "Error: " + ex.Message;
            }
        }

        private void LoadLiveModels()
        {
            if (!_liveModelsWorker.IsBusy)
            {
                _liveModelsWorker.RunWorkerAsync();
            }
        }

        private void LiveModelsWorker_DoWork(object sender, DoWorkEventArgs e)
        {
            try
            {
                // Get usernames from status files (on background thread)
                var statuses = _statusService.GetAllStatuses();
                var usernames = statuses
                    .Where(s => !s.IsStale && s.AgeSeconds < 3600)
                    .Select(s => s.Username)
                    .Distinct()
                    .ToList();

                var results = new List<ModelCardData>();

                foreach (var username in usernames)
                {
                    var status = statuses.First(s => s.Username == username);
                    var cardData = new ModelCardData
                    {
                        Username = username,
                        State = status.State,
                        IsRecording = status.State == "RECORDING"
                    };

                    // Fetch live info (blocking call on background thread)
                    try
                    {
                        var liveInfo = _tikTokApi.GetLiveInfoAsync(username).Result;
                        cardData.IsLive = liveInfo.IsLive;
                        cardData.ViewerCount = liveInfo.ViewerCount;
                        cardData.Title = liveInfo.Title;

                        // Always try to get thumbnail (avatar fallback works for non-live users too)
                        if (!string.IsNullOrEmpty(liveInfo.ThumbnailUrl))
                        {
                            cardData.Thumbnail = _tikTokApi.GetThumbnailAsync(liveInfo.ThumbnailUrl).Result;
                        }
                    }
                    catch (Exception)
                    {
                        // Continue without live info
                    }

                    results.Add(cardData);
                    
                    // Report progress for UI update
                    _liveModelsWorker.ReportProgress(0, cardData);
                }

                e.Result = usernames;
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error in background worker: " + ex.Message);
            }
        }

        private void LiveModelsWorker_ProgressChanged(object sender, ProgressChangedEventArgs e)
        {
            var cardData = e.UserState as ModelCardData;
            if (cardData == null) return;

            UpdateOrCreateModelCard(cardData);
        }

        private void LiveModelsWorker_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            if (e.Result is List<string> usernames)
            {
                // Remove cards for users no longer tracked
                var toRemove = _modelCards.Keys.Except(usernames).ToList();
                foreach (var username in toRemove)
                {
                    LiveModelCard card;
                    if (_modelCards.TryGetValue(username, out card))
                    {
                        flowLayoutModels.Controls.Remove(card);
                        card.Dispose();
                        _modelCards.Remove(username);
                    }
                }
            }
        }

        private void UpdateOrCreateModelCard(ModelCardData data)
        {
            LiveModelCard card;

            if (!_modelCards.TryGetValue(data.Username, out card))
            {
                // Create new card
                card = new LiveModelCard();
                card.Username = data.Username;
                _modelCards[data.Username] = card;
                flowLayoutModels.Controls.Add(card);
            }

            // Update card info
            card.State = data.State;
            card.IsRecording = data.IsRecording;
            card.IsLive = data.IsLive;
            card.ViewerCount = data.ViewerCount;
            card.Title = data.Title;

            if (data.Thumbnail != null)
            {
                card.SetSnapshot(data.Thumbnail);
            }
        }

        private void refreshTimer_Tick(object sender, EventArgs e)
        {
            _refreshCountdown--;

            if (_refreshCountdown <= 0)
            {
                RefreshStatusGrid();
                _refreshCountdown = _statusRefreshInterval;
            }

            lblRefreshStatus.Text = "Auto-refresh in " + _refreshCountdown + "s";
        }

        private void snapshotTimer_Tick(object sender, EventArgs e)
        {
            if (tabControl.SelectedTab == tabLiveModels)
            {
                LoadLiveModels();
            }
        }

        private void CmbRefreshInterval_SelectedIndexChanged(object sender, EventArgs e)
        {
            switch (cmbRefreshInterval.SelectedIndex)
            {
                case 0: _snapshotRefreshInterval = 15; break;
                case 1: _snapshotRefreshInterval = 30; break;
                case 2: _snapshotRefreshInterval = 60; break;
                case 3: _snapshotRefreshInterval = 120; break;
                case 4:
                    snapshotTimer.Stop();
                    return;
            }

            snapshotTimer.Interval = _snapshotRefreshInterval * 1000;
            snapshotTimer.Start();
        }

        private void btnRefreshNow_Click(object sender, EventArgs e)
        {
            LoadLiveModels();
        }

        private void btnSettings_Click(object sender, EventArgs e)
        {
            using (var settingsForm = new SettingsForm(_statusDirectory, _statusRefreshInterval))
            {
                if (settingsForm.ShowDialog() == DialogResult.OK)
                {
                    _statusDirectory = settingsForm.StatusDirectory;
                    _statusRefreshInterval = settingsForm.RefreshInterval;
                    _statusService = new StatusService(_statusDirectory);
                    RefreshStatusGrid();
                }
            }
        }

        protected override void OnFormClosing(FormClosingEventArgs e)
        {
            base.OnFormClosing(e);
            if (_tikTokApi != null) _tikTokApi.Dispose();
        }

        /// <summary>
        /// Updates the disk space display in the footer.
        /// </summary>
        private void UpdateDiskSpace()
        {
            try
            {
                // Try to get the output path from first active status to determine which drive to check
                string driveLetter = "C";
                var statuses = _statusService.GetAllStatuses();
                foreach (var status in statuses)
                {
                    if (!string.IsNullOrEmpty(status.OutputPath) && status.OutputPath.Length >= 2 && status.OutputPath[1] == ':')
                    {
                        driveLetter = status.OutputPath.Substring(0, 1).ToUpper();
                        break;
                    }
                }

                var driveInfo = new DriveInfo(driveLetter);
                if (driveInfo.IsReady)
                {
                    var freeGb = driveInfo.AvailableFreeSpace / (1024.0 * 1024.0 * 1024.0);
                    var totalGb = driveInfo.TotalSize / (1024.0 * 1024.0 * 1024.0);
                    
                    // Choose color based on free space
                    Color diskColor;
                    if (freeGb < 10)
                        diskColor = Color.OrangeRed;
                    else if (freeGb < 50)
                        diskColor = Color.Orange;
                    else
                        diskColor = Color.FromArgb(100, 200, 150);

                    var diskText = string.Format("💾 {0}: Free {1:F1} GB / {2:F0} GB", driveLetter, freeGb, totalGb);

                    // Update both labels
                    lblDiskSpace.ForeColor = diskColor;
                    lblDiskSpace.Text = diskText;
                    lblLiveDiskSpace.ForeColor = diskColor;
                    lblLiveDiskSpace.Text = diskText;
                }
                else
                {
                    lblDiskSpace.Text = "💾 Drive not ready";
                    lblLiveDiskSpace.Text = "💾 Drive not ready";
                }
            }
            catch (Exception)
            {
                lblDiskSpace.Text = "💾 --";
                lblLiveDiskSpace.Text = "💾 --";
            }
        }
    }

    /// <summary>
    /// Data class for passing model card info between threads.
    /// </summary>
    internal class ModelCardData
    {
        public string Username { get; set; }
        public string State { get; set; }
        public bool IsRecording { get; set; }
        public bool IsLive { get; set; }
        public int ViewerCount { get; set; }
        public string Title { get; set; }
        public Image Thumbnail { get; set; }
    }
}
