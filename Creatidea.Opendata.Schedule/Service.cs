using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Linq;
using System.ServiceProcess;
using System.Text;
using System.Threading.Tasks;

namespace Creatidea.Opendata.Schedule
{
    public partial class Service : ServiceBase
    {
        #region 服務

        public Service()
        {
            InitializeComponent();
        }

        protected override void OnStart(string[] args)
        {
            Console.WriteLine("SERVICE IS START");

            Run();
        }

        protected override void OnStop()
        {
            Console.Write("WAIT SERVICE STOP...");

            foreach (var schedule in _schedules.Keys)
            {
                schedule.Stop();
            }

            //檢查各執行序的狀態
            while (true)
            {
                var hasAlive = false;

                foreach (var task in _schedules.Values)
                {
                    if (!task.IsCompleted)
                    {
                        hasAlive = true;
                    }
                }
                if (!hasAlive)
                {
                    break;
                }
            }
        }

        public void Start(string[] args)
        {
            OnStart(args);
        }

        public new void Stop()
        {
            OnStop();
        }
        #endregion

        /// <summary>
        /// 執行序id,排程類別
        /// </summary>
        private readonly Dictionary<BaseSchedule, Task> _schedules = new Dictionary<BaseSchedule, Task>();

        /// <summary>
        /// 執行
        /// </summary>
        private void Run()
        {
            var taipeiBusEstimateTimeSchedule = new Taipei.BusSchedule.EstimateTime { Second = 2 };
            _schedules.Add(taipeiBusEstimateTimeSchedule, Task.Factory.StartNew(taipeiBusEstimateTimeSchedule.Start));
        }
    }
}
