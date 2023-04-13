using Demon.Functions.Objects;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Demon.Functions.Backups
{
    public class IncrementalBackup : Backuper
    {
        public IncrementalBackup(Core core, string algorithm) : base(core, algorithm)
        {

        }

        public override void CopyMain(string source, string destination, Snapshot snapshot)
        {
            base.CopyMain(source, destination, snapshot);

            //Vždy přidá source ve formátu Json (jako string) do UpdatedPaths
            DirectoryInfo directoryInfo = new DirectoryInfo(source);
            UpdatePaths(directoryInfo);

        }

        public void UpdatePaths(DirectoryInfo directoryInfo)
        {
            foreach (var dir in directoryInfo.GetDirectories())
            {
                Objects.Path snapshotJson = new Objects.Path()
                {
                    FileName = dir.Name,
                    FullPath = dir.FullName,
                    UpdateTime = DateTime.Now
                };

                UpdatedPaths += JsonConvert.SerializeObject(snapshotJson);
                UpdatePaths(dir);
            }

            foreach (var file in directoryInfo.GetFiles())
            {
                Objects.Path snapshotJson = new Objects.Path()
                {
                    FileName = file.Name,
                    FullPath = file.FullName,
                    UpdateTime = DateTime.Now
                };

                UpdatedPaths += JsonConvert.SerializeObject(snapshotJson);
            }
        }
    }
}
