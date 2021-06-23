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
            this.Key = key;
            this.Source = value;
            this._current = null;
            this._pending = null;
        }

        public void Render(ListViewItem into)
        {
            into.SubItems[1].Text = Pending;
            into.ForeColor = Pending != Current ? Color.DarkRed : Form.DefaultForeColor;
        }

        public void Synchronize()
        {
            if (_pending != null)
            {
                _current = (_pending != Source) ? _pending : null;
                _pending = null;
            }
        }

        #region Members
        //! key is the label or name of this field.
        public string Key { get; }
        //! value we saw on load.
        public string Source { get; }
        //! Last value we saved, or null if unchanged
        private string _current = null;
        public string Current { get => _current ?? Source; }
        //! Value we need to write to disk or null
        private string _pending = null;
        public string Pending { get => _pending ?? Current; set => _pending = (value != Current) ? value : null; }
        //! value we originally loaded from disk
        public bool IsChanged => _pending != null;
        #endregion
    };
}
