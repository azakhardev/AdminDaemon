using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Demon.Models
{
    public class Sources
    {
        public int ID { get; set; }

        public int ConfigID { get; set; }

        public string SourcePath { get; set; }

        public string FileName { get; set; }

        public DateTime UpdateDate { get; set; }

    }
}
