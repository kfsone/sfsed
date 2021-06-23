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
        public string CurrentFilename { get; private set; } = null;
        public string CurrentBasename { get; private set; } = null;
        protected SaveFile CurrentSave { get; private set; } = null;
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

            CurrentSave = await Task.Run(() => SaveFile.Load(filename)).ConfigureAwait(true);
            CurrentSave.ListDomains(domainsView.Nodes);
            SetActionStatus("Loaded", filename, 3);
            domainsCountLabel.Text = CurrentSave.NumDomains.ToString();
            propertiesCountLabel.Text = CurrentSave.NumProperties.ToString();

            domainsView.BeginUpdate();
            domainsView.Nodes.RemoveAt(0);
            domainsView.TopNode.Expand();
            domainsView.SelectedNode = domainsView.TopNode;
            domainsView.EndUpdate();
        }

        private void ContainerView_DoubleClick(object sender, EventArgs e)
        {
            // On load, we display a single node telling the user to open a file.
            // People will likely try to click/double click on this before going
            // to the menu system, so in the case where we receive a double-click
            // and no file is open, jump to the open file function.
            if (CurrentFilename is null)
            {
                openDialog.ShowDialog();
                return;
            }
        }

        private void ContainerView_AfterSelect(object sender, TreeViewEventArgs e)
        {
            ((Domain)e.Node.Tag)?.ListProperties(propertiesView);
        }

        private void OpenToolStripMenuItem_Click(object sender, EventArgs e)
        {
            openDialog.ShowDialog();
        }

        private void OpenFileDialog1_FileOk(object sender, CancelEventArgs e)
        {
            if (e.Cancel)
                return;

            var dialog = (OpenFileDialog)sender;
            LoadFile(dialog.FileName);
            CurrentFilename = dialog.FileName;
            CurrentBasename = dialog.SafeFileName;
            saveToolStripMenuItem.Enabled = true;
            saveAsToolStripMenuItem.Enabled = true;
        }

        private void SaveAsToolStripMenuItem_Click(object sender, EventArgs e)
        {
            saveDialog.FileName = CurrentBasename;
            saveDialog.ShowDialog();
        }

        private void Save(string doing, string done, string filename)
        {
            SetStatus($"{doing}: {filename}");

            CurrentSave.Save(filename);

            SetActionStatus(done, filename, 3);

            // If a property set is being displayed, refresh it.
            ClearProperties();
            ((Domain)domainsView.SelectedNode.Tag)?.ListProperties(propertiesView);
        }

        private void SaveSFSDialog_FileOk(object sender, CancelEventArgs e)
        {
            if (e.Cancel)
                return;

            var dialog = (SaveFileDialog)sender;

            Save("Saving to", "Saved", dialog.FileName);

            CurrentFilename = dialog.FileName;
            CurrentBasename = Path.GetFileName(dialog.FileName);
        }

        private void SaveToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Save("Rewriting", "Wrote", CurrentFilename);
        }

        private void ExitToolStripMenuItem_Click(object sender, EventArgs e)
        {
            ClearDomainsAndProperties();
            this.Close();
        }

        private void AboutToolStripMenuItem_Click(object sender, EventArgs e)
        {
            var about = new About();
            about.ShowDialog();
        }

        private void ValueView_DoubleClick(object sender, EventArgs e)
        {
            var list = (ListView)sender;
            if (list.SelectedItems.Count == 0)
                return;
            Contract.Assert(list.SelectedItems.Count == 1);
            var selected = list.SelectedItems[0];
            var entry = (Property)selected.Tag;

            var editDlg = new PropertyEdit(entry);
            if (editDlg.ShowDialog() == DialogResult.OK)
            {
                entry.Pending = editDlg.ResultingText;
                entry.Render(list.SelectedItems[0]);
                list.SelectedItems[0].SubItems[1].Text = entry.Pending;
            }
        }
    }
}
