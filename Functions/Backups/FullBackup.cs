using Demon.Functions.Objects;
using Demon.Models;
using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Demon.Functions.Backups
{
    public class FullBackup : Backuper
    {
        public FullBackup(Core core, string algorithm) : base(core, algorithm)
        {

        }
    }
}