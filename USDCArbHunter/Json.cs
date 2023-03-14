using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace USDCArbHunter
{
    internal class Arbs
    {
        public int chain { get; set; }
        public string[] coins { get; set; }
        public string[] addresses { get; set; }
        public double profit { get; set; }
    }
}
