using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Chup_8
{
    class Program
    {
        static void Main(string[] args)
        {
            Chup8 chup8 = new Chup8(args[0], 0);
            chup8.Run(1000);
        }
    }
}
