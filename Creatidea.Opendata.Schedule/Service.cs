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

        /// <summary>
        /// 初始化
        /// </summary>
        public Service()
        {
            InitializeComponent();

            //加入需要排程的服務
            _scheduleList.Add(new Taipei.BusSchedule.EstimateTime { ScheduleType = ScheduleType.None, Second = 5 });
        }

        protected override void OnStart(string[] args)
        {
            Console.WriteLine(@"SERVICE IS START");

            Run();
        }

        protected override void OnStop()
        {
            Console.Write(@"WAIT SERVICE STOP...");

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
        private readonly List<OpenDataSchedule> _scheduleList = new List<OpenDataSchedule>();
        /// <summary>
        /// 執行序id,排程類別
        /// </summary>
        private readonly Dictionary<OpenDataSchedule, Task> _schedules = new Dictionary<OpenDataSchedule, Task>();

        /// <summary>
        /// 執行
        /// </summary>
        private void Run()
        {
            foreach (var schedule in _scheduleList)
            {
                _schedules.Add(schedule, Task.Factory.StartNew(schedule.Start));
            }
        }

        /// <summary>
        /// 執行一次
        /// </summary>
        public void RunOnce()
        {
            foreach (var schedule in _scheduleList)
            {
                _schedules.Add(schedule, Task.Factory.StartNew(schedule.Run));
            }
        }
    }
}
