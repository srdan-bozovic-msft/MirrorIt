using MirrorIt.App.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MirrorIt.App.Service
{
    public interface IMirrorService
    {
        MirrorResult Mirror(string sourcePath, string mirrorPath, MirrorConfiguration configuration);
    }
}
