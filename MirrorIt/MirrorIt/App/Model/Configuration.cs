using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MirrorIt.App.Model
{
    public class FolderConfiguration
    {
        public string SourcePath { get; set; }
        public string MirrorPath { get; set; }
    }

    public class MirrorConfiguration
    {
        public bool ExcludeHidden { get; set; }

        public bool DeleteFromDest { get; set; }

        public string[] ExcludeFiles { get; set; }

        public string[] ExcludeDirs { get; set; }

        public string[] IncludeFiles { get; set; }

        public string[] IncludeDirs { get; set; }

        public string[] DeleteExcludeFiles { get; set; }

        public string[] DeleteExcludeDirs { get; set; }

        public bool AreSourceFilesFiltered
        {
            get
            {
                return ExcludeHidden || (IncludeFiles != null) || (ExcludeFiles != null) ||
                    (IncludeDirs != null) || (ExcludeDirs != null);
            }
        }
    }

    public class Configuration
    {
        public bool RunAppOnStartup { get; set; }
        public decimal MirrorInterval { get; set; }
        public MirrorConfiguration MirrorConfiguration { get; set; } = new MirrorConfiguration();
        public List<FolderConfiguration> Folders { get; } = new List<FolderConfiguration>();
    }
}
