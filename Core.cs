using Demon.Functions.Backups;
using Demon.Functions.Objects;
using Demon.Models;
using Newtonsoft.Json;
using System.Net.Http.Json;

namespace Demon
{
    public class Core
    {
        public int ComputerID { get; set; }

        public HttpClient Client { get; set; }

        public List<Snapshot> Snapshots { get; set; } = new List<Snapshot>();

        public List<Configs> Configs { get; set; }

        public List<Log> Logs { get; set; } = new List<Log>();

        public List<Backuper> Backupers { get; set; } = new List<Backuper>();

        public DateTime LastBackup { get; set; }

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

        //Nejdřív by si měl zjistit jestli je povolen/má přístup do sítě - pokud ne tak načte data z texťáku uloženého na PC
        public async void Saver()
        {
            await Client.PutAsJsonAsync($"api/Computers/{this.ComputerID}", new Computer {BackupStatus = "Backup in progress" });

            Backupers.Add(new FullBackup(this, "Full"));
            Backupers.Add(new DifferentialBackup(this, "Differential"));
            Backupers.Add(new IncrementalBackup(this, "Incremental"));

            //Forcyklem prjedeme všechny configy, pokud se ID configu v Coru shoduje s ID configu v Backuperu a je čas pro zálohu tak se spustí záloha > předá se list sourců a pathů pro daný config
            foreach (Backuper backuper in Backupers)
            {
                foreach (var config in this.Configs)
                {
                    int noErrors = 0;
                    foreach (var backuperConfig in backuper.Configs)
                    {
                        if (config.ID == backuperConfig.ID)
                            if (CheckSchedule(config) == true)
                                backuper.ExecBackup(ReturnSourcesToCopy(config), config.Destinations, config);
                    }

                    if (backuper.Reports.Count == noErrors && config.Algorithm == backuper.Algorithm)
                    {
                        Log log = new Log(config.ID) { Date = DateTime.Now, Errors = false, Message = $"Backup on computer: {ComputerID} for config: {config.ID} completed succesfully" };
                        Logs.Add(log);
                    }
                }                

                //Přidá všechny reporty z backuperů do Core
                foreach (Log report in backuper.Reports)
                {
                    this.Logs.Add(report);
                }
            }

            Backupers.Clear();

            await Client.PutAsJsonAsync($"api/Computers/{this.ComputerID}", new Computer { LastBackup = DateTime.Now, BackupStatus = "Backup ended" });
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
            List<Sources> sources = new List<Sources>();

            foreach (Sources source in config.Sources)
            {
                foreach (Snapshot snaphsot in Snapshots)
                {
                    bool pathExists = false;

                    foreach (var path in snaphsot.Paths)
                    {
                        if (source.SourcePath == path.FullPath)
                        {
                            pathExists = true;
                            break;
                        }
                    }

                    if (pathExists == false)
                        sources.Add(source);
                }
            }

            return sources;
        }

        //Metoda která přiřadí ke configu jeho snapshot
        public async Task SetSnapshots()
        {
            foreach (Configs config in Configs)
            {
                string snapshotsResult = await Client.GetStringAsync($"/api/Configs/{config.ID}/{ComputerID}/Snapshot");
                Snapshot snapshot = JsonConvert.DeserializeObject<Snapshot>(snapshotsResult);

                if (snapshot == null)
                {
                    Functions.Objects.Path path = new Functions.Objects.Path() { FileName = "", FullPath = "", UpdateTime = DateTime.Now };
                    snapshot = new Snapshot()
                    {
                        ConfigID = config.ID,
                        PackageVersion = 1,
                        PackagePartVersion = 1,
                        Paths = new List<Functions.Objects.Path>()
                    };
                }

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

        //Postuje všechny reporty z Core na server
        public async Task PostReports()
        {
            foreach (Log log in Logs)
            {
                LogToPost report = new LogToPost() {Date = log.Date, Errors = log.Errors, Message = log.Message };
                var pcCfId = await Client.GetStringAsync($"api/Logs/{this.ComputerID}/{log.ConfigId}");

                report.ComputersConfigsId = JsonConvert.DeserializeObject<int>(pcCfId);
                                
                await Client.PostAsJsonAsync($"/api/Logs", report);
            }           
        }

        public async Task<bool> CheckStatus() 
        {
            var request = await Client.GetStringAsync($"api/Computers/{this.ComputerID}");
            Computer computer = JsonConvert.DeserializeObject<Computer>(request);

            this.LastBackup = computer.LastBackup;

            if (computer.ComputerStatus.ToLower() == "blocked")
                return false;            

            return true;
        }
    }
}
