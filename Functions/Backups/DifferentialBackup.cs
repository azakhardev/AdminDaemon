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

        //**místo 1 bude ID správného configu**
        //Pokud snaphot pro PC neexistuje vytvori ho a postne sa server 

        public override void Copy(string source, string destination, bool snapshotExists)
        {
            Objects.Path snapshotJson = new Objects.Path()
            {
                FileName = source.Substring(source.LastIndexOf('\\')),
                FullPath = source,
                UpdateTime = DateTime.Now
            };

            base.Copy(source, destination, snapshotExists);

            if (snapshotExists == false)
                UpdatedSnapshot += JsonConvert.SerializeObject(snapshotJson);
        }
    }
}
