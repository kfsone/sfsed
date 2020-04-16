using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace SFSEd
{
    public class SFSTree : SFSNode
    {
        ulong LineNo = 0;
        public ulong TotalContainers { get; private set; } = 0;
        public ulong TotalValues { get; private set; } = 0;

        protected SFSTree() : base("/") { }

        public static SFSTree ReadFromFile(string filename)
        {
            Contract.Requires(filename != null);

            SFSTree root = new SFSTree();
            var openNodes = new Stack<SFSNode>();
            string lastLabel = null;

            openNodes.Push(root);
            foreach (string line in File.ReadLines(filename))
            {
                ++root.LineNo;

                // Clean out whitespace and ignore blank lines.
                string text = line.TrimStart();
                if (text.Length <= 0)
                    continue;

                // New scope opening, should follow a label.
                if (text.StartsWith("{"))
                {
                    if (string.IsNullOrEmpty(lastLabel))
                        throw new Exception($"{root.LineNo}: Missing label");

                    SFSNode child = new SFSNode(lastLabel);
                    SFSNode parent = openNodes.Peek();
                    parent.Children.Add(child);
                    parent.Order.Add(new Entry(child));
                    openNodes.Push(child);

                    lastLabel = null;
                    ++root.TotalContainers;

                    continue;
                }

                // labels only ever appear before a {
                if (lastLabel != null)
                    throw new Exception($"{root.LineNo}: Uncomsumed label: {lastLabel}");

                // Closing a scope
                if (text.StartsWith("}"))
                {
                    if (openNodes.Count <= 1)
                        throw new Exception($"{root.LineNo}: Too many '}}'s");
                    openNodes.Pop();
                    continue;
                }

                int equals = text.IndexOf("=", StringComparison.InvariantCulture);
                if (equals < 0)
                {
                    // No equals sign means it was a scope label
                    lastLabel = text.TrimEnd();
                    continue;
                }
                else
                {
                    // Otherwise it is a key-value pair
                    string key = text.Substring(0, equals).Trim();
                    string value = text.Substring(equals + 1).TrimStart();
                    var parent = openNodes.Peek();
                    var leaf = new SFSLeaf(key, value);
                    parent.Values.Add(leaf);
                    parent.Order.Add(new Entry(leaf));

                    ++root.TotalValues;
                }
            }

            return root;
        }

        public string AsText()
        {
            // I'm going to go ahead and assume that there's only the one node, called Game.
            Contract.Requires(Children.Count == 1);
            Contract.Requires(Values.Count == 0);
            Contract.Requires(Children[0].Name == "GAME");
            return Children[0].AsText();
        }

        public async Task WriteToFile(string filename)
        {
            if (File.Exists(filename))
            {
                string backup = filename + ".saved";
                if (File.Exists(backup))
                {
                    File.Delete(backup);
                }
                File.Move(filename, backup);
            }
            string text = await Task<string>.Run(this.AsText).ConfigureAwait(true);
            await Task.Run(() => File.WriteAllText(filename, text));
        }

    };
}