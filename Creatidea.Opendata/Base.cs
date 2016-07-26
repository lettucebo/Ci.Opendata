﻿using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data.SqlTypes;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Text;
using Newtonsoft.Json.Linq;

namespace Creatidea.Opendata
{
    /// <summary>
    /// 排程類型
    /// </summary>
    public enum ScheduleType
    {
        /// <summary>
        /// 間格時間執行
        /// </summary>
        None,
        /// <summary>
        /// 每天執行
        /// </summary>
        Daily,
        /// <summary>
        /// 每個禮拜執行
        /// </summary>
        Weekly,
        /// <summary>
        /// 每個月執行
        /// </summary>
        Monthly,
    }

    public interface ISchedule
    {
        void Start();
        void Stop();
    }

    public abstract class BaseClass
    {
        /// <summary>
        /// 現在執行的類別名稱
        /// </summary>
        protected string ClassName
        {
            get
            {
                return $"{GetType().FullName}";
            }
        }

        /// <summary>
        /// 紀錄
        /// </summary>
        /// <param name="message"></param>
        /// <param name="str"></param>
        public void Trace(string message, params object[] str)
        {
            var programName = GetType().FullName;
            try
            {
                message = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + "\t" + string.Format(message, str) + "\r\n";
                var tempPath = System.IO.Path.GetTempPath();
                if (!tempPath.EndsWith("\\"))
                {
                    tempPath += "\\";
                }
                tempPath += programName + DateTime.Now.ToString("yyyyMMdd") + ".txt";

                System.IO.File.AppendAllText(tempPath, message, Encoding.UTF8);
            }
            catch (Exception)
            {
                // ignored
            }
            try
            {
                var sLog = "Application";

                if (!EventLog.SourceExists(programName))
                    EventLog.CreateEventSource(programName, sLog);

                EventLog.WriteEntry(programName, message, EventLogEntryType.Information, 0);
                //EventLog.WriteEntry(programName, message);
            }
            catch (Exception)
            {
                // ignored
            }

            System.Diagnostics.Trace.WriteLine(message);
            Console.WriteLine(message);
        }

        /// <summary>
        /// 紀錄
        /// </summary>
        /// <param name="message"></param>
        /// <param name="str"></param>
        public void Display(string message, params object[] str)
        {
            System.Diagnostics.Trace.WriteLine(message);
            Console.WriteLine(message);
        }

        /// <summary>
        /// 鎖定用物件
        /// </summary>
        private static readonly Dictionary<string, object> LockObjList = new Dictionary<string, object>();

        protected object LockObj
        {
            get
            {
                if (!LockObjList.ContainsKey(ClassName))
                {
                    LockObjList.Add(ClassName, new object());
                }
                return LockObjList[ClassName];
            }
        }

    }

    public abstract class OpenData : BaseClass, IDisposable
    {
        /// <summary>
        /// 即時資訊最後更新時間
        /// </summary>
        private static readonly Dictionary<string, DateTime> LastUpdateList = new Dictionary<string, DateTime>();

        /// <summary>
        /// 即時資訊最後更新時間
        /// </summary>
        public DateTime LastUpdate
        {
            get
            {
                lock (LockObj)
                {
                    if (!LastUpdateList.ContainsKey(ClassName))
                    {
                        return DateTime.MinValue;
                    }
                    return LastUpdateList[ClassName];
                }
            }
            set
            {
                lock (LockObj)
                {
                    if (!LastUpdateList.ContainsKey(ClassName))
                    {
                        LastUpdateList.Add(ClassName, value);
                    }
                    else
                    {
                        LastUpdateList[ClassName] = value;
                    }
                }
            }
        }

        /// <summary>
        /// 更新頻率(秒)
        /// </summary>
        private static readonly Dictionary<string, double> IntervalList = new Dictionary<string, double>();

        /// <summary>
        /// 設定更新頻率(秒)
        /// </summary>
        public double Interval
        {
            get
            {
                lock (LockObj)
                {
                    if (!IntervalList.ContainsKey(ClassName))
                    {
                        return 0;
                    }
                    return IntervalList[ClassName];
                }
            }
            set
            {
                lock (LockObj)
                {
                    if (!IntervalList.ContainsKey(ClassName))
                    {
                        IntervalList.Add(ClassName, value);
                    }
                    else
                    {
                        IntervalList[ClassName] = value;
                    }
                }
            }
        }

        /// <summary>
        /// 讀取資料
        /// </summary>
        /// <returns></returns>
        public abstract JObject Data();

        /// <summary>
        /// 儲存資料(物件)
        /// </summary>
        /// <param name="jObj">The j object.</param>
        protected abstract void Save(JObject jObj);

        /// <summary>
        /// 讀取資料並存入物件
        /// </summary>
        public void DataSave()
        {
            lock (LockObj)
            {
                var needUpdate = false;
                if (Interval > 0)
                {
                    if (LastUpdate < DateTime.Now.AddSeconds(-Interval))
                    {
                        needUpdate = true;
                    }
                }
                else
                {
                    needUpdate = true;
                }

                if (needUpdate)
                {
                    var jsonObj = Data();
                    Save(jsonObj);

                    LastUpdate = DateTime.Now;
                }
            }
        }

        public abstract void Dispose();

        /// <summary>
        /// 馬上抓取資料
        /// </summary>
        public static void GetNow(OpenData opendata, double interval = 0)
        {
            opendata.Interval = interval;
            opendata.DataSave();
        }

    }


    public abstract class OpenDataDataBase : OpenData
    {
        protected string ConnectionString = ConfigurationManager.ConnectionStrings["OpenData"].ConnectionString;
        protected int TimeOut = 3600;
    }

    public abstract class OpenDataSchedule : BaseClass, ISchedule, IDisposable
    {
        /// <summary>
        /// 執行目錄路徑
        /// </summary>
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
        /// 延遲執行
        /// </summary>
        public bool DelayStart { get; set; }

        /// <summary>
        /// 執行的方法
        /// </summary>
        public abstract void Run();

        /// <summary>
        /// 排程模式
        /// </summary>
        public ScheduleType ScheduleType { get; set; }

        /// <summary>
        /// Monthly:每個月幾號 Daily:每幾天
        /// </summary>
        public int RunDay { get; set; }

        /// <summary>
        /// 每周星期
        /// </summary>
        public DayOfWeek RunWeekday { get; set; }


        /// <summary>
        /// 時
        /// </summary>
        public int Hour { get; set; }
        /// <summary>
        /// 分
        /// </summary>
        public int Minute { get; set; }
        /// <summary>
        /// 秒
        /// </summary>
        public int Second { get; set; }

        /// <summary>
        /// 取得下次執行時間
        /// </summary>
        private DateTime NextSchedule
        {
            get
            {
                if (NextRunTime == DateTime.MinValue)
                {
                    //最小值 則先指定現在
                    NextRunTime = DateTime.Now;
                }

                DateTime nextRunTime;
                if (ScheduleType.None == ScheduleType)
                {
                    //定時排程
                    nextRunTime = NextRunTime;
                    while (NextRunTime >= nextRunTime)
                    {
                        nextRunTime = nextRunTime.AddHours(Hour).AddMinutes(Minute).AddSeconds(Second);
                    }
                }
                else
                {
                    //固定時間
                    nextRunTime = new DateTime(NextRunTime.Year, NextRunTime.Month, NextRunTime.Day, Hour, Minute, Second);
                    switch (ScheduleType)
                    {
                        case ScheduleType.Daily:
                            nextRunTime = nextRunTime.AddDays(RunDay);
                            break;
                        case ScheduleType.Weekly:
                            //每個禮拜
                            while (true)
                            {
                                nextRunTime = nextRunTime.AddDays(1);

                                if (nextRunTime.DayOfWeek == RunWeekday)
                                {
                                    break;
                                }
                            }
                            break;
                        case ScheduleType.Monthly:
                            //每月幾號
                            while (true)
                            {
                                nextRunTime = nextRunTime.AddDays(1);

                                if (nextRunTime.Day == RunDay)
                                {
                                    break;
                                }
                            }
                            break;
                        default:
                            return DateTime.MaxValue;
                    }
                }

                return nextRunTime;
            }
        }

        /// <summary>
        /// 開始排程
        /// </summary>
        public void Start()
        {
            IsStart = true;
            NextRunTime = ScheduleType == ScheduleType.None ? DateTime.Now : NextSchedule;

            Display($"{ClassName} First Run Time {NextRunTime:yyyy/MM/dd HH:mm:ss}.");

            while (NextRunTime != DateTime.MaxValue)
            {
                if (!IsStart)
                {
                    //已經停止運作
                    break;
                }

                if (NextRunTime > DateTime.Now)
                {
                    //還沒到執行時間
                    continue;
                }

                //先清除記憶體
                GC.Collect();

                Display($"{DateTime.Now:yyyyMMddHHmmss}\t{ClassName} Start.");

                var sw = new System.Diagnostics.Stopwatch();
                sw.Reset(); //碼表歸零
                sw.Start(); //碼表開始計時

                try
                {
                    Run();
                }
                catch (Exception e)
                {
                    Trace(e.ToString());
                }

                NextRunTime = NextSchedule;

                sw.Stop();
                Display($"{DateTime.Now:yyyyMMddHHmmss}\t{ClassName} End.({sw.Elapsed.TotalMilliseconds})");
            }
        }

        /// <summary>
        /// 停止排程
        /// </summary>
        public void Stop()
        {
            IsStart = false;
        }

        /// <summary>
        /// 釋放資源
        /// </summary>
        public abstract void Dispose();



    }
}
