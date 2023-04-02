using Demon.Functions.Backups;
using Demon.Functions.Objects;
using Demon.Models;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Demon
{
    public class Core
    {
        public int ComputerID { get; set; }

        //Ukládá data - rozvrh, cesty odkud a kam pro jednotlivý config - nejspíš zbytečné
        //public Schedules Schedules { get; set; }
        public HttpClient Client { get; set; }
        ////Ukldádá snapshoty k jednotlivým configům pro tento počítač
        public List<Snapshot> Snapshots { get; set; } = new List<Snapshot>();

        //Seznam configů pro daný počítač (ID počítače)
        public List<Configs> Configs { get; set; }

        public List<Logs> Logs { get; set; }

        public List<Backuper> Backupers { get; set; } = new List<Backuper>();

        public int FullRetention { get; set; } = 0;

        public int IncRetention { get; set; } = 0;

        public int DifRetention { get; set; } = 0;

        public Core(HttpClient client)
        {
            this.Client = client;
        }

        public async Task GetDataFromAPI()
        {
            //Načte configy
            string configsResult = await Client.GetStringAsync($"/api/Computers/{ComputerID}/Configs");
            List<Configs> configs = JsonConvert.DeserializeObject<List<Configs>>(configsResult);

            //Uloží configy
            Configs = configs;

            await SetSnapshots();
            await GetSources();
            await GetDestinations();
        }

        //Nejdřív by si měl zjistit jestli je povolen/ má přístup do sítě - pokud ne tak načte data z texťáku uloženého na PC
        public void Saver()
        {
            if (Backupers.Count < 1)
            {
                Backupers.Add(new FullBackup(this, "Full"));
                Backupers.Add(new DifferentialBackup(this, "Differential"));
                Backupers.Add(new IncrementalBackup(this, "Incremental"));
            }

            //Forcyklem prjedeme všechny configy, pokud se ID configu v Coru shoduje s ID configu v Backuperu a je čas pro zálohu tak se spustí záloha > předá se list sourců a pathů pro daný config
            foreach (Backuper backuper in Backupers)
            {
                foreach (var config in this.Configs)
                {
                    foreach (var backuperConfig in backuper.Configs)
                    {
                        if (config.ID == backuperConfig.ID)
                            if (CheckSchedule(config) == true)
                                backuper.ExecBackup(ReturnSourcesToCopy(config), config.Destinations, config);
                    }
                }
            }

            Backupers.Clear();
        }

        //Metoda která zjistí jestli se má zálohovat config který se jí předá jako argument - pro cron
        public bool CheckSchedule(Configs config)
        {
            //    if (config.Schedule == "")
            //        return true;
            return true;
        }

        //Metoda která vrací list cest pro zadaný config ke složkám které by se měly nakopírovat
        public List<Sources> ReturnSourcesToCopy(Configs config)
        {
            List<Sources> paths = new List<Sources>();

            foreach (Sources source in config.Sources)
            {
                foreach (Snapshot snaphsot in Snapshots)
                {
                    foreach (Functions.Objects.Path path in snaphsot.Paths)
                    {
                        if (source.SourcePath != path.FullPath.Replace(path.FileName, "") && source.FileName != path.FileName)
                            paths.Add(source);
                    }
                }
            }

            return paths;
        }

        //Metoda která přiřadí ke configu jeho snapshot
        //**Nepřiřazuje ke Configu, ve snapshotu není uložel název configu ani verze**
        public async Task SetSnapshots()
        {
            foreach (Configs config in Configs)
            {
                string snapshotsResult = await Client.GetStringAsync($"/api/Configs/{config.ID}/{ComputerID}/Snapshot");
                Snapshot snapshot = JsonConvert.DeserializeObject<Snapshot>(snapshotsResult);

                Snapshots.Add(snapshot);
            }
        }

        //Metoda která přiřadí List<Sources> ke správnému configu
        public async Task GetSources()
        {
            foreach (Configs config in Configs)
            {
                string sourcesResult = await Client.GetStringAsync($"/api/Configs/{config.ID}/Sources");
                List<Sources> sources = JsonConvert.DeserializeObject<List<Sources>>(sourcesResult);

                config.Sources = sources;
            }
        }

        //Metoda která přiřadí List<Destinations> ke správnému configu
        public async Task GetDestinations()
        {
            foreach (Configs config in Configs)
            {
                string destinationsResult = await Client.GetStringAsync($"/api/Configs/{config.ID}/Destinations");
                List<Destinations> destinations = JsonConvert.DeserializeObject<List<Destinations>>(destinationsResult);

                config.Destinations = destinations;
            }
        }
    }
}
