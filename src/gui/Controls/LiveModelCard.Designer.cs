namespace TikTokRecorderGui.Controls
{
    partial class LiveModelCard
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

        #region Component Designer generated code

        private void InitializeComponent()
        {
            this.panelMain = new System.Windows.Forms.Panel();
            this.pictureSnapshot = new System.Windows.Forms.PictureBox();
            this.panelInfo = new System.Windows.Forms.Panel();
            this.lblUsername = new System.Windows.Forms.Label();
            this.lblStatus = new System.Windows.Forms.Label();
            this.lblViewers = new System.Windows.Forms.Label();
            this.panelStatusIndicator = new System.Windows.Forms.Panel();
            this.panelMain.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.pictureSnapshot)).BeginInit();
            this.panelInfo.SuspendLayout();
            this.SuspendLayout();

            // panelMain
            this.panelMain.BackColor = System.Drawing.Color.FromArgb(40, 40, 48);
            this.panelMain.Controls.Add(this.pictureSnapshot);
            this.panelMain.Controls.Add(this.panelInfo);
            this.panelMain.Dock = System.Windows.Forms.DockStyle.Fill;
            this.panelMain.Location = new System.Drawing.Point(5, 5);
            this.panelMain.Name = "panelMain";
            this.panelMain.Padding = new System.Windows.Forms.Padding(3);
            this.panelMain.Size = new System.Drawing.Size(190, 240);
            this.panelMain.TabIndex = 0;

            // pictureSnapshot
            this.pictureSnapshot.BackColor = System.Drawing.Color.FromArgb(30, 30, 35);
            this.pictureSnapshot.Dock = System.Windows.Forms.DockStyle.Fill;
            this.pictureSnapshot.Location = new System.Drawing.Point(3, 3);
            this.pictureSnapshot.Name = "pictureSnapshot";
            this.pictureSnapshot.Size = new System.Drawing.Size(184, 174);
            this.pictureSnapshot.SizeMode = System.Windows.Forms.PictureBoxSizeMode.Zoom;
            this.pictureSnapshot.TabIndex = 0;
            this.pictureSnapshot.TabStop = false;

            // panelInfo
            this.panelInfo.BackColor = System.Drawing.Color.FromArgb(35, 35, 42);
            this.panelInfo.Controls.Add(this.panelStatusIndicator);
            this.panelInfo.Controls.Add(this.lblUsername);
            this.panelInfo.Controls.Add(this.lblStatus);
            this.panelInfo.Controls.Add(this.lblViewers);
            this.panelInfo.Dock = System.Windows.Forms.DockStyle.Bottom;
            this.panelInfo.Location = new System.Drawing.Point(3, 177);
            this.panelInfo.Name = "panelInfo";
            this.panelInfo.Padding = new System.Windows.Forms.Padding(8, 5, 8, 5);
            this.panelInfo.Size = new System.Drawing.Size(184, 60);
            this.panelInfo.TabIndex = 1;

            // panelStatusIndicator
            this.panelStatusIndicator.BackColor = System.Drawing.Color.Gray;
            this.panelStatusIndicator.Location = new System.Drawing.Point(8, 8);
            this.panelStatusIndicator.Name = "panelStatusIndicator";
            this.panelStatusIndicator.Size = new System.Drawing.Size(10, 10);
            this.panelStatusIndicator.TabIndex = 0;

            // lblUsername
            this.lblUsername.AutoSize = true;
            this.lblUsername.Font = new System.Drawing.Font("Segoe UI Semibold", 10F, System.Drawing.FontStyle.Bold);
            this.lblUsername.ForeColor = System.Drawing.Color.White;
            this.lblUsername.Location = new System.Drawing.Point(24, 5);
            this.lblUsername.Name = "lblUsername";
            this.lblUsername.Size = new System.Drawing.Size(72, 19);
            this.lblUsername.TabIndex = 1;
            this.lblUsername.Text = "username";

            // lblStatus
            this.lblStatus.AutoSize = true;
            this.lblStatus.Font = new System.Drawing.Font("Segoe UI", 8F);
            this.lblStatus.ForeColor = System.Drawing.Color.LightGray;
            this.lblStatus.Location = new System.Drawing.Point(8, 28);
            this.lblStatus.Name = "lblStatus";
            this.lblStatus.Size = new System.Drawing.Size(50, 13);
            this.lblStatus.TabIndex = 2;
            this.lblStatus.Text = "WAITING";

            // lblViewers
            this.lblViewers.Anchor = System.Windows.Forms.AnchorStyles.Right;
            this.lblViewers.Font = new System.Drawing.Font("Segoe UI", 8F);
            this.lblViewers.ForeColor = System.Drawing.Color.LightCoral;
            this.lblViewers.Location = new System.Drawing.Point(100, 28);
            this.lblViewers.Name = "lblViewers";
            this.lblViewers.Size = new System.Drawing.Size(76, 13);
            this.lblViewers.TabIndex = 3;
            this.lblViewers.Text = "👁 0";
            this.lblViewers.TextAlign = System.Drawing.ContentAlignment.MiddleRight;

            // LiveModelCard
            this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.Color.FromArgb(25, 25, 30);
            this.Controls.Add(this.panelMain);
            this.Font = new System.Drawing.Font("Segoe UI", 9F);
            this.Margin = new System.Windows.Forms.Padding(10);
            this.Name = "LiveModelCard";
            this.Padding = new System.Windows.Forms.Padding(5);
            this.Size = new System.Drawing.Size(200, 250);
            this.panelMain.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.pictureSnapshot)).EndInit();
            this.panelInfo.ResumeLayout(false);
            this.panelInfo.PerformLayout();
            this.ResumeLayout(false);
        }

        #endregion

        private System.Windows.Forms.Panel panelMain;
        private System.Windows.Forms.PictureBox pictureSnapshot;
        private System.Windows.Forms.Panel panelInfo;
        private System.Windows.Forms.Label lblUsername;
        private System.Windows.Forms.Label lblStatus;
        private System.Windows.Forms.Label lblViewers;
        private System.Windows.Forms.Panel panelStatusIndicator;
    }
}
