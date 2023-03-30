using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
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

        //**nefunguje post**
        //string snapResult = await Core.Client.PostAsync();

    }
}
