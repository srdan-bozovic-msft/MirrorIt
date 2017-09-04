using MirrorIt.App.Model;
using MirrorIt.App.Service;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace MirrorIt
{
    class TrayApp : IDisposable
    {
        private NotifyIcon _notifyIcon;
        private bool _isOptionsVisible;
        private readonly BackgroundWorker _backgroundWorker;
        private readonly Timer _timer;
        private readonly IConfigurationService _configurationService;
        private readonly IMirrorService _mirrorService;
        private Configuration Configuration { get; set; }

        public TrayApp()
        {
            _notifyIcon = new NotifyIcon();
            _isOptionsVisible = false;
            _backgroundWorker = new BackgroundWorker();
            _backgroundWorker.DoWork += _backgroundWorker_DoWork;
            _timer = new Timer { Enabled = false };
            _timer.Tick += _timer_Tick;
            _configurationService = new ConfigurationService(Application.CommonAppDataPath, Application.ExecutablePath);
            _mirrorService = new MirrorService();
        }

        private void _backgroundWorker_DoWork(object sender, DoWorkEventArgs e)
        {
            foreach (var folder in Configuration.Folders)
            {
                var result = _mirrorService.Mirror(folder.SourcePath, folder.MirrorPath, Configuration.MirrorConfiguration);
            }
        }

        private void _timer_Tick(object sender, EventArgs e)
        {
            if(!_backgroundWorker.IsBusy)
            {
                _backgroundWorker.RunWorkerAsync();
            }
        }

        public void Run()
        {
            Configuration = _configurationService.Load();

            var resources = new ComponentResourceManager(typeof(MainForm));
            _notifyIcon.Click += Options_Click;
            _notifyIcon.Icon = ((Icon)(resources.GetObject("$this.Icon")));
            _notifyIcon.Text = "MirrorIt";
            _notifyIcon.Visible = true;
            _notifyIcon.ContextMenuStrip = CreateContextMenuStrip();

            _timer.Interval = (int)(60 * 1000 * Configuration.MirrorInterval);
            _timer.Enabled = true;
            _backgroundWorker.RunWorkerAsync();
        }

        public ContextMenuStrip CreateContextMenuStrip()
        {
            var menu = new ContextMenuStrip();
            ToolStripMenuItem item;

            item = new ToolStripMenuItem();
            item.Text = "Options";
            item.Click += Options_Click;
            menu.Items.Add(item);

            menu.Items.Add(new ToolStripSeparator());

            item = new ToolStripMenuItem();
            item.Text = "Exit";
            item.Click += (s, e) => { Application.Exit(); };
            menu.Items.Add(item);

            return menu;
        }

        private void Options_Click(object sender, EventArgs e)
        {
            if (_isOptionsVisible)
                return;
            _isOptionsVisible = true;
            var configuration = _configurationService.Load();
            var form = new MainForm(configuration);
            if(form.ShowDialog()==DialogResult.OK)
            {
                var newConfiguration = form.Configuration;
                if(_configurationService.Store(newConfiguration))
                {
                    var interval = (int)(60 * 1000 * newConfiguration.MirrorInterval);
                    if(_timer.Interval!=interval)
                    {
                        _timer.Enabled = false;
                        _timer.Interval = interval;
                        _timer.Enabled = true;
                    }
                }
            }
            _isOptionsVisible = false;
        }

        public void Dispose()
        {
            _notifyIcon.Dispose();
        }
    }
}
