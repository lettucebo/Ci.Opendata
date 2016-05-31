using System;
using Newtonsoft.Json.Linq;

namespace Creatidea.Opendata
{
    public interface ISchedule
    {
        void Start();
        void Stop();
    }

    public abstract class OpenData: IDisposable
    {
        /// <summary>
        /// 鎖定用物件
        /// </summary>
        protected object LockObj = new object();

        /// <summary>
        /// 讀取資料
        /// </summary>
        /// <returns></returns>
        public abstract JObject Data();

        /// <summary>
        /// 儲存資料(物件)
        /// </summary>
        /// <param name="jObj">The j object.</param>
        protected abstract void ToMemory(JObject jObj);

        /// <summary>
        /// 讀取資料並存入物件
        /// </summary>
        public void DataToMemory()
        {
            var jsonObj = Data();
            ToMemory(jsonObj);
        }

        public abstract void Dispose();
    }

    public abstract class OpenDataSchedule : ISchedule, IDisposable
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

                    System.Diagnostics.Trace.WriteLine(string.Format("{0:yyyyMMddHHmmss}\t{1} Start.", DateTime.Now, GetType().FullName));

                    var sw = new System.Diagnostics.Stopwatch();
                    sw.Reset();//碼表歸零
                    sw.Start();//碼表開始計時

                    Run();
                    NextRunTime = NextSchedule;
                    GC.Collect();

                    sw.Stop();
                    System.Diagnostics.Trace.WriteLine(string.Format("{0:yyyyMMddHHmmss}\t{1} End.({2})", DateTime.Now, GetType().FullName, sw.Elapsed.TotalMilliseconds));
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

        public abstract void Dispose();
    }
}
