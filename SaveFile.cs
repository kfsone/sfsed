using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.IO;
using System.Threading.Tasks;

namespace SFSEd
{

    /// <summary>
    /// SaveFile is a top-level Domain representing an entire sfs file.
    /// </summary>
    public class SaveFile : Domain
    {
        #region Members
        public ulong numDomains { get; private set; } = 0;
        public ulong numProperties { get; private set; } = 0;
        #endregion

        protected SaveFile() : base("/") { }

        public static SaveFile Load(string filename)
        {
            Contract.Requires(filename != null);

           SaveFile save = new SaveFile();

            // Use a stack to track open Domains, starting with the SaveFile itself.
            var openDomains = new Stack<Domain>();
            openDomains.Push(save);

            // Labels appear on the line before the scope opens, e.g.
            //  GAME
            //  {
            //     ...
            //  }
            // so track labels onto the next line.
            string lastLabel = null;
            int lineNo = 0;

            foreach (string line in File.ReadLines(filename))
            {
                ++lineNo;

                // Clean out whitespace and ignore blank lines.
                string text = line.TrimStart();
                if (text.Length <= 0)
                    continue;

                // New scope opening, should follow a label.
                if (text.StartsWith("{"))
                {
                    if (string.IsNullOrEmpty(lastLabel))
                        throw new Exception($"{lineNo}: Missing label");

                    Domain child = new Domain(lastLabel);
                    Domain parent = openDomains.Peek();
                    parent.domains.Add(child);
                    parent.loadOrder.Add(new OrderingEntry(child));
                    openDomains.Push(child);

                    lastLabel = null;
                    ++save.numDomains;

                    continue;
                }

                // labels only ever appear before a {
                if (lastLabel != null)
                    throw new Exception($"{lineNo}: Uncomsumed label: {lastLabel}");

                // Closing a scope
                if (text.StartsWith("}"))
                {
                    if (openDomains.Count <= 1)
                        throw new Exception($"{lineNo}: Too many '}}'s");
                    var priorDomain = openDomains.Pop();
                    if (priorDomain.name == "VESSEL")
                    {
                        int partNo = 0;
                        foreach (var child in priorDomain.domains)
                        {
                            if (child.name == "PART")
                                child.itemNo = partNo++;
                        }
                    }
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
                    // Otherwise it is a property, i.e a key-value pair.
                    string key = text.Substring(0, equals).Trim();
                    string value = text.Substring(equals + 1).TrimStart();
                    var domain = openDomains.Peek();
                    var property = new Property(key, value);
                    domain.properties.Add(property);
                    domain.loadOrder.Add(new OrderingEntry(property));

                    ++save.numProperties;
                }
            }

            return save;
        }

        public string AsText(bool saving)
        {
            // I'm going to go ahead and assume that there's only one top-level Domain, called GAME.
            Contract.Requires(domains.Count == 1);
            Contract.Requires(properties.Count == 0);
            Contract.Requires(domains[0].name == "GAME");
            return domains[0].AsText(saving);
        }

        public async Task Save(string filename)
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
            string text = await Task<string>.Run(() => this.AsText(saving: true)).ConfigureAwait(true);
            await Task.Run(() => File.WriteAllText(filename, text));
        }

    };
}