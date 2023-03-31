using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Demon.Functions.Objects
{
    public class Path
    {
        public string FileName { get; set; }

        public string FullPath { get; set; }

        public DateTime UpdateTime { get; set; }

        //při vytvoření Path mu předáme FullPath a UpdateTime, ctor si doplní FileName z FullPath

    }
}
