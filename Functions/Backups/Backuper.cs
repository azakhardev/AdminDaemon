using Demon.Functions.Objects;
using Demon.Models;
using Newtonsoft.Json;
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
        public Core Core { get; set; }

        public HttpClient Client { get; set; }

        public string Algorithm { get; set; }

        public string UpdatedPaths { get; set; } = "";

        public List<Configs> Configs { get; set; } = new List<Configs>();

        public List<string> DestinationsCopied { get; set; } = new List<string>();

        public Backuper(Core core, string algorithm)
        {
            Core = core;
            Algorithm = algorithm;
            Client = Core.Client;

            //Projedeme všechny configy v core, přidá do this.Configs configy u kterých se shoduje algoritmus s this.Algorithm
            foreach (Configs config in Core.Configs)
            {
                if (config.Algorithm == algorithm)
                    Configs.Add(config);
            }

            ////Projedeme všechny snapshoty v core, přidá do this.Snapshots snapshoty u kterých se shoduje typ algoritmu a configID s this.Configs
            //foreach (Snapshot snap in Core.Snapshots)
            //{
            //    foreach (Configs config in this.Configs)
            //    {
            //        if (snap.ConfigID == config.ID)
            //            Snapshots.Add(snap);
            //    }
            //}
        }

        //Core předá argumenty podle ID Configs/Schedules které se schodují s ID v daném backupru a pak proběhne zálohování dat podle typu zálohování
        //Projede všechny sourcy, pro každý source projede každou destinaci a pro každou destinaci spustí Copy() 
        public virtual async Task ExecBackup(List<Sources> sources, List<Destinations> destinations, Configs config)
        {
            //if (Client.GetStringAsync($"api/{config.ID}/{Core.ComputerID}/Snapshot") == null)
            //    snapshotExists = false;

            foreach (Sources source in sources)
            {
                foreach (Destinations destination in destinations)
                {
                    Copy(source.SourcePath, destination.DestinationPath, Core.Snapshots.Where(x => x.ConfigID == config.ID).FirstOrDefault());
                }
            }

            //Obnoví v Core snapshot pro daný config
            Snapshot updatedSnap = Core.Snapshots.Where(x => x.ConfigID == config.ID).FirstOrDefault();
            if (UpdatedPaths != "")
            {
                updatedSnap.ConfigID = config.ID;
                updatedSnap.Version++;
                updatedSnap.Paths.Add(JsonConvert.DeserializeObject<Objects.Path>(UpdatedPaths));
            }
            //Putne (obnoví) updatedSnap na server po kažé záloze
            await Client.PutAsJsonAsync($"api/Computers/Snapshot/{Core.ComputerID}/{config.ID}", updatedSnap);
        }

        //Metoda pro kopírování dat - zkopíruje soubor nebo prázdný soubor který získá z listu sources
        //Odfiltrováno pomocí metody ReturnSourcesToCopy která vrací list cest které nejsou ve snapshotu
        public virtual void Copy(string source, string destination, Snapshot snapshot)
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
                Copy(subDirectory.FullName, destinationSubDirectory, snapshot);
            }
        }

        //Maže složky z destinace které jsou staré (na podobě Queue - FiFo)
        public void Deleter(Configs config, Snapshot snapshot, Destinations destination)
        {
            if (snapshot.Version > config.MaxPackageAmount)
            {
                DirectoryInfo destinationDirectory = new DirectoryInfo(DestinationsCopied.First());
                destinationDirectory.Delete();
                DestinationsCopied.RemoveAt(DestinationsCopied.IndexOf(DestinationsCopied.First()));
            }
        }


    }
}
