using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Demon.Models
{
    public class Configs
    {
        public int ID { get; set; }

        public string ConfigName { get; set; }

        public DateTime CreationDate { get; set; }

        public string Algorithm { get; set; }

        public int MaxPackageAmount { get; set; }

        public int MaxPackageSize { get; set; }

        public string Schedule { get; set; }

        public bool Zip { get; set; }

        public List<Sources> Sources { get; set; } // přidáme vlastnost Source

        public List<Destinations> Destinations { get; set; } // přidáme vlastnost Destination

        //public List <Logs> Logs { get; set; } // přidáme vlasnost Logs

        //public Snapshot Snapshot { get; set; }
    }
}
