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
            this.value = value;
            this._newValue = null;
        }

        public void Render(ListViewItem into)
        {
            into.SubItems[1].Text = newValue;
            into.ForeColor = isChanged ? Color.Red : Form.DefaultForeColor;
        }

        public void Synchronize()
        {
            if (_newValue != null)
                (value, _newValue) = (_newValue, null);
        }

        #region Members
        //! key is the label or name of this field.
        public string key { get; }
        //! value we originally loaded from disk
        public string value { get; private set; }
        //! null or new value specified by user
        private string _newValue = null;
        public string newValue { get => _newValue ?? this.value; set => this._newValue = this.value == value ? null : value; }
        public bool isChanged => _newValue != null;
        #endregion
    };
}
