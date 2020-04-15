using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
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

        private void SFSEd_Load(object sender, EventArgs e)
        {
            containerView.Nodes.Add("Open a Save Game file...");

            const string filename = @"G:\Steam\SteamApps\Common\Kerbal Space Program\saves\learned\goofed.sfs";
            LoadFile(filename);
        }

        private void ResetTrees()
        {
            valueView.Items.Clear();
            valueView.View = View.List;
            containerView.Nodes.Clear();
        }

        private async void LoadFile(string filename)
        {
            containerView.Nodes.Clear();
            ResetTrees();
            var tree = await Task.Run(() => SFSTree.ReadFromFile(filename)).ConfigureAwait(true);
            tree.AddContainers(containerView.Nodes);
            containerView.TopNode.Expand();
            statusLabel.Text = $"Loaded: {filename}; {tree.TotalContainers} nodes";
        }

        private void ContainerView_AfterSelect(object sender, TreeViewEventArgs e)
        {
            valueView.Items.Clear();
            var sfsNode = (SFSNode)e.Node.Tag;
            if (sfsNode != null)
                sfsNode.AddValues(valueView);
        }
    }
}
