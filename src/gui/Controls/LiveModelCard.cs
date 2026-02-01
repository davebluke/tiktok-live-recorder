using System;
using System.Drawing;
using System.Windows.Forms;

namespace TikTokRecorderGui.Controls
{
    /// <summary>
    /// Custom control to display a live model card with snapshot.
    /// </summary>
    public partial class LiveModelCard : UserControl
    {
        private string _username = "";
        private string _state = "WAITING";
        private bool _isLive = false;
        private bool _isRecording = false;
        private int _viewerCount = 0;
        private string _title = "";

        public LiveModelCard()
        {
            InitializeComponent();
            SetRoundedCorners();
        }

        /// <summary>
        /// Gets or sets the TikTok username.
        /// </summary>
        public string Username
        {
            get => _username;
            set
            {
                _username = value;
                lblUsername.Text = value;
            }
        }

        /// <summary>
        /// Gets or sets the recorder state.
        /// </summary>
        public string State
        {
            get => _state;
            set
            {
                _state = value;
                UpdateStatusDisplay();
            }
        }

        /// <summary>
        /// Gets or sets whether the user is currently live.
        /// </summary>
        public bool IsLive
        {
            get => _isLive;
            set
            {
                _isLive = value;
                UpdateStatusDisplay();
            }
        }

        /// <summary>
        /// Gets or sets whether we are currently recording this user.
        /// </summary>
        public bool IsRecording
        {
            get => _isRecording;
            set
            {
                _isRecording = value;
                UpdateStatusDisplay();
            }
        }

        /// <summary>
        /// Gets or sets the viewer count.
        /// </summary>
        public int ViewerCount
        {
            get => _viewerCount;
            set
            {
                _viewerCount = value;
                lblViewers.Text = $"👁 {FormatViewerCount(value)}";
            }
        }

        /// <summary>
        /// Gets or sets the stream title.
        /// </summary>
        public string Title
        {
            get => _title;
            set
            {
                _title = value;
                this.ToolTip(value);
            }
        }

        /// <summary>
        /// Sets the snapshot image.
        /// </summary>
        public void SetSnapshot(Image image)
        {
            if (image == null)
            {
                Console.WriteLine("[LiveModelCard] SetSnapshot: Image is null for " + _username);
                return;
            }

            Console.WriteLine("[LiveModelCard] SetSnapshot: Setting image " + image.Width + "x" + image.Height + " for " + _username);
            
            if (pictureSnapshot.Image != null)
            {
                var oldImage = pictureSnapshot.Image;
                pictureSnapshot.Image = null;
                oldImage.Dispose();
            }
            pictureSnapshot.Image = image;
            
            // Ensure the picture box is visible and properly rendered
            pictureSnapshot.Visible = true;
            pictureSnapshot.BringToFront();
            pictureSnapshot.Invalidate();
            pictureSnapshot.Update();
            
            Console.WriteLine("[LiveModelCard] SetSnapshot: Image set successfully, pictureSnapshot.Image is " + (pictureSnapshot.Image != null ? "not null" : "null"));
        }

        /// <summary>
        /// Clears the snapshot image.
        /// </summary>
        public void ClearSnapshot()
        {
            if (pictureSnapshot.Image != null)
            {
                var oldImage = pictureSnapshot.Image;
                pictureSnapshot.Image = null;
                oldImage.Dispose();
            }
        }

        private void UpdateStatusDisplay()
        {
            // Update status indicator color
            if (_isRecording)
            {
                panelStatusIndicator.BackColor = Color.LimeGreen;
                lblStatus.Text = "🔴 RECORDING";
                lblStatus.ForeColor = Color.LimeGreen;
                // Add yellow border highlight for recording
                panelMain.BackColor = Color.FromArgb(60, 60, 20);
                this.BorderStyle = BorderStyle.None;
                this.Padding = new Padding(3);
                this.BackColor = Color.Gold;
            }
            else if (_isLive)
            {
                panelStatusIndicator.BackColor = Color.Red;
                lblStatus.Text = "🔴 LIVE";
                lblStatus.ForeColor = Color.Red;
                // Reset border for non-recording
                this.Padding = new Padding(0);
                this.BackColor = Color.Transparent;
                panelMain.BackColor = Color.FromArgb(45, 45, 50);
            }
            else
            {
                // Reset border for non-recording
                this.Padding = new Padding(0);
                this.BackColor = Color.Transparent;
                panelMain.BackColor = Color.FromArgb(45, 45, 50);
                
                switch (_state)
                {
                    case "WAITING":
                        panelStatusIndicator.BackColor = Color.Orange;
                        lblStatus.Text = "WAITING";
                        lblStatus.ForeColor = Color.Orange;
                        break;
                    case "STOPPED":
                        panelStatusIndicator.BackColor = Color.Gray;
                        lblStatus.Text = "STOPPED";
                        lblStatus.ForeColor = Color.Gray;
                        break;
                    default:
                        panelStatusIndicator.BackColor = Color.Gray;
                        lblStatus.Text = _state ?? "OFFLINE";
                        lblStatus.ForeColor = Color.Gray;
                        break;
                }
            }

            // Make indicator circular
            MakeCircular(panelStatusIndicator);
        }

        private void SetRoundedCorners()
        {
            // Apply rounded corners to main panel
            panelMain.Region = CreateRoundedRegion(panelMain.Width, panelMain.Height, 8);
        }

        private void MakeCircular(Control control)
        {
            System.Drawing.Drawing2D.GraphicsPath path = new System.Drawing.Drawing2D.GraphicsPath();
            path.AddEllipse(0, 0, control.Width, control.Height);
            control.Region = new Region(path);
        }

        private Region CreateRoundedRegion(int width, int height, int radius)
        {
            System.Drawing.Drawing2D.GraphicsPath path = new System.Drawing.Drawing2D.GraphicsPath();
            path.AddArc(0, 0, radius * 2, radius * 2, 180, 90);
            path.AddArc(width - radius * 2, 0, radius * 2, radius * 2, 270, 90);
            path.AddArc(width - radius * 2, height - radius * 2, radius * 2, radius * 2, 0, 90);
            path.AddArc(0, height - radius * 2, radius * 2, radius * 2, 90, 90);
            path.CloseFigure();
            return new Region(path);
        }

        private string FormatViewerCount(int count)
        {
            if (count >= 1000000)
                return $"{count / 1000000.0:F1}M";
            if (count >= 1000)
                return $"{count / 1000.0:F1}K";
            return count.ToString();
        }

        private void ToolTip(string text)
        {
            var tooltip = new ToolTip();
            tooltip.SetToolTip(this, text);
            tooltip.SetToolTip(lblUsername, text);
        }

        protected override void OnResize(EventArgs e)
        {
            base.OnResize(e);
            SetRoundedCorners();
        }
    }
}
