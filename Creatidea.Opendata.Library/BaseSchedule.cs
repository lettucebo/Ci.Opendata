using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Creatidea.Opendata.Library
{
    public interface ISchedule
    {

    }

    public abstract class BaseSchedule : ISchedule
    {
        protected string ExecutionPath
        {
            get
            {
                var executionPath = System.AppDomain.CurrentDomain.SetupInformation.ApplicationBase;
                if (!executionPath.EndsWith("\\"))
                {
                    executionPath += "\\";
                }
                return executionPath;
            }
        }
        public DateTime NextRunTime { get; set; }
        public bool IsStart { get; set; }
        public abstract void Run();

        public void Start()
        {
            IsStart = true;

            while (true)
            {
                if (!IsStart)
                {
                    break;
                }

                if (NextRunTime < DateTime.Now)
                {
                    Run();
                }

                GC.Collect();

                System.Threading.Thread.Sleep(1000);
            }
        }


    }
}
