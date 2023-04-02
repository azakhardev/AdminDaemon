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

        public override void Copy(string source, string destination, Snapshot snapshot)
        {
            base.Copy(source, destination, snapshot);

            //Vždy přidá source ve formátu Json (jako string) do UpdatedPaths
            Objects.Path snapshotJson = new Objects.Path()
            {
                FileName = source.Substring(source.LastIndexOf('\\')),
                FullPath = source,
                UpdateTime = DateTime.Now
            };

            UpdatedPaths += JsonConvert.SerializeObject(snapshotJson);

        }        
    }
}
