using System.Windows.Forms;

namespace SFSEd
{
    public partial class PropertyEdit : Form
    {
        public PropertyEdit(Property property)
        {
            InitializeComponent();

            Text = $"Edit {property.Key}";
            originalText.Text = property.Source;
            currentText.Text = property.Current;
            changedText.Text = property.Pending;
            changedText.Tag = property.Pending;
        }

        #region Members

        public string ResultingText => changedText.Text;

        #endregion Members
    }
}