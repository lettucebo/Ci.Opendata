using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Creatidea.Opendata.Test
{
    class Program
    {
        static void Main(string[] args)
        {
            Creatidea.Opendata.TaipeiParking tp = new TaipeiParking();
            var s = tp.LeftParkingAvailable();
            tp.SaveLeftParkingAvailable(s);

           //Console.WriteLine(tp.GetLeftUbike("0001"));
            Console.ReadLine();
        }
    }
}
