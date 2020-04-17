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
            name = named;
            domains = new List<Domain>();
            properties = new List<Property>();
            loadOrder = new List<OrderingEntry>();
        }

        /// <summary>
        ///     GetFullName applies appropriate modifiers to return the user-friendly version of the name.
        /// </summary>
        /// <returns></returns>
        public string GetFullName()
        {
            var fullName = name;
            if (itemNo >= 0)
                fullName += $" #{itemNo}";
            foreach (var child in properties.Where(child => child.key == "name" && child.value.Length > 0))
                return fullName + ": " + child.value;
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
            var text = $"{indentation}{name}\n{indentation}{{\n";
            var innerIndent = indentation + "\t";
            foreach (var entry in loadOrder)
                if (entry.subDomain != null)
                {
                    text += entry.subDomain.AsText(saving, innerIndent);
                }
                else if (entry.property.value != null)
                {
                    text += $"{innerIndent}{entry.property.key} = {entry.property.value}\n";
                    if (saving && entry.property.isChanged)
                        entry.property.Synchronize();
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
            foreach (var property in properties)
            {
                var value = new ListViewItem(new[] {property.key, property.value})
                {
                    Tag = property,
                    ForeColor = property.isChanged ? Color.Red : Control.DefaultForeColor
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
            var treeNodes = new TreeNode[domains.Count];
            for (var i = 0; i < domains.Count; ++i)
                treeNodes[i] = new TreeNode(domains[i].GetFullName())
                {
                    Name = domains[i].name,
                    Tag = domains[i]
                };

            insertAt.AddRange(treeNodes);

            for (var i = 0; i < domains.Count; ++i) domains[i].ListDomains(treeNodes[i].Nodes);
        }

        #region Members

        public string name { get; }
        public List<Domain> domains { get; }
        public List<Property> properties { get; }
        public List<OrderingEntry> loadOrder { get; }

        /// <summary>
        ///     itemNo is used by for some Domains that repeat multiple times in a parent,
        ///     as PART is repeated multiple times within VESSEL.
        /// </summary>
        public int itemNo { get; set; } = -1;

        #endregion Members
    }
}