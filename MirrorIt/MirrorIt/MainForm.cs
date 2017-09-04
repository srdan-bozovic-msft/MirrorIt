using MirrorIt.App.Model;
using MirrorIt.App.Service;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace MirrorIt
{
    public partial class MainForm : Form
    {
        public Configuration Configuration { get; set; }

        public MainForm()
            : this(new Configuration())
        {
        }

        public MainForm(Configuration configuration)
        {
            InitializeComponent();
            Configuration = configuration;
        }

        private void LoadConfiguration()
        {
            var configuration = Configuration;

            chkOnWindowsStart.Checked = configuration.RunAppOnStartup;
            nudMirrorEvery.Value = configuration.MirrorInterval;

            chkDeleteFromDestination.Checked = configuration.MirrorConfiguration.DeleteFromDest;

            lstFolders.Items.Clear();
            foreach (var folder in configuration.Folders)
            {
                var item = new ListViewItem(new[] { folder.SourcePath, folder.MirrorPath });
                lstFolders.Items.Add(item);
            }
        }

        private void SaveConfiguration()
        {
            var configuration = new Configuration();

            configuration.RunAppOnStartup = chkOnWindowsStart.Checked;
            configuration.MirrorInterval = nudMirrorEvery.Value;

            configuration.MirrorConfiguration.DeleteFromDest = chkDeleteFromDestination.Checked;

            foreach (ListViewItem item in lstFolders.Items)
            {
                var folderConfiguration = new FolderConfiguration
                {
                    SourcePath = item.SubItems[0].Text,
                    MirrorPath = item.SubItems[1].Text
                };

                configuration.Folders.Add(folderConfiguration);
            }

            Configuration = configuration;
        }

        private void MainForm_Load(object sender, EventArgs e)
        {
            tabControl.SelectedIndex = 1;
            LoadConfiguration();
        }


        private void btnNew_Click(object sender, EventArgs e)
        {
            var form = new NewEditMirrorForm();
            if (form.ShowDialog() == DialogResult.OK)
            {
                var folder = form.Configuration;
                var item = new ListViewItem(new[] { folder.SourcePath, folder.MirrorPath });
                lstFolders.Items.Add(item);
            }
        }

        private void btnEdit_Click(object sender, EventArgs e)
        {
            var item = lstFolders.SelectedItems[0];
            var configuration = new FolderConfiguration
            {
                SourcePath = item.SubItems[0].Text,
                MirrorPath = item.SubItems[1].Text
            };
            var form = new NewEditMirrorForm(false, configuration);
            if (form.ShowDialog() == DialogResult.OK)
            {
                var folder = form.Configuration;
                item.SubItems[0].Text = folder.SourcePath;
                item.SubItems[1].Text = folder.MirrorPath;
            }
        }

        private void btnDelete_Click(object sender, EventArgs e)
        {
            var index = lstFolders.SelectedIndices[0];
            lstFolders.Items.RemoveAt(index);
        }

        private void lstFolders_SelectedIndexChanged(object sender, EventArgs e)
        {
            btnEdit.Enabled = btnDelete.Enabled = lstFolders.SelectedIndices.Count > 0;
        }

        private void btnOk_Click(object sender, EventArgs e)
        {
            SaveConfiguration();
            this.Close();
        }

        private void btnCancel_Click(object sender, EventArgs e)
        {
            this.Close();
        }
    }
}
