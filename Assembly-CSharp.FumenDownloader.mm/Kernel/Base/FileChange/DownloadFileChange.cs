using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace DpPatches.FumenDownloader.Kernel.Base.FileChange
{
    internal class DownloadFileChange : FileChangeItemBase
    {
        public string FilePath { get; set; }
        public string DownloadUrl { get; set; }

        public override void Exeuctue()
        {
            var folderPath = Path.GetDirectoryName(FilePath);
            Directory.CreateDirectory(folderPath);

            SimpleHttp.DownloadFile(DownloadUrl, FilePath);
        }

        public override string ToString()
        {
            return "+ " + base.ToString() + $" download  {DownloadUrl}  ->  {FilePath}";
        }
    }
}
