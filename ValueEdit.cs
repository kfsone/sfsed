using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace SFSEd
{
    public partial class ValueEdit : Form
    {
        SFSLeaf ValueEntry;

        public ValueEdit(SFSLeaf valueEntry)
        {
            InitializeComponent();
            ValueEntry = valueEntry;
            Text = $"Edit {valueEntry.Key}";
            originalText.Text = valueEntry.Original;
            currentText.Text = valueEntry.Value;
            changedText.Text = valueEntry.Value;
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
