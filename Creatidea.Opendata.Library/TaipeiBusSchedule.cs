using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Creatidea.Opendata.Library
{
    public class TaipeiBusSchedule : BaseSchedule
    {
        public override void Run()
        {
            Console.WriteLine("{0}\tTaipeiBusSchedule Start.", DateTime.Now);

            var synchronize = new TaipeiBus();

            var jObjectEstimateTime = synchronize.EstimateTime();

            synchronize.SaveEstimateTime(jObjectEstimateTime, ExecutionPath);

            Console.WriteLine("{0}\tTaipeiBusSchedule End.", DateTime.Now);
            NextRunTime = DateTime.Now.AddSeconds(20);
        }
    }
}
