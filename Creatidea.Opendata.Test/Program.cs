using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Creatidea.Opendata.Taipei;

namespace Creatidea.Opendata.Test
{
    class Program
    {
        static void Main(string[] args)
        {
            var tp = new Parking.Available();
            var s = tp.Get();
            tp.Save(s);

           //Console.WriteLine(tp.GetLeftUbike("0001"));
            Console.ReadLine();
        }
    }
}
