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
        public int Retention { get; set; }

        public IncrementalBackup(Core core, string algorithm) : base(core, algorithm)
        {

        }

        public override void Copy(string source, string destination, Snapshot snapshot)
        {
            //Pokud cesta existuje v snapshotu tak se metoda returne, jinak složku/soubor zkopíruje do destinace
            foreach (var path in snapshot.Paths)
            {
                if (path.FullPath == source)
                    return;
            }

            base.Copy(source, destination, snapshot);

            //Vždy přidá source ve formátu Json (jako string) do UpdatedSnapshot
            Objects.Path snapshotJson = new Objects.Path()
            {
                FileName = source.Substring(source.LastIndexOf('\\')),
                FullPath = source,
                UpdateTime = DateTime.Now
            };

            UpdatedSnapshot += JsonConvert.SerializeObject(snapshotJson);

        }        
    }
}
