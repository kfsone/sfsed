using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Windows.Forms;

namespace SFSEd
{
    public class Entry
    {
        public SFSNode Node { get; private set; } = null;
        public SFSLeaf Leaf { get; private set; } = null;

        public Entry(SFSNode node) => Node = node;
        public Entry(SFSLeaf leaf) => Leaf = leaf;
    };

    public class SFSNode
    {
        public string Name { get; }
        public List<SFSNode> Children { get; }
        public List<SFSLeaf> Values { get; }
        public Entry[] Order { get; }

        public SFSNode(string named)
        {
            Contract.Requires(named != null);
            Name = named;
            Children = new List<SFSNode>();
            Values = new List<SFSLeaf>();
            Order = new Entry[] { };
        }

        public string GetFullName()
        {
            foreach (var child in Values)
            {
                if (child.Key == "name" && child.Value.Length > 0)
                {
                    return Name + ": " + child.Value;
                }
            }
            return Name;
        }

        public string AsText(string indentation="")
        {
            string text = $"{indentation}{Name}\n{indentation}\t{{\n";
            string innerdent = indentation + "\t";
            foreach (var entry in Order)
            {
                if (entry.Node != null)
                    text += entry.Node.AsText(innerdent);
                else if (entry.Leaf.Value != null)
                {
                    text += innerdent;
                    text += entry.Leaf.Key;
                    if (entry.Leaf.Value.Length > 0)
                        text += " = " + entry.Leaf.Value + "\n";
                    else
                        text += " =\n";
                }
                text += $"{innerdent}{entry.Leaf.Key} =\n";
            }
            text += indentation + "\t";
            return text;
        }

        public void AddValues(ListView listView)
        {
            Contract.Requires(listView != null);
            listView.Items.Clear();
            listView.View = View.List;

            listView.BeginUpdate();
            foreach (var kvPair in Values)
            {
                listView.Items.Add(new ListViewItem(new[] { kvPair.Key, kvPair.Value }));
            }
            listView.View = View.Details;
            listView.AutoResizeColumns(ColumnHeaderAutoResizeStyle.ColumnContent);
            listView.AutoResizeColumns(ColumnHeaderAutoResizeStyle.HeaderSize);
            listView.EndUpdate();
        }

        public void AddContainers(TreeNodeCollection insertAt)
        {
            Contract.Requires(insertAt != null);
            TreeNode[] treeNodes = new TreeNode[Children.Count];
            for (int i = 0; i < Children.Count; ++i)
            {
                var child = Children[i];
                treeNodes[i] = new TreeNode(child.GetFullName())
                {
                    Name = child.Name,
                    Tag = child
                };
            }
            insertAt.AddRange(treeNodes);
            for (int i = 0; i < Children.Count; ++i)
            {
                Children[i].AddContainers(treeNodes[i].Nodes);
            }
        }
    };
}
