using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Demon.Functions.Objects
{
    public class SnapshotPut
    {
        public int ComputerID { get; set; }

        public int ConfigID { get; set; }

        public string Snapshot { get; set; }

        public SnapshotPut(Snapshot snap, int computerId, int configId)
        {
            ComputerID = computerId;
            ConfigID = configId;
            Snapshot = JsonConvert.SerializeObject(snap);
        }
    }
}
