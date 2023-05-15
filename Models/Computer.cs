using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Demon.Models
{
    public class Computer
    {
        public int ID { get; set; }
        public string ComputerName { get; set; }
        public string ComputerStatus { get; set; }
        public string Description { get; set; }
        public DateTime LastBackup { get; set; }
        public string BackupStatus { get; set; }

        public virtual List<string> MacAddresses { get; set; }
    }
}
