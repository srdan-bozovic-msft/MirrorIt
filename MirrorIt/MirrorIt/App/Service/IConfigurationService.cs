using MirrorIt.App.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MirrorIt.App.Service
{
    public interface IConfigurationService
    {
        Configuration Load();
        bool Store(Configuration configuration);
    }
}
