using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Creatidea.Opendata;

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
            var list = new List<OpenData>
            {
                //new Taipei.Bus.Route(),
                //new Taipei.Bus.Stop(),
                //new Taipei.Bus(),
                //new Taipei.ArtsMuseum(),
                //new Taipei.Parking.Description(),
                //new Taipei.YouBike.Station(),
                //new Taipei.TravelAttractions.Chinese(),
                //new Taipei.ShoppingArea.Location(),
                //new Taipei.CulturalHeritage(),
                //new Taipei.Metro.Entrance(),
                //new Taipei.Hotel.Chinese(),
            };

            foreach (var opendata in list)
            {
                opendata.DataSave();
            }

            var stations = Opendata.Taipei.Bus.Stop.Get(Convert.ToSingle("25.0477498"), Convert.ToSingle("121.5170497"), 2);


            var routes = Opendata.Taipei.Bus.Route.Get(stations.Select(x => x.Id).Distinct().ToArray());

            foreach (var i in routes)
            {
                Console.WriteLine(i.Name + "(" + i.PathAttributeName + ")");
            }

            //var s = Opendata.Taipei.Parking.Description.Get((float)25.034006, (float)121.543749);


            Console.WriteLine("Test End");
            Console.ReadLine();
        }

    }
}
