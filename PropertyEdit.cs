using System;
using System.Drawing;
using System.Windows.Forms;

namespace SFSEd
{
    public partial class PropertyEdit : Form
    {
        Property ValueEntry;

        public PropertyEdit(Property valueEntry)
        {
            InitializeComponent();
            ValueEntry = valueEntry;
            Text = $"Edit {valueEntry.key}";
            originalText.Text = valueEntry.value;
            currentText.Text = valueEntry.newValue;
            changedText.Text = valueEntry.newValue;
            // you don't appear to be able to change the foreground color of read-only
            // input boxes unless you first change the background color, which is fun.
            currentText.BackColor = originalText.BackColor;
            originalText.BackColor = currentText.BackColor;
            SetColors();
        }

        public string ResultingText
        {
            get { return this.changedText.Text; }
        }

        private void SetColors()
        {
            bool different = changedText.Text != currentText.Text;
            btnOK.Enabled = different;
            currentText.ForeColor = different ? Color.Red : DefaultForeColor;
            originalText.ForeColor = changedText.Text != originalText.Text ? Color.Red : (different ? Color.Blue : DefaultForeColor); 
        }
        private void changedText_TextChanged(object sender, EventArgs e)
        {
            SetColors();
        }
    }
}
