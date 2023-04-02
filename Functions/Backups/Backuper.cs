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

        public List<Report> Reports { get; set; } = new List<Report>();

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
        }
        
        //Core předá argumenty podle ID Configs/Schedules které se schodují s ID v daném backupru a pak proběhne zálohování dat podle typu zálohování
        //Projede všechny sourcy, pro každý source projede každou destinaci a pro každou destinaci spustí Copy() 
        public virtual async Task ExecBackup(List<Sources> sources, List<Destinations> destinations, Configs config)
        {
            foreach (Sources source in sources)
            {
                foreach (Destinations destination in destinations)
                {                    
                    Copy(source.SourcePath, destination.DestinationPath, Core.Snapshots.Where(x => x.ConfigID == config.ID).FirstOrDefault());
                }
            }

            //Obnoví v Core snapshot pro daný config
            await UpdateSnapshot(config, destinations);
        }

        //Metoda pro kopírování dat - zkopíruje soubor nebo prázdný soubor který získá z listu sources
        //Odfiltrováno pomocí metody ReturnSourcesToCopy která vrací list cest které nejsou ve snapshotu
        public virtual void Copy(string source, string destination, Snapshot snapshot)
        {
            DirectoryInfo sourceDirectory = new DirectoryInfo(source);
            DirectoryInfo destinationDirectory = new DirectoryInfo(destination);

            try
            {
                if (!destinationDirectory.Exists)
                {
                    destinationDirectory.Create();
                }
            }
            catch (Exception)
            {
                Report log = new Report() { Date = DateTime.Now, Errors = true, Message = $"Couldn't create directory: {destination} on computer with ID: {Core.ComputerID}" };
                Reports.Add(log);
            }

            foreach (FileInfo file in sourceDirectory.GetFiles())
            {
                try
                {
                    string destinationFile = System.IO.Path.Combine($"{destinationDirectory.FullName}\\{snapshot.PackageVersion}_{this.Algorithm}_{snapshot.PackagePartVersion}", file.Name);
                    file.CopyTo(destinationFile, true);
                }
                catch (Exception)
                {
                    Report log = new Report() { Date = DateTime.Now, Errors = true, Message = $"Couldn't copy file: {file} on computer with ID: {Core.ComputerID}" };
                    Reports.Add(log);
                }
            }

            foreach (DirectoryInfo subDirectory in sourceDirectory.GetDirectories())
            {
                try
                {
                    string destinationSubDirectory = System.IO.Path.Combine($"{destinationDirectory.FullName}\\{snapshot.PackageVersion}_{this.Algorithm}_{snapshot.PackagePartVersion}", subDirectory.Name);
                    Copy(subDirectory.FullName, destinationSubDirectory, snapshot);
                }
                catch (Exception)
                {
                    Report log = new Report() { Date = DateTime.Now, Errors = true, Message = $"Couldn't copy directory: {subDirectory.FullName} on computer with ID: {Core.ComputerID}" };
                    Reports.Add(log);
                }
            }
        }

        //Maže složky z destinace které jsou staré (na podobě Queue - FiFo)
        //DeleteOld - virtual => každá třída si z toho bude ukládat snapshot pro retenci (kromě inceremntal??)
        public virtual void DeleteOld(Configs config, Snapshot snapshot, Destinations destination)
        {
            int packageVersionToDelete = snapshot.PackageVersion % config.MaxPackageAmount + (snapshot.PackageVersion - (snapshot.PackageVersion % config.MaxPackageAmount));
            for (int i = 1; i <= config.MaxPackageSize; i++)
            {
                DirectoryInfo destinationDirectory = new DirectoryInfo($"{destination.DestinationPath}\\{packageVersionToDelete}_{config.Algorithm}_{i}");
                destinationDirectory.Delete();
            }
        }

        //Obnoví snapshot - změníme verze a cesty v snapshotu a pak je putneme
        public async Task UpdateSnapshot(Configs config, List<Destinations> destinations)
        {
            //Nastavíme referenci updatedSnap na příslušný snapshot v Core
            Snapshot updatedSnap = Core.Snapshots.Where(x => x.ConfigID == config.ID).FirstOrDefault();

            //Nastavíme updatedSnapu verzi balíčku, pokud přesahuje maximální množství balíčků tak se odstraní nejstarší balíček
            if (++updatedSnap.PackagePartVersion > config.MaxPackageSize)
            {
                if (++updatedSnap.PackageVersion > config.MaxPackageAmount)
                {
                    foreach (Destinations destination in destinations)
                    {
                        DeleteOld(config, updatedSnap, destination);
                    }
                    updatedSnap.PackageVersion++;
                }
                else
                {
                    updatedSnap.PackageVersion++;
                }

                updatedSnap.PackagePartVersion = 1;
            }
            else
            {
                updatedSnap.PackagePartVersion++;
            }
                        
            //Pokud cesta není prázdná a algoritmus není Full tak updatne cesty
            if (UpdatedPaths != "" && Algorithm != "Full")
            {
                updatedSnap.Paths.Add(JsonConvert.DeserializeObject<Objects.Path>(UpdatedPaths));
            }

            //Putne (obnoví) updatedSnap na server po kažé záloze
            await Client.PutAsJsonAsync($"api/Computers/Snapshot/{Core.ComputerID}/{config.ID}", updatedSnap);
        }
    }
}