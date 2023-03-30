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


        //Pokud snaphot pro PC neexistuje vytvori ho a postne sa server 

        //**nefunguje post**
        //string resultPost = await Core.Client.PostAsync(source);
    }
}
