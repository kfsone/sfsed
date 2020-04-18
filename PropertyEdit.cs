using System;
using System.Drawing;
using System.Windows.Forms;

namespace SFSEd
{
    public partial class PropertyEdit : Form
    {
        public PropertyEdit(Property property)
        {
            InitializeComponent();

            Text = $"Edit {property.key}";
            originalText.Text = property.source;
            currentText.Text = property.current;
            changedText.Text = property.pending;
            changedText.Tag = property.pending;
        }

        #region Members

        public string ResultingText => changedText.Text;

        #endregion Members
    }
}