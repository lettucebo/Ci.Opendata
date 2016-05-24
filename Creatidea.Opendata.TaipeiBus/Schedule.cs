using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Creatidea.Opendata.Library;

namespace Creatidea.Opendata.TaipeiBus
{
    public class Schedule : BaseSchedule
    {
        public override void Run()
        {
            var synchronize = new Synchronize();

            synchronize.EstimateTime();

            NextRunTime = DateTime.Now.AddSeconds(20);
        }
    }
}
