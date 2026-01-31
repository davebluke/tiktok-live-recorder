using System;
using System.Windows.Forms;

namespace TikTokRecorderGui
{
    /// <summary>
    /// Settings dialog for configuring the monitor application.
    /// </summary>
    public partial class SettingsForm : Form
    {
        public string StatusDirectory { get; private set; }
        public int RefreshInterval { get; private set; }

        public SettingsForm(string currentStatusDir, int currentRefreshInterval)
        {
            InitializeComponent();
            
            StatusDirectory = currentStatusDir;
            RefreshInterval = currentRefreshInterval;
            
            txtStatusDir.Text = currentStatusDir;
            numRefreshInterval.Value = currentRefreshInterval;
        }

        private void btnBrowse_Click(object sender, EventArgs e)
        {
            using (var dialog = new FolderBrowserDialog())
            {
                dialog.Description = "Select the .tiktok_status directory";
                dialog.SelectedPath = txtStatusDir.Text;
                dialog.ShowNewFolderButton = false;

                if (dialog.ShowDialog() == DialogResult.OK)
                {
                    txtStatusDir.Text = dialog.SelectedPath;
                }
            }
        }

        private void btnSave_Click(object sender, EventArgs e)
        {
            StatusDirectory = txtStatusDir.Text.Trim();
            RefreshInterval = (int)numRefreshInterval.Value;
            
            this.DialogResult = DialogResult.OK;
            this.Close();
        }
    }
}
