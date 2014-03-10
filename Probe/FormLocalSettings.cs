using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using Probe.Properties;
using Probe.Utility;

namespace Probe
{
    public partial class FormLocalSettings : Form
    {
        public FormLocalSettings()
        {
            InitializeComponent();
        }

        private void FormLocalSettings_Load(object sender, System.EventArgs e)
        {
            textReplayFolders.Text = Settings.Default.ReplayFolders;
            textSc2Path.Text = Settings.Default.sc2path;
        }

        private void label2_Click(object sender, System.EventArgs e)
        {

        }

        private void btnReplayFolder_Click(object sender, System.EventArgs e)
        {
            replaysFolderDialog.SelectedPath = textReplayFolders.Text;
            if (replaysFolderDialog.ShowDialog() != DialogResult.OK) return;
            textReplayFolders.Text = replaysFolderDialog.SelectedPath;
        }

        private void button2_Click(object sender, System.EventArgs e)
        {
            sc2pathDialog.FileName = textSc2Path.Text;
            if (sc2pathDialog.ShowDialog() != DialogResult.OK) return;
            textSc2Path.Text = sc2pathDialog.FileName;
        }

        private void FormLocalSettings_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (this.DialogResult == DialogResult.OK)
            {
                Settings.Default.sc2path = textSc2Path.Text;
                Settings.Default.ReplayFolders = textReplayFolders.Text;
                Settings.Default.Save();
            }
        }
    }
}
