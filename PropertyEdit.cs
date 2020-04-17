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
            originalText.Text = property.value;
            currentText.Text = property.newValue;
            changedText.Text = property.newValue;

            // you don't appear to be able to change the foreground color of read-only
            // input boxes unless you first change the background color, which is fun.
            currentText.BackColor = originalText.BackColor;
            originalText.BackColor = currentText.BackColor;
            SetColors();
        }

        #region Members

        public string ResultingText => changedText.Text;

        #endregion Members

        private void changedText_TextChanged(object sender, EventArgs e)
        {
            SetColors();
        }

        private void SetColors()
        {
            var different = changedText.Text != currentText.Text;
            btnOK.Enabled = different;
            currentText.ForeColor = different ? Color.Red : DefaultForeColor;
            originalText.ForeColor = changedText.Text != originalText.Text ? Color.Red :
                different ? Color.Blue : DefaultForeColor;
        }
    }
}