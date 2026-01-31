namespace TikTokRecorderGui
{
    partial class SettingsForm
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
            this.panelMain = new System.Windows.Forms.Panel();
            this.lblStatusDir = new System.Windows.Forms.Label();
            this.txtStatusDir = new System.Windows.Forms.TextBox();
            this.btnBrowse = new System.Windows.Forms.Button();
            this.lblRefreshInterval = new System.Windows.Forms.Label();
            this.numRefreshInterval = new System.Windows.Forms.NumericUpDown();
            this.lblSeconds = new System.Windows.Forms.Label();
            this.panelButtons = new System.Windows.Forms.Panel();
            this.btnCancel = new System.Windows.Forms.Button();
            this.btnSave = new System.Windows.Forms.Button();
            this.panelMain.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.numRefreshInterval)).BeginInit();
            this.panelButtons.SuspendLayout();
            this.SuspendLayout();

            // panelMain
            this.panelMain.Controls.Add(this.lblStatusDir);
            this.panelMain.Controls.Add(this.txtStatusDir);
            this.panelMain.Controls.Add(this.btnBrowse);
            this.panelMain.Controls.Add(this.lblRefreshInterval);
            this.panelMain.Controls.Add(this.numRefreshInterval);
            this.panelMain.Controls.Add(this.lblSeconds);
            this.panelMain.Dock = System.Windows.Forms.DockStyle.Fill;
            this.panelMain.Location = new System.Drawing.Point(0, 0);
            this.panelMain.Name = "panelMain";
            this.panelMain.Padding = new System.Windows.Forms.Padding(20);
            this.panelMain.Size = new System.Drawing.Size(450, 150);
            this.panelMain.TabIndex = 0;

            // lblStatusDir
            this.lblStatusDir.AutoSize = true;
            this.lblStatusDir.Font = new System.Drawing.Font("Segoe UI", 9F);
            this.lblStatusDir.ForeColor = System.Drawing.Color.LightGray;
            this.lblStatusDir.Location = new System.Drawing.Point(20, 25);
            this.lblStatusDir.Name = "lblStatusDir";
            this.lblStatusDir.Size = new System.Drawing.Size(95, 15);
            this.lblStatusDir.TabIndex = 0;
            this.lblStatusDir.Text = "Status Directory:";

            // txtStatusDir
            this.txtStatusDir.BackColor = System.Drawing.Color.FromArgb(45, 45, 55);
            this.txtStatusDir.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.txtStatusDir.Font = new System.Drawing.Font("Segoe UI", 9F);
            this.txtStatusDir.ForeColor = System.Drawing.Color.White;
            this.txtStatusDir.Location = new System.Drawing.Point(130, 22);
            this.txtStatusDir.Name = "txtStatusDir";
            this.txtStatusDir.Size = new System.Drawing.Size(230, 23);
            this.txtStatusDir.TabIndex = 1;

            // btnBrowse
            this.btnBrowse.BackColor = System.Drawing.Color.FromArgb(60, 60, 70);
            this.btnBrowse.FlatAppearance.BorderSize = 0;
            this.btnBrowse.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnBrowse.Font = new System.Drawing.Font("Segoe UI", 9F);
            this.btnBrowse.ForeColor = System.Drawing.Color.White;
            this.btnBrowse.Location = new System.Drawing.Point(370, 20);
            this.btnBrowse.Name = "btnBrowse";
            this.btnBrowse.Size = new System.Drawing.Size(60, 27);
            this.btnBrowse.TabIndex = 2;
            this.btnBrowse.Text = "Browse";
            this.btnBrowse.UseVisualStyleBackColor = false;
            this.btnBrowse.Click += new System.EventHandler(this.btnBrowse_Click);

            // lblRefreshInterval
            this.lblRefreshInterval.AutoSize = true;
            this.lblRefreshInterval.Font = new System.Drawing.Font("Segoe UI", 9F);
            this.lblRefreshInterval.ForeColor = System.Drawing.Color.LightGray;
            this.lblRefreshInterval.Location = new System.Drawing.Point(20, 65);
            this.lblRefreshInterval.Name = "lblRefreshInterval";
            this.lblRefreshInterval.Size = new System.Drawing.Size(94, 15);
            this.lblRefreshInterval.TabIndex = 3;
            this.lblRefreshInterval.Text = "Refresh Interval:";

            // numRefreshInterval
            this.numRefreshInterval.BackColor = System.Drawing.Color.FromArgb(45, 45, 55);
            this.numRefreshInterval.Font = new System.Drawing.Font("Segoe UI", 9F);
            this.numRefreshInterval.ForeColor = System.Drawing.Color.White;
            this.numRefreshInterval.Location = new System.Drawing.Point(130, 62);
            this.numRefreshInterval.Maximum = new decimal(new int[] { 60, 0, 0, 0 });
            this.numRefreshInterval.Minimum = new decimal(new int[] { 1, 0, 0, 0 });
            this.numRefreshInterval.Name = "numRefreshInterval";
            this.numRefreshInterval.Size = new System.Drawing.Size(80, 23);
            this.numRefreshInterval.TabIndex = 4;
            this.numRefreshInterval.Value = new decimal(new int[] { 5, 0, 0, 0 });

            // lblSeconds
            this.lblSeconds.AutoSize = true;
            this.lblSeconds.Font = new System.Drawing.Font("Segoe UI", 9F);
            this.lblSeconds.ForeColor = System.Drawing.Color.LightGray;
            this.lblSeconds.Location = new System.Drawing.Point(220, 65);
            this.lblSeconds.Name = "lblSeconds";
            this.lblSeconds.Size = new System.Drawing.Size(50, 15);
            this.lblSeconds.TabIndex = 5;
            this.lblSeconds.Text = "seconds";

            // panelButtons
            this.panelButtons.Controls.Add(this.btnCancel);
            this.panelButtons.Controls.Add(this.btnSave);
            this.panelButtons.Dock = System.Windows.Forms.DockStyle.Bottom;
            this.panelButtons.Location = new System.Drawing.Point(0, 150);
            this.panelButtons.Name = "panelButtons";
            this.panelButtons.Padding = new System.Windows.Forms.Padding(20, 10, 20, 15);
            this.panelButtons.Size = new System.Drawing.Size(450, 50);
            this.panelButtons.TabIndex = 1;

            // btnCancel
            this.btnCancel.Anchor = System.Windows.Forms.AnchorStyles.Right;
            this.btnCancel.BackColor = System.Drawing.Color.FromArgb(60, 60, 70);
            this.btnCancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.btnCancel.FlatAppearance.BorderSize = 0;
            this.btnCancel.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnCancel.Font = new System.Drawing.Font("Segoe UI", 9F);
            this.btnCancel.ForeColor = System.Drawing.Color.White;
            this.btnCancel.Location = new System.Drawing.Point(260, 10);
            this.btnCancel.Name = "btnCancel";
            this.btnCancel.Size = new System.Drawing.Size(80, 28);
            this.btnCancel.TabIndex = 0;
            this.btnCancel.Text = "Cancel";
            this.btnCancel.UseVisualStyleBackColor = false;

            // btnSave
            this.btnSave.Anchor = System.Windows.Forms.AnchorStyles.Right;
            this.btnSave.BackColor = System.Drawing.Color.FromArgb(50, 130, 200);
            this.btnSave.FlatAppearance.BorderSize = 0;
            this.btnSave.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnSave.Font = new System.Drawing.Font("Segoe UI", 9F);
            this.btnSave.ForeColor = System.Drawing.Color.White;
            this.btnSave.Location = new System.Drawing.Point(350, 10);
            this.btnSave.Name = "btnSave";
            this.btnSave.Size = new System.Drawing.Size(80, 28);
            this.btnSave.TabIndex = 1;
            this.btnSave.Text = "Save";
            this.btnSave.UseVisualStyleBackColor = false;
            this.btnSave.Click += new System.EventHandler(this.btnSave_Click);

            // SettingsForm
            this.AcceptButton = this.btnSave;
            this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.Color.FromArgb(35, 35, 42);
            this.CancelButton = this.btnCancel;
            this.ClientSize = new System.Drawing.Size(450, 200);
            this.Controls.Add(this.panelMain);
            this.Controls.Add(this.panelButtons);
            this.Font = new System.Drawing.Font("Segoe UI", 9F);
            this.ForeColor = System.Drawing.Color.White;
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "SettingsForm";
            this.ShowInTaskbar = false;
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "Settings";
            this.panelMain.ResumeLayout(false);
            this.panelMain.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.numRefreshInterval)).EndInit();
            this.panelButtons.ResumeLayout(false);
            this.ResumeLayout(false);
        }

        #endregion

        private System.Windows.Forms.Panel panelMain;
        private System.Windows.Forms.Label lblStatusDir;
        private System.Windows.Forms.TextBox txtStatusDir;
        private System.Windows.Forms.Button btnBrowse;
        private System.Windows.Forms.Label lblRefreshInterval;
        private System.Windows.Forms.NumericUpDown numRefreshInterval;
        private System.Windows.Forms.Label lblSeconds;
        private System.Windows.Forms.Panel panelButtons;
        private System.Windows.Forms.Button btnCancel;
        private System.Windows.Forms.Button btnSave;
    }
}
