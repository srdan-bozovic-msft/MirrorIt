using MirrorIt.App.Model;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace MirrorIt
{
    public partial class NewEditMirrorForm : Form
    {
        public FolderConfiguration Configuration { get; set; }

        public NewEditMirrorForm()
            : this(true, new FolderConfiguration())
        {
        }

        public NewEditMirrorForm(bool isNew, FolderConfiguration folderConfiguration)
        {
            InitializeComponent();

            Text = isNew ? "New mirror configuration" : "Edit mirror configuration";

            Configuration = folderConfiguration;
            txtSource.Text = folderConfiguration.SourcePath;
            txtMirror.Text = folderConfiguration.MirrorPath;
        }

        private void btnOk_Click(object sender, EventArgs e)
        {
            Configuration.SourcePath = txtSource.Text;
            Configuration.MirrorPath = txtMirror.Text;
        }

        private void btnPickSource_Click(object sender, EventArgs e)
        {
            if(folderBrowserDialog.ShowDialog()==DialogResult.OK)
            {
                txtSource.Text = folderBrowserDialog.SelectedPath;
            }
        }

        private void btnMirror_Click(object sender, EventArgs e)
        {
            if (folderBrowserDialog.ShowDialog() == DialogResult.OK)
            {
                txtMirror.Text = folderBrowserDialog.SelectedPath;
            }
        }

        private void txtSource_TextChanged(object sender, EventArgs e)
        {
            btnOk.Enabled =
                Directory.Exists(txtSource.Text) && Directory.Exists(txtMirror.Text);
        }
    }
}
