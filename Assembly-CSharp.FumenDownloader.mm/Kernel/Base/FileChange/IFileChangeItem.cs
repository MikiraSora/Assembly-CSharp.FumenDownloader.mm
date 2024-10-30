using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace DpPatches.FumenDownloader.Kernel.Base.FileChange
{
    public interface IFileChangeItem
    {
        void Exeuctue();

        int Id { get; }
    }
}
