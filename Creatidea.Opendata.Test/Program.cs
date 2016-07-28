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
            var ISX = Taipei.Bus.GetMapStop(Convert.ToSingle("25.047746"), Convert.ToSingle("121.517050"));


            foreach (var i in ISX)
            {

            }

            //LoadTest();

            Console.WriteLine("Test End");
            Console.ReadLine();
        }

        private static void LoadTest()
        {
            var list = new List<OpenData>
            {
                new Weather.Cwb(),
                new Weather.EpaUv(),
                new Weather.Cwb(),
                new Taipei.Bus.Route(),
                new Taipei.Bus.Stop(),
                new Taipei.Bus(),
                new Taipei.ArtsMuseum(),
                new Taipei.Parking.Description(),
                new Taipei.YouBike.Station(),
                new Taipei.TravelAttractions.Chinese(),
                new Taipei.ShoppingArea.Location(),
                new Taipei.CulturalHeritage(),
                new Taipei.Metro.Entrance(),
                new Taipei.Hotel.Chinese(),
            };

            foreach (var opendata in list)
            {
                Console.WriteLine($"{DateTime.Now:yyyyMMddHHmmss}\t{opendata.GetType()} Start.");
                var sw = new System.Diagnostics.Stopwatch();
                sw.Reset(); //碼表歸零
                sw.Start(); //碼表開始計時
                opendata.DataSave();
                sw.Stop();
                Console.WriteLine($"{DateTime.Now:yyyyMMddHHmmss}\t{opendata.GetType()} End.({sw.Elapsed.TotalMilliseconds})");
            }

        }

    }
}
