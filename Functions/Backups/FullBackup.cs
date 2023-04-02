using Demon.Functions.Objects;
using Demon.Models;
using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Demon.Functions.Backups
{
    public class FullBackup : Backuper
    {
        public FullBackup(Core core, string algorithm) : base(core, algorithm)
        {

        }

        //pokaždé kdy odstraní složku v destination tak do snapshotu přidá její cestu (musí přidávat cestu sourcu)
        public override void DeleteOld(Configs config, Snapshot snapshot, Destinations destination)
        {
            base.DeleteOld(config, snapshot, destination);
        }
    }
}
