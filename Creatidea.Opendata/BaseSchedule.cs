using System;

namespace Creatidea.Opendata
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

        /// <summary>
        /// 下次執行時間
        /// </summary>
        private DateTime NextRunTime { get; set; }

        /// <summary>
        /// 是否繼續執行
        /// </summary>
        private bool IsStart { get; set; }

        /// <summary>
        /// 是否開始就執行
        /// </summary>
        protected abstract bool RunForStart();

        /// <summary>
        /// 執行的方法
        /// </summary>
        protected abstract void Run();

        public int Month { get; set; }
        public int Day { get; set; }
        public int Hour { get; set; }
        public int Minute { get; set; }
        public int Second { get; set; }

        /// <summary>
        /// 取得下次執行時間
        /// </summary>
        private DateTime NextSchedule
        {
            get
            {
                var result = DateTime.Now.AddMonths(Month).AddDays(Day).AddHours(Hour).AddMinutes(Minute).AddSeconds(Second);

                return result;
            }
        }

        /// <summary>
        /// 開始排程
        /// </summary>
        public void Start()
        {
            try
            {

                IsStart = true;
                NextRunTime = RunForStart() ? DateTime.Now : NextSchedule;

                while (NextRunTime != DateTime.MaxValue)
                {
                    if (!IsStart)
                    {
                        break;
                    }

                    if (NextRunTime > DateTime.Now)
                    {
                        continue;
                    }

                    Run();
                    NextRunTime = NextSchedule;
                    GC.Collect();
                }
            }
            catch (Exception e)
            {
                System.Diagnostics.Trace.WriteLine(e.ToString());
            }
        }

        /// <summary>
        /// 停止排程
        /// </summary>
        public void Stop()
        {
            IsStart = false;
        }
    }
}
