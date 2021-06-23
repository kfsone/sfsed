using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;

namespace SFSEd
{
    /// <summary>
    ///     Domain is a level within the save file hierarchy, which may contain child
    ///     Domains or Properties.
    /// </summary>
    public class Domain
    {
        public Domain(string named)
        {
            Contract.Requires(named != null);
            Name = named;
            Domains = new List<Domain>();
            Properties = new List<Property>();
            LoadOrder = new List<OrderingEntry>();
        }

        /// <summary>
        ///     GetFullName applies appropriate modifiers to return the user-friendly version of the name.
        /// </summary>
        /// <returns></returns>
        public string GetFullName()
        {
            var fullName = Name;
            if (ItemNo >= 0)
                fullName += $" #{ItemNo}";
            foreach (var child in Properties.Where(child => child.Key == "name" && child.Pending.Length > 0))
                return fullName + ": " + child.Pending;
            return fullName;
        }

        /// <summary>
        ///     Returns the current domain and its children in .sfs text format. I deliberately
        ///     chose not to add '\r' on Windows because this can save MBs of cruft in large saves.
        /// </summary>
        /// <param name="indentation">Indentation to prefix lines at this scope with.</param>
        /// <returns>Text representation of this domain and its children.</returns>
        public string AsText(bool saving, string indentation = "")
        {
            var text = $"{indentation}{Name}\n{indentation}{{\n";
            var innerIndent = indentation + "\t";
            foreach (var entry in LoadOrder)
                if (entry.SubDomain != null)
                {
                    text += entry.SubDomain.AsText(saving, innerIndent);
                }
                else
                {
                    text += $"{innerIndent}{entry.Property.Key} = {entry.Property.Pending}\n";
                    if (saving && entry.Property.IsChanged)
                        entry.Property.Synchronize();
                }

            text += indentation + "}\n";
            return text;
        }

        /// <summary>
        ///     ListProperties adds the properties of this domain to the specified ListView.
        /// </summary>
        /// <param name="listView">ListView to populate.</param>
        public void ListProperties(ListView listView)
        {
            Contract.Requires(listView != null);
            listView.Items.Clear();
            listView.View = View.List;

            listView.BeginUpdate();
            foreach (var property in Properties)
            {
                var value = new ListViewItem(new[] { property.Key, property.Pending })
                {
                    Tag = property,
                    ForeColor = property.IsChanged ? Color.Red : Control.DefaultForeColor
                };
                listView.Items.Add(value);
            }

            listView.View = View.Details;
            listView.AutoResizeColumns(ColumnHeaderAutoResizeStyle.ColumnContent);
            listView.AutoResizeColumns(ColumnHeaderAutoResizeStyle.HeaderSize);
            listView.EndUpdate();
        }

        /// <summary>
        ///     ListDomains adds entries for a Domain into a TreeView at a given point.
        /// </summary>
        /// <param name="insertAt">Specific point at which to insert this domain.</param>
        public void ListDomains(TreeNodeCollection insertAt)
        {
            Contract.Requires(insertAt != null);
            var treeNodes = new TreeNode[Domains.Count];
            for (var i = 0; i < Domains.Count; ++i)
                treeNodes[i] = new TreeNode(Domains[i].GetFullName())
                {
                    Name = Domains[i].Name,
                    Tag = Domains[i]
                };

            insertAt.AddRange(treeNodes);

            for (var i = 0; i < Domains.Count; ++i) Domains[i].ListDomains(treeNodes[i].Nodes);
        }

        #region Members

        public string Name { get; }
        public List<Domain> Domains { get; }
        public List<Property> Properties { get; }
        public List<OrderingEntry> LoadOrder { get; }

        /// <summary>
        ///     itemNo is used by for some Domains that repeat multiple times in a parent,
        ///     as PART is repeated multiple times within VESSEL.
        /// </summary>
        public int ItemNo { get; set; } = -1;

        #endregion Members
    }
}