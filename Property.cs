using System.Drawing;
using System.Windows.Forms;

namespace SFSEd
{
    /// <summary>
    /// Property describes a "key = value" entry, as well as tracking whether the value is
    /// different than the original.
    /// </summary>
    public class Property
    {
        //! Constructor.
        public Property(string key, string value)
        {
            this.key = key;
            this.source = value;
            this._current = null;
            this._pending = null;
        }

        public void Render(ListViewItem into)
        {
            into.SubItems[1].Text = pending;
            into.ForeColor = pending != current ? Color.DarkRed : Form.DefaultForeColor;
        }

        public void Synchronize()
        {
            if (_pending != null)
            {
                _current = (_pending != source) ? _pending : null;
                _pending = null;
            }
        }

        #region Members
        //! key is the label or name of this field.
        public string key { get; }
        //! value we saw on load.
        public string source { get; }
        //! Last value we saved, or null if unchanged
        private string _current = null;
        public string current { get => _current ?? source; }
        //! Value we need to write to disk or null
        private string _pending = null;
        public string pending { get => _pending ?? current; set => _pending = (value != current) ? value : null; }
        //! value we originally loaded from disk
        public bool isChanged => _pending != null;
        #endregion
    };
}
