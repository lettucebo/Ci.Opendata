using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Linq;
using System.ServiceProcess;
using System.Text;
using System.Threading.Tasks;
using Creatidea.Opendata.Library;

namespace Creatidea.Opendata.Schedule
{
    public partial class Service : ServiceBase
    {
        public Service()
        {
            InitializeComponent();
        }

        private readonly List<int> _managedThreadIds = new List<int>();
        private readonly Dictionary<int, BaseSchedule> _schedules = new Dictionary<int, BaseSchedule>();
        private readonly Dictionary<int, System.Threading.Thread> _scheduleThreads = new Dictionary<int, System.Threading.Thread>();
        protected override void OnStart(string[] args)
        {
            Console.WriteLine(@"SERVICE IS START");
            
            Run();
        }

        protected override void OnStop()
        {
            Console.Write("WAIT SERVICE STOP...");

            foreach (var managedThreadId in _managedThreadIds)
            {
                _schedules[managedThreadId].IsStart = false;
            }
            //檢查各執行序的狀態
            while (true)
            {
                var hasAlive = false;

                foreach (var managedThreadId in _managedThreadIds)
                {
                    if (_scheduleThreads[managedThreadId].IsAlive)
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
            this.OnStart(args);
        }

        public void Stop()
        {
            this.OnStop();
        }

        private void Run()
        {
            var taipeiBus = new Library.TaipeiBusSchedule();
            var taipeiBusThread = new System.Threading.Thread(taipeiBus.Start);
            taipeiBusThread.Start();
            _managedThreadIds.Add(taipeiBusThread.ManagedThreadId);
            _schedules.Add(taipeiBusThread.ManagedThreadId, taipeiBus);
            _scheduleThreads.Add(taipeiBusThread.ManagedThreadId, taipeiBusThread);
        }
    }
}
