using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.IO;
using System.Linq;

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

                // Prune comments
                int comment = line.IndexOf('#');
                if (comment < 0)
                    comment = line.Length;

                // Clean out whitespace and ignore blank lines.
                string text = line.Substring(0, comment).Trim();
                if (text.Length <= 0)
                    continue;

                if (text == "{")
                {
                    if (string.IsNullOrEmpty(lastLabel))
                        throw new Exception($"{root.LineNo}: Missing label");

                    SFSNode child = new SFSNode(lastLabel);
                    SFSNode parent = openNodes.Peek();
                    parent.Children.Add(child);
                    parent.Order.Append(new Entry(child));
                    openNodes.Push(child);

                    lastLabel = null;
                    ++root.TotalContainers;

                    continue;
                }

                // labels only ever appear before a {
                if (lastLabel != null)
                    throw new Exception($"{root.LineNo}: Uncomsumed label: {lastLabel}");

                if (text == "}")
                {
                    if (openNodes.Count <= 1)
                        throw new Exception($"{root.LineNo}: Too many '}}'s");
                    openNodes.Pop();
                    continue;
                }

                int equals = text.IndexOf("=", StringComparison.InvariantCulture);
                if (equals < 0)
                {
                    lastLabel = text;
                    continue;
                }
                else
                {
                    string key = text.Substring(0, equals).Trim();
                    string value = text.Substring(equals + 1).Trim();
                    var parent = openNodes.Peek();
                    var leaf = new SFSLeaf(key, value);
                    parent.Values.Add(leaf);
                    parent.Order.Append(new Entry(leaf));

                    ++root.TotalValues;
                }
            }

            return root;
        }

        public void WriteToFile(string filename)
        {
            System.IO.File.WriteAllText(filename + ".new", AsText());
        }

    };
}