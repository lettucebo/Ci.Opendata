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
        private bool _isRun;
        private bool _isStart;
        private string _path;
        private System.Threading.Thread _thread = null;

        public Service()
        {
            InitializeComponent();
        }

        protected override void OnStart(string[] args)
        {
            _path = System.IO.Path.GetTempPath();
            if (!_path.EndsWith("\\"))
            {
                _path += "\\";
            }

            Console.WriteLine(@"SERVICE IS START");

            _isRun = true;
            _isStart = true;

            _thread = new System.Threading.Thread(Run);
            _thread.Start();
        }

        protected override void OnStop()
        {
            _isRun = false;

            Console.Write("WAIT SERVICE STOP...");
            while (_isStart)
            {
                System.Threading.Thread.Sleep(1000);
                Console.Write(".");
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
            try
            {
                while (_isRun)
                {
                   

                    Console.ReadLine();
                }
            }
            catch (Exception e)
            {

            }

            _isStart = false;
        }

        private void Bus()
        {
            //http://data.taipei/bus/EstimateTime
            System.Threading.Thread thread = null;
        }

    }
}
