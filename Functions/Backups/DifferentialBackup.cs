using Demon.Functions.Objects;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics.Contracts;
using System.IO;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Text.Json.Serialization.Metadata;
using System.Threading.Tasks;

namespace Demon.Functions.Backups
{
    public class DifferentialBackup : Backuper
    {
        public DifferentialBackup(Core core, string algorithm) : base(core, algorithm)
        {

        }

        //Za osnovu se bere base.Copy, akorát porovná jestli existuje daný záznam v snapshotu 
        public override void CopyMain(string source, string destination, Snapshot snapshot)
        {
            base.CopyMain(source, destination, snapshot);

            //Ověřuje jestli snapshot obsahuje nějaké cesty, pokude ne tak přidá cestu ve formátu Json (jako string) do UpdatedPaths
            if (!snapshot.Paths.Any())
            {
                DirectoryInfo directoryInfo = new DirectoryInfo(source);
                foreach (var dirs in directoryInfo.GetDirectories())
                {
                    UpdatePaths(directoryInfo);

                }
            }
        }

        //Metoda na přidání cest do snapshotu
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

                UpdatedPaths.Add(snapshotJson);
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

                UpdatedPaths.Add(snapshotJson);
            }
        }
    }
}