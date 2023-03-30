using Demon.Models;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http.Json;
using System.Net.NetworkInformation;
using System.Text;
using System.Threading.Tasks;

namespace Demon
{
    public class Authorization
    {
        private int ID { get; set; }
        public void Authorize(HttpClient client)
        {
            string idFile = (Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData) + "\\computerID.txt").ToString();
            DirectoryInfo dir = new DirectoryInfo(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData));

            if (File.Exists(idFile))
            {
                IdReader(idFile);
            }
            else
            {
                UploadPC(client);

                File.WriteAllText(idFile, ID.ToString());
                
            }
        }

        public async Task UploadPC(HttpClient client)
        {
            Computer pc = new Computer()
            {
                ComputerName = Environment.MachineName,
                Description = "New computer",
                ComputerStatus = "unauthorized",
                MacAddresses = GetComputersMacAddresses()
            };

            var item = await client.PostAsJsonAsync("api/computers", pc);
            var computerID = await item.Content.ReadAsStringAsync();

            ID = Convert.ToInt32(computerID);
        }

        public void IdReader(string idFile)
        {
            StreamReader sr = new StreamReader(idFile);
            ID = sr.Read();
            sr.Close();
        }

        public int ReturnId()
        {
            return ID;
        }


        //Metoda která zjistí všechny MacAdresy počítače
        public List<string> GetComputersMacAddresses()
        {
            List<string> macAddresses = new List<string>();
            NetworkInterface[] nics = NetworkInterface.GetAllNetworkInterfaces();
            foreach (NetworkInterface adapter in nics)
            {
                PhysicalAddress address = adapter.GetPhysicalAddress();
                byte[] bytes = address.GetAddressBytes();
                string macAddress = "";
                for (int i = 0; i < bytes.Length; i++)
                {
                    macAddress += bytes[i].ToString("X2");
                    if (i != bytes.Length - 1)
                    {
                        macAddress += "-";
                    }
                }
                macAddresses.Add(macAddress);
            }

            return macAddresses;
        }
    }
}
