using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace DpPatches.FumenDownloader.Kernel.Base.FileChange
{
    internal abstract class FileChangeItemBase : IFileChangeItem
    {
        private static int id = 0;
        public int Id { get; } = id++;

        public abstract void Exeuctue();

        public override string ToString()
        {
            return $"#{Id}";
        }
    }
}
