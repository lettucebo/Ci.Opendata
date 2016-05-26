using System;

namespace Creatidea.Opendata
{
    /// <summary>
    /// 即時資訊排程
    /// </summary>
    /// <seealso cref="BaseSchedule" />
    public class TaipeiBusScheduleEstimateTime : BaseSchedule
    {
        protected override bool RunForStart()
        {
            return true;
        }

        protected override void Run()
        {
            System.Diagnostics.Trace.WriteLine(string.Format("{0:yyyyMMddHHmmss}\tTaipeiBusScheduleEstimateTime Start.", DateTime.Now));

            var sw = new System.Diagnostics.Stopwatch();
            sw.Reset();//碼表歸零
            sw.Start();//碼表開始計時

            var synchronize = new TaipeiBus();

            var jObjectEstimateTime = synchronize.EstimateTime();

            synchronize.SaveEstimateTime(jObjectEstimateTime);

            sw.Stop();

            System.Diagnostics.Trace.WriteLine(string.Format("{0:yyyyMMddHHmmss}\tTaipeiBusScheduleEstimateTime End.({1})", DateTime.Now, sw.Elapsed.TotalMilliseconds));
        }
    }

    /// <summary>
    /// 站牌資訊排程
    /// </summary>
    /// <seealso cref="BaseSchedule" />
    public class TaipeiBusScheduleStopSign : BaseSchedule
    {
        protected override bool RunForStart()
        {
            return false;
        }

        protected override void Run()
        {
            System.Diagnostics.Trace.WriteLine(string.Format("{0:yyyyMMddHHmmss}\tTaipeiBusScheduleStopSign Start.", DateTime.Now));
            var sw = new System.Diagnostics.Stopwatch();
            sw.Reset();//碼表歸零
            sw.Start();//碼表開始計時

            sw.Stop();
            System.Diagnostics.Trace.WriteLine(string.Format("{0:yyyyMMddHHmmss}\tTaipeiBusScheduleStopSign End.({1})", DateTime.Now, sw.Elapsed.TotalMilliseconds));
        }
    }
}
