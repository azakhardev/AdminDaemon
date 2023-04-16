using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Demon.Functions.Objects
{
    public class LogToPost
    {
        public int ID { get; set; }

        public int ComputersConfigsId { get; set; }

        public DateTime Date { get; set; }

        public bool Errors { get; set; }

        public string Message { get; set; }
    }
}
