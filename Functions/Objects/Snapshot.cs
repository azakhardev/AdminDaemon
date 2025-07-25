﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Demon.Functions.Objects
{
    public class Snapshot
    {
        public int ConfigID { get; set; }

        public int PackageVersion { get; set; } = 0;

        public int PackagePartVersion { get; set; } = 0;

        public List<Path> Paths { get; set; }

        //každá složka v snapshotu uložena v téhle podobě:
        //C:\Users\Artem\Desktop\SSSVT predmety\AJ
        //C:\Users\Artem\Desktop\SSSVT predmety\ČJL
        //C:\Users\Artem\Desktop\SSSVT predmety\ČJL\5 knih hebrejskych.png
    }
}
