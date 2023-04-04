using Demon.Functions.Objects;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
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
        public override void Copy(string source, string destination, Snapshot snapshot)
        {
            base.Copy(source, destination, snapshot);

            //Ověřuje jestli snapshot obsahuje nějaké cesty, pokude ne tak přidá cestu ve formátu Json (jako string) do UpdatedPaths
            if (!snapshot.Paths.Any())
            {
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
}