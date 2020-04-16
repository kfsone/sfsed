using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics.Contracts;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace SFSEd
{
    public partial class SFSEd : Form
    {
        public SFSEd()
        {
            InitializeComponent();
        }

        public string CurrentFilename { get; private set; }
        public string CurrentBasename { get; private set; }

        protected SFSTree currentTree { get; private set; }

        private void SFSEd_Load(object sender, EventArgs e)
        {
            containerView.Nodes.Add("Open a Save Game file...");
        }

        private void ResetTrees()
        {
            valueView.Items.Clear();
            valueView.View = View.List;
            containerView.Nodes.Clear();
        }

        private async void LoadFile(string filename)
        {
            // Clear out values and set the mode to list mode to hide the headers.
            valueView.Items.Clear();
            valueView.View = View.List;
            // Clear out containers.
            containerView.Nodes.Clear();
            containerView.Nodes.Add("Loading...");

            currentTree = await Task.Run(() => SFSTree.ReadFromFile(filename)).ConfigureAwait(true);
            currentTree.AddContainers(containerView.Nodes);
            statusLabel.Text = $"Loaded: {filename}; {currentTree.TotalContainers} nodes, {currentTree.TotalValues} values.";

            containerView.BeginUpdate();
            containerView.Nodes.RemoveAt(0);
            containerView.TopNode.Expand();
            containerView.SelectedNode = containerView.TopNode;
            containerView.EndUpdate();
        }

        private void ContainerView_AfterSelect(object sender, TreeViewEventArgs e)
        {
            valueView.Items.Clear();
            var sfsNode = (SFSNode)e.Node.Tag;
            if (sfsNode != null)
                sfsNode.AddValues(valueView);
        }

        private void openToolStripMenuItem_Click(object sender, EventArgs e)
        {
            OpenSFSDialog.ShowDialog();
        }

        private void openFileDialog1_FileOk(object sender, CancelEventArgs e)
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

        private void saveAsToolStripMenuItem_Click(object sender, EventArgs e)
        {
            SaveSFSDialog.FileName = CurrentBasename;
            SaveSFSDialog.ShowDialog();
        }

        private async void SaveSFSDialog_FileOk(object sender, CancelEventArgs e)
        {
            if (e.Cancel)
                return;

            var dialog = (SaveFileDialog)sender;
            statusLabel.Text = $"Saving to: {dialog.FileName}";

            await currentTree.WriteToFile(dialog.FileName).ConfigureAwait(true);

            CurrentFilename = dialog.FileName;
            CurrentBasename = Path.GetFileName(dialog.FileName);

            statusLabel.Text = $"Saved: {dialog.FileName}";
        }

        private async void saveToolStripMenuItem_Click(object sender, EventArgs e)
        {
            statusLabel.Text = $"Rewriting: {CurrentFilename}";
            await Task.Run(() => currentTree.WriteToFile(CurrentFilename)).ConfigureAwait(true);
            statusLabel.Text = $"Wrote: {CurrentFilename}";
        }

        private void exitToolStripMenuItem_Click(object sender, EventArgs e)
        {
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
            var entry = (SFSLeaf)list.SelectedItems[0].Tag;

            var editDlg = new ValueEdit(entry);
            var result = editDlg.ShowDialog();
            if (result == DialogResult.OK)
            {
                entry.Value = editDlg.ResultingText;
                list.SelectedItems[0].SubItems[1].Text = entry.Value;
            }
        }
    }
}
