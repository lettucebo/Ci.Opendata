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
            //var o = new List<OpenData>
            //{
            //    new Weather.Cwb(),
            //    new Weather.EpaUv(),
            //    new Weather.Cwb()
            //};
            //Parallel.ForEach(o, (openData, loopState) =>
            //{
            //    OpenData.GetNow(openData, 10);
            //    OpenData.GetNow(openData, 30);
            //    OpenData.GetNow(openData, 20);
            //});
            //var s = Weather.Cwb.Get;

            //Console.WriteLine(tp.GetLeftUbike("0001"));


            var DXX = new Opendata.Taipei.Bus.Route();
            DXX.DataSave();

            //var s = Opendata.Taipei.Parking.Description.Get((float)25.034006, (float)121.543749);


            Console.WriteLine("Test End");
            Console.ReadLine();
        }

    }
}
