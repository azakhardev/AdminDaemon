using Demon.Functions.Objects;
using Demon.Models;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
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

        public List<Objects.Path> UpdatedPaths { get; set; } = new List<Objects.Path>();

        public List<Configs> Configs { get; set; } = new List<Configs>();

        public List<Log> Reports { get; set; } = new List<Log>();

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
                    CopyMain(source.SourcePath, destination.DestinationPath, Core.Snapshots.Where(x => x.ConfigID == config.ID).FirstOrDefault());
                }
            }

            //Obnoví v Core snapshot pro daný config pokud nedošlo k chybám
            if (!Reports.Any())
                await UpdateSnapshot(config, destinations);
        }

        //Metoda pro kopírování dat - zkopíruje soubor nebo prázdný soubor který získá z listu sources
        //Odfiltrováno pomocí metody ReturnSourcesToCopy která vrací list cest které nejsou ve snapshotu
        public virtual void CopyMain(string sourcePath, string destination, Snapshot snapshot)
        {
            DirectoryInfo sourceDirectory = new DirectoryInfo(sourcePath);
            DirectoryInfo destinationDirectory = new DirectoryInfo(($"{destination}\\{snapshot.PackageVersion}_{this.Algorithm}_{snapshot.PackagePartVersion}_Pc{Core.ComputerID}_Cf{snapshot.ConfigID}"));

            List<string> matchingPaths = new List<string>();
            foreach (var path in snapshot.Paths)
            {
                foreach (var item in TraverseDirectories(sourcePath, path.FullPath))
                {
                    matchingPaths.Add(item);
                }
            }
            TryToCopy(sourceDirectory, destinationDirectory, snapshot, matchingPaths);
        }

        public List<string> TraverseDirectories(string currentDir, string snapshotPath)
        {
            List<string> matchingPaths = new List<string>();
            foreach (string file in Directory.GetFiles(currentDir))
            {
                if (snapshotPath == file)
                    matchingPaths.Add(file);
            }

            foreach (string subDir in Directory.GetDirectories(currentDir))
            {
                TraverseDirectories(subDir, snapshotPath);
                if (snapshotPath == subDir)
                    matchingPaths.Add(subDir);
            }

            return matchingPaths;
        }

        public void TryToCopy(DirectoryInfo sourceDirectory, DirectoryInfo destinationDirectory, Snapshot snapshot, List<string> matchingPaths)
        {
            bool stop = false;
            if (!destinationDirectory.Exists)
            {
                try
                {
                    destinationDirectory.Create();
                }
                catch (Exception)
                {
                    Log log = new Log(snapshot.ConfigID) { Date = DateTime.Now, Errors = true, Message = $"Couldn't create directory: {destinationDirectory} on computer with ID: {Core.ComputerID}" };
                    Reports.Add(log);
                }
            }

            foreach (FileInfo file in sourceDirectory.GetFiles())
            {
                foreach (var item in matchingPaths)
                {
                    if (item == file.FullName)
                        stop = true;
                }

                if (stop == false)
                    try
                    {
                        string destinationFile = System.IO.Path.Combine(destinationDirectory.FullName, file.Name);
                        file.CopyTo(destinationFile, true);
                    }
                    catch (Exception)
                    {
                        Log log = new Log(snapshot.ConfigID) { Date = DateTime.Now, Errors = true, Message = $"Couldn't copy file: {file} on computer with ID: {Core.ComputerID}" };
                        Reports.Add(log);
                    }

                stop = false;
            }

            foreach (DirectoryInfo subDirectory in sourceDirectory.GetDirectories())
            {
                foreach (var item in matchingPaths)
                {
                    if (item == subDirectory.FullName)
                        stop = true;
                }

                if (stop == false)
                    try
                    {
                        string destinationSubDirectory = System.IO.Path.Combine(destinationDirectory.FullName, subDirectory.Name);
                        Copy(subDirectory.FullName, destinationSubDirectory);
                    }
                    catch (Exception)
                    {
                        Log log = new Log(snapshot.ConfigID) { Date = DateTime.Now, Errors = true, Message = $"Couldn't copy directory: {subDirectory.FullName} on computer with ID: {Core.ComputerID}" };
                        Reports.Add(log);
                    }

                stop = false;
            }
        }

        public virtual void Copy(string source, string destination)
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

                File.Copy(file.FullName, destinationFile);
            }

            foreach (DirectoryInfo subDirectory in sourceDirectory.GetDirectories())
            {
                string destinationSubDirectory = System.IO.Path.Combine(destinationDirectory.FullName, subDirectory.Name);
                Copy(subDirectory.FullName, destinationSubDirectory);
            }
        }

        //Maže složky z destinace které jsou staré (na podobě Queue - FiFo)
        //DeleteOld - virtual => každá třída si z toho bude ukládat snapshot pro retenci (kromě inceremntal??)
        public virtual void DeleteOld(Configs config, Snapshot snapshot, Destinations destination)
        {
            int packageVersionToDelete = snapshot.PackageVersion - config.MaxPackageAmount;
            for (int i = 1; i <= config.MaxPackageSize; i++)
            {
                DirectoryInfo destinationDirectory = new DirectoryInfo($"{destination.DestinationPath}\\{packageVersionToDelete}_{config.Algorithm}_{i}_Pc{Core.ComputerID}_Cf{config.ID}");
                try
                {
                    destinationDirectory.Delete(true);
                }
                catch (Exception ex)
                {
                    Log log = new Log(snapshot.ConfigID) { Date = DateTime.Now, Errors = true, Message = $"Couldn't delete old directory(retention): {destinationDirectory.FullName} on computer with ID: {Core.ComputerID}, with error mesage: {ex}" };
                    Reports.Add(log);
                }
            }
        }

        //Obnoví snapshot - změníme verze a cesty v snapshotu a pak je putneme
        public async Task UpdateSnapshot(Configs config, List<Destinations> destinations)
        {
            //Nastavíme referenci updatedSnap na příslušný snapshot v Core
            Snapshot updatedSnap = Core.Snapshots.Where(x => x.ConfigID == config.ID).FirstOrDefault();

            //Nastavíme updatedSnapu verzi balíčku, pokud přesahuje maximální množství balíčků tak se odstraní nejstarší balíček
            if (updatedSnap.PackageVersion > config.MaxPackageAmount)
            {
                if (updatedSnap.PackagePartVersion == 1)
                    foreach (Destinations destination in destinations)
                    {
                        DeleteOld(config, updatedSnap, destination);
                    }

                if (updatedSnap.PackagePartVersion >= config.MaxPackageSize)
                {
                    updatedSnap.PackageVersion++;
                    updatedSnap.PackagePartVersion = 1;

                    //Odstraníme cesty ze snapshotu(kvůli retenci)
                    UpdatedPaths.Clear();
                    updatedSnap.Paths.Clear();
                }
                else
                {
                    updatedSnap.PackagePartVersion++;
                }
            }
            else if (updatedSnap.PackagePartVersion >= config.MaxPackageSize)
            {
                updatedSnap.PackagePartVersion = 1;

                updatedSnap.PackageVersion++;

                //Odstraníme cesty ze snapshotu(kvůli retenci)
                UpdatedPaths.Clear();
                updatedSnap.Paths.Clear();
            }
            else
            {
                updatedSnap.PackagePartVersion++;
            }


            //Pokud cesta není prázdná a algoritmus není Full tak updatne cesty
            if (UpdatedPaths.Count != 0 && Algorithm != "Full")
            {
                updatedSnap.Paths = UpdatedPaths;
            }

            //Putne (obnoví) updatedSnap na server po každé záloze            
            Snapshot snapshot = new Snapshot()
            {
                ConfigID = config.ID,
                PackagePartVersion = updatedSnap.PackagePartVersion,
                PackageVersion = updatedSnap.PackageVersion,
                Paths = updatedSnap.Paths
            };

            SnapshotPut snap = new SnapshotPut(snapshot, Core.ComputerID, snapshot.ConfigID);
            var result = await Client.PutAsJsonAsync("api/Computers/Snapshot", snap);
        }
    }
}