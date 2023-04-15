using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http.Json;
using System.Text;
using System.Threading.Tasks;

namespace Demon.Models
{
    public class Log
    {
        public int ID { get; set; }

        public int ConfigId { get; set; }

        public DateTime Date { get; set; }

        public bool Errors { get; set; }

        public string? Message { get; set; }

        public Log( int cofnigId)
        {
            ConfigId = cofnigId;
        }        
    }
}
