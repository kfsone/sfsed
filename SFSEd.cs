using System;
using System.ComponentModel;
using System.Diagnostics.Contracts;
using System.IO;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace SFSEd
{
    public partial class SFSEd : Form
    {
        #region Members
        public string currentFilename { get; private set; } = null;
        public string currentBasename { get; private set; } = null;
        protected SaveFile currentSave { get; private set; } = null;
        protected System.Timers.Timer statusTimer;
        #endregion

        private void SFSEd_Load(object sender, EventArgs e)
        {
            domainsView.Nodes.Add("Open a Save Game file...");
        }

        public SFSEd()
        {
            InitializeComponent();
        }

        public void SetStatus(string status)
        {
            statusTimer = null;
            statusLabel.Text = status;
        }

        /// <summary>
        /// SetTransitionalStatus sets a status label immediately but then overwrites it after a number of
        /// seconds has elapsed. This allows for distinguishing between "I just saved a file" and
        /// "the file was saved as X but that was a while ago"...
        /// </summary>
        /// <param name="action">Action to prefix text with.</param>
        /// <param name="information">Information about the action.</param>
        /// <param name="duration">Seconds after which tor emove the 'action term</param>
        public void SetActionStatus(string action, string information, double duration)
        {
            SetStatus(action + ": " + information);
            statusTimer = new System.Timers.Timer(duration * 1000.0) { AutoReset = false, Enabled = true };
            statusTimer.Elapsed += (sender, e) => statusLabel.Text = information;
        }

        public void ClearProperties()
        {
            propertiesView.Items.Clear();
            propertiesView.View = View.List;
        }

        public void ClearDomainsAndProperties()
        {
            ClearProperties();
            domainsView.Nodes.Clear();
        }

        private async void LoadFile(string filename)
        {

            ClearDomainsAndProperties();
            domainsView.Nodes.Add("Loading...");
            SetStatus("Loading...");

            currentSave = await Task.Run(() => SaveFile.Load(filename)).ConfigureAwait(true);
            currentSave.ListDomains(domainsView.Nodes);
            SetActionStatus("Loaded", filename, 3);
            domainsCountLabel.Text = currentSave.numDomains.ToString();
            propertiesCountLabel.Text = currentSave.numProperties.ToString();

            domainsView.BeginUpdate();
            domainsView.Nodes.RemoveAt(0);
            domainsView.TopNode.Expand();
            domainsView.SelectedNode = domainsView.TopNode;
            domainsView.EndUpdate();
        }

        private void containerView_DoubleClick(object sender, EventArgs e)
        {
            // On load, we display a single node telling the user to open a file.
            // People will likely try to click/double click on this before going
            // to the menu system, so in the case where we receive a double-click
            // and no file is open, jump to the open file function.
            if (currentFilename is null)
            {
                openDialog.ShowDialog();
                return;
            }
        }

        private void ContainerView_AfterSelect(object sender, TreeViewEventArgs e)
        {
            ((Domain)e.Node.Tag)?.ListProperties(propertiesView);
        }

        private void openToolStripMenuItem_Click(object sender, EventArgs e)
        {
            openDialog.ShowDialog();
        }

        private void openFileDialog1_FileOk(object sender, CancelEventArgs e)
        {
            if (e.Cancel)
                return;

            var dialog = (OpenFileDialog)sender;
            LoadFile(dialog.FileName);
            currentFilename = dialog.FileName;
            currentBasename = dialog.SafeFileName;
            saveToolStripMenuItem.Enabled = true;
            saveAsToolStripMenuItem.Enabled = true;
        }

        private void saveAsToolStripMenuItem_Click(object sender, EventArgs e)
        {
            saveDialog.FileName = currentBasename;
            saveDialog.ShowDialog();
        }

        private async Task save(string doing, string done, string filename)
        {
            SetStatus($"{doing}: {filename}");

            await currentSave.Save(filename).ConfigureAwait(true);

            SetActionStatus(done, filename, 3);

            // If a property set is being displayed, refresh it.
            ClearProperties();
            ((Domain)domainsView.SelectedNode.Tag)?.ListProperties(propertiesView);
        }

        private async void SaveSFSDialog_FileOk(object sender, CancelEventArgs e)
        {
            if (e.Cancel)
                return;

            var dialog = (SaveFileDialog)sender;

            await save("Saving to", "Saved", dialog.FileName).ConfigureAwait(true);

            currentFilename = dialog.FileName;
            currentBasename = Path.GetFileName(dialog.FileName);
        }

        private async void saveToolStripMenuItem_Click(object sender, EventArgs e)
        {
            await save("Rewriting", "Wrote", currentFilename);
        }

        private void exitToolStripMenuItem_Click(object sender, EventArgs e)
        {
            ClearDomainsAndProperties();
            this.Close();
        }

        private void aboutToolStripMenuItem_Click(object sender, EventArgs e)
        {
            var about = new About();
            about.ShowDialog();
        }

        private void valueView_DoubleClick(object sender, EventArgs e)
        {
            var list = (ListView)sender;
            Contract.Assert(list.SelectedItems.Count == 1);
            var entry = (Property)list.SelectedItems[0].Tag;

            var editDlg = new PropertyEdit(entry);
            var result = editDlg.ShowDialog();
            if (result == DialogResult.OK)
            {
                entry.newValue = editDlg.ResultingText;
                entry.Render(list.SelectedItems[0]);
                list.SelectedItems[0].SubItems[1].Text = entry.newValue;
            }
        }
    }
}
