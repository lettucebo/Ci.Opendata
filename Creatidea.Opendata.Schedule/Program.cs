using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceProcess;
using System.Text;
using System.Threading.Tasks;

namespace Creatidea.Opendata.Schedule
{
    static class Program
    {
        /// <summary>
        /// 應用程式的主要進入點。
        /// </summary>
        static void Main()
        {
            if (Environment.UserInteractive)
            {
                var service = new Service();

                service.Start(null);

                Console.WriteLine("SERVICE IS START, PRESS ENTER TO STOP SERVICE.");
                // 必須要透過 Console.ReadLine(); 先停止程式執行
                // 因為 Windows Service 大多是利用多 Thread 或 Timer 執行長時間的工作
                // 所以雖然主執行緒停止執行了，但服務中的執行緒已經在運行了!
                Console.ReadLine();

                service.Stop();

                Console.WriteLine("SERVICE IS STOP.");

                System.Threading.Thread.Sleep(5000);
            }
            else
            {
                ServiceBase[] ServicesToRun;
                ServicesToRun = new ServiceBase[]
                {
                    new Service()
                };
                ServiceBase.Run(ServicesToRun);
            }
        }
    }
}
