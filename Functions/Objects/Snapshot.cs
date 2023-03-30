using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Demon.Functions.Objects
{
    public class Snapshot
    {
        public int ConfigID { get; set; }

        public int Version { get; set; } = 0;

        public List<Path> Paths { get; set; }

        //Konstruktor zavolá metodu která naplní snapshot daty 
        public Snapshot(int configID)
        {
            ConfigID = configID;
        }

        ////naplní pathy jednotlivými cestami ze snapshotu, názvem souboru a posledním datumem změny
        //public void FillData(string snaphsotString)
        //{           
        //    this.Paths.Add(JsonConvert.DeserializeObject<Path>(snaphsotString));           
        //    Version++;
        //}

        //každá složka v snapshotu uložena v téhle podobě:
        //C:\Users\Artem\Desktop\SSSVT predmety\AJ
        //C:\Users\Artem\Desktop\SSSVT predmety\ČJL
        //C:\Users\Artem\Desktop\SSSVT predmety\ČJL\5 knih hebrejskych.png
    }
}
