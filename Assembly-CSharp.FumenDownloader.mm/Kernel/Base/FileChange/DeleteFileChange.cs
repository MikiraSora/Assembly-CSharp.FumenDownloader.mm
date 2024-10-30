using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace DpPatches.FumenDownloader.Kernel.Base.FileChange
{
    internal class DeleteFileChange : FileChangeItemBase
    {
        public string FilePath { get; set; }

        public override void Exeuctue()
        {
            if (File.Exists(FilePath))
            {
                File.Delete(FilePath);

                var folderPath = Path.GetDirectoryName(FilePath);
                if (!Directory.GetFiles(folderPath).Any())
                    Directory.Delete(folderPath);
            }
        }

        public override string ToString()
        {
            return "- " + base.ToString() + " delete " + FilePath;
        }
    }
}
