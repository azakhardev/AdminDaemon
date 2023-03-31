using Demon.Functions.Objects;
using Demon.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http.Json;
using System.Text;
using System.Threading.Tasks;
using System.Xml;

namespace Demon.Functions.Backups
{
    public abstract class Backuper
    {
        //public Snapshot Snapshots { get; set; }
        public Core Core { get; set; }

        public HttpClient Client { get; set; }

        public string Algorithm { get; set; }

        public string UpdatedSnapshot { get; set; } = "";

        public List<Configs> Configs { get; set; } = new List<Configs>();

        public List<Snapshot> Snapshots { get; set; } = new List<Snapshot>();

        public Backuper(Core core, string algorithm)
        {
            Core = core;
            Algorithm = algorithm;
            Client = Core.Client;

            //projedeme všechny configy v core, přidá do this.Configs configy u kterých se shoduje algoritmus s this.Algorithm
            foreach (Configs config in Core.Configs)
            {
                if (config.Algorithm == algorithm)
                    Configs.Add(config);
            }

            //Projedeme všechny snapshoty v core, přidá do this.Snapshots snapshoty u kterých se shoduje typ algoritmu a configID s this.Configs
            foreach (Snapshot snap in Core.Snapshots)
            {
                foreach (Configs config in this.Configs)
                {
                    if (snap.ConfigID == config.ID)
                        Snapshots.Add(snap);
                }
            }
        }

        //Core předá argumenty podle ID Configs/Schedules které se schodují s ID v daném backupru a pak proběhne zálohování dat podle typu zálohování
        //Projede všechny sourcy, pro každý source projede každou destinaci a pro každou destinaci spustí Copy() 
        public virtual async Task ExecBackup(List<Sources> sources, List<Destinations> destinations, Configs config)
        {
            bool snapshotExists = true;
            if (Client.GetStringAsync($"api/{config.ID}/{Core.ComputerID}/Snapshot") == null)
                snapshotExists = false;

            foreach (Sources source in sources)
            {
                foreach (Destinations destination in destinations)
                {
                    Copy(source.SourcePath, destination.DestinationPath,snapshotExists);
                }
            }

            if (UpdatedSnapshot != "")
               await Client.PostAsJsonAsync($"api/Computers/Snapshot/{Core.ComputerID}/{config.ID}",UpdatedSnapshot);

        }

        //**rozbité kopírování**
        //metoda pro kopírování dat - zkopíruje soubor nebo prázdný soubor který získá z listu sources
        //Odfiltrováno pomocí metody ReturnSourcesToCopy která vrací list cest které je potřeba zkopírovat
        public virtual void Copy(string source, string destination, bool snapshotExists)
        {
            DirectoryInfo sourceDirectory = new DirectoryInfo(source);
            DirectoryInfo destinationDirectory = new DirectoryInfo(destination);

            if (!destinationDirectory.Exists)
            {
                destinationDirectory.Create();
            }

            foreach (FileInfo file in sourceDirectory.GetFiles())
            {
                string destinationFile = System.IO.Path.Combine(destinationDirectory.FullName, file.Name);
                file.CopyTo(destinationFile, true);
            }

            foreach (DirectoryInfo subDirectory in sourceDirectory.GetDirectories())
            {
                string destinationSubDirectory = System.IO.Path.Combine(destinationDirectory.FullName, subDirectory.Name);
                Copy(subDirectory.FullName, destinationSubDirectory, snapshotExists);
            }
        }
    }
}
