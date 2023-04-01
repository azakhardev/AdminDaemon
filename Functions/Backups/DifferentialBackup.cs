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
        public int Retention { get; set; }

        public DifferentialBackup(Core core, string algorithm) : base(core, algorithm)
        {

        }

        //Za osnovu se bera base.Copy, akorát porovná jestli existuje daný záznam v snapshotu 
        public override void Copy(string source, string destination, Snapshot snapshot)
        {
            //Pokud cesta existuje v snapshotu tak se metoda returne, jinak složu/soubor zkopíruje do destinace
            foreach (var path in snapshot.Paths) 
            {
                if (path.FullPath == source)
                    return;
            }

            base.Copy(source, destination, snapshot);

            //Ovšřuje jestli snapshot existuje, pokude ne tak přidá cestu ve formátu Json (jako string) do UpdatedSnapshot
            if (snapshot == null) 
            {
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
}
