using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http.Json;
using System.Text;
using System.Threading.Tasks;

namespace Demon.Models
{
    public class Report
    {
        public int ID { get; set; }

        public int ComputersConfigsID { get; set; }

        public DateTime Date { get; set; }

        public bool Errors { get; set; }

        public string Message { get; set; }

        public Report(int computerId, int configId, HttpClient client)
        {
            var ResultId = GetCpsCfgsId(computerId, configId, client);
            ComputersConfigsID = Convert.ToInt32(ResultId);
        }

        public async Task<int> GetCpsCfgsId(int computerId, int configId, HttpClient client) 
        {
            var computersConfigsResult = await client.GetStringAsync($"api/Logs/{computerId}/{configId}");
            return Convert.ToInt32(computersConfigsResult);
        }
    }
}
