using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MirrorIt.App.Model
{
    public class MirrorResult
    {
        public bool Succedeed { get; set; }
        public int FilesCopied { get; set; }

        public int FilesUpToDate { get; set; }

        public int FilesDeleted { get; set; }

        public int FilesIgnored { get; set; }

        public int DirectoriesCreated { get; set; }

        public int DirectoriesDeleted { get; set; }

        public int DirectoriesIgnored { get; set; }
    }
}
