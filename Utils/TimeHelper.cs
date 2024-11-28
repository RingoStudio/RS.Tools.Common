using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RS.Tools.Common.Utils
{
    public class TimeHelper
    {
        #region TIME STAMP
        public static long ToTimeStamp(int year, int month, int day, int hour = 0, int minute = 0, int second = 0) => ToTimeStamp(new DateTime(year: year, month: month, day: day, hour: hour, minute: minute, second: second));
        public static long ToTimeStampMills() => ToTimeStampMills(DateTime.Now);
        public static long ToTimeStampMills(DateTime dateTime) => Convert.ToInt64((dateTime.ToUniversalTime() - new DateTime(1970, 1, 1, 0, 0, 0, 0, System.DateTimeKind.Utc)).TotalMilliseconds);
        public static long ToTimeStamp(DateTime dateTime) => Convert.ToInt64((dateTime.ToUniversalTime() - new DateTime(1970, 1, 1, 0, 0, 0, 0, System.DateTimeKind.Utc)).TotalSeconds);
        public static long ToTimeStamp() => ToTimeStamp(DateTime.Now);
        public static long GetTodayTimeStamp(int hour, int min = 0, int sec = 0)
        {
            var now = DateTime.Now;
            return ToTimeStamp(now.Year, now.Month, now.Day, hour, min, sec);
        }
        /// <summary>
        /// 计算当前是今年第几天
        /// </summary>
        /// <param name="dateTime"></param>
        /// <returns></returns>
        public static long YearDays(DateTime dateTime)
        {
            var firstDate = new DateTime(year: dateTime.Year, month: 1, day: 1, hour: 0, minute: 0, second: 0);
            return (dateTime - firstDate).Days + 1;
        }
        /// <summary>
        /// 从时间戳转换DateTime
        /// </summary>
        /// <param name="TimeStamp"></param>
        /// <returns></returns>
        public static DateTime ToDateTime(long timeStamp)
        {
            System.DateTime dtDateTime = new DateTime(1970, 1, 1, 0, 0, 0, 0, System.DateTimeKind.Utc);
            dtDateTime = dtDateTime.AddSeconds(timeStamp).ToLocalTime();
            return dtDateTime;
        }

        public static string ToGMTFormat(long time = -1)
        {
            time = time == -1 ? ToTimeStamp() : time;
            var dt = ToDateTime(time);
            return dt.ToString("r") + dt.ToString("zzz").Replace(":", "");
        }
        /// <summary>
        /// 从DateTime转换对应格式的字符串
        /// </summary>
        /// <param name="dateTime"></param>
        /// <param name="format">"yyyy-MM-dd-HH-mm-ss"</param>
        /// <returns></returns>
        public static string DateTimeDesc(DateTime dateTime, string format) => dateTime.ToString(format);
        /// <summary>
        /// 从时间戳转换对应格式的字符串
        /// </summary>
        /// <param name="timeStamp"></param>
        /// <param name="format">"yyyy-MM-dd-HH-mm-ss"</param>
        /// <returns></returns>
        public static string DateTimeDesc(long timeStamp, string format) => ToDateTime(timeStamp).ToString(format);

        /// <summary>
        /// 条件121适配的时间字符串
        /// </summary>
        /// <param name="timeStamp"></param>
        /// <returns></returns>
        public static string ConditionTimeDesc(long timeStamp) => DateTimeDesc(timeStamp, "yyyy-MM-dd-HH:mm:ss");
        public static string PureTimeDesc(long timeStamp) => DateTimeDesc(timeStamp, "yyyyMMddHHmmss");
        public static string SimpleTimeDesc(long timeStamp) => DateTimeDesc(timeStamp, "yyyy.MM.dd HH:mm:ss");
        public static string SimpleTimeDescJustDate(long timeStamp) => DateTimeDesc(timeStamp, "yyyy.MM.dd");
       
        /// <summary>
        /// 时间戳转中文时间 yyyy年MM月dd日 HH:mm:ss
        /// </summary>
        /// <param name="timeStamp"></param>
        /// <returns></returns>
        public static string ChinsesTimeDesc(long timeStamp) => timeStamp == long.MinValue ? "N/A" : DateTimeDesc(timeStamp, "yyyy年MM月dd日 HH:mm:ss");
        /// <summary>
        /// 时间戳转中文时间 周w HH:mm:ss
        /// </summary>
        /// <param name="timeStamp"></param>
        /// <returns></returns>
        public static string ChineseTimeDescWithWeekday(long timeStamp)
        {
            var dateTime = ToDateTime(timeStamp);
            return dateTime.DayOfWeek switch
            {
                DayOfWeek.Monday => "周一",
                DayOfWeek.Tuesday => "周二",
                DayOfWeek.Wednesday => "周三",
                DayOfWeek.Thursday => "周四",
                DayOfWeek.Friday => "周五",
                DayOfWeek.Saturday => "周六",
                DayOfWeek.Sunday => "周日",
                _ => "",
            } + $"{dateTime.ToString("HH:mm:ss")}";
        }
        public static string ChinsesTimeDurationDesc(long timeStamp)
        {
            var list = new List<string>();

            var d = timeStamp / 86400;
            if (d > 0) list.Add($"{d}天");
            timeStamp %= 86400;
            d = timeStamp / 3600;
            if (d > 0) list.Add($"{d}小时");
            timeStamp %= 3600;
            d = timeStamp / 60;
            if (d > 0) list.Add($"{d}分钟");
            timeStamp %= 60;
            if (timeStamp > 0) list.Add($"{timeStamp}秒");
            return string.Join("", list);
        }
        public static string FileNameTimeDesc(long timeStamp) => DateTimeDesc(timeStamp, "yyyy_MM_dd_HH_mm_ss");

        private static List<string> _chineseTiemDescCharactors = new List<string> { "年", "月", "日", " " };
        public static long ChineseTimeValue(string desc)
        {
            if (string.IsNullOrEmpty(desc)) return 0;
            desc = desc.Replace(" ", "");
            foreach (var item in _chineseTiemDescCharactors) desc = desc.Replace(item, ":");
            var arr = desc.Split(":");
            if (arr.Length != 6) return 0;
            return ToTimeStamp(new DateTime(year: Convert.ToInt32(arr[0]), month: Convert.ToInt32(arr[1]), day: Convert.ToInt32(arr[2]), hour: Convert.ToInt32(arr[3]), minute: Convert.ToInt32(arr[4]), second: Convert.ToInt32(arr[5])));
        }
        /// <summary>
        /// 为时间戳增加月数
        /// </summary>
        /// <param name="time"></param>
        /// <param name="months"></param>
        /// <returns></returns>
        public static long AddMonths(long time, int months)
        {
            var dateTime = ToDateTime(time);
            dateTime = dateTime.AddMonths(months);
            return ToTimeStamp(dateTime);
        }

        public static long AddYears(long time, int years)
        {
            var dateTime = ToDateTime(time);
            dateTime = dateTime.AddYears(years);
            return ToTimeStamp(dateTime);
        }

        public static long AddWeeks(long time, int weeks)
        {
            return AddDays(time, weeks * 7);
        }
        public static long AddDays(long time, int days)
        {
            var dateTime = ToDateTime(time);
            dateTime = dateTime.AddDays(days);
            return ToTimeStamp(dateTime);
        }

        #endregion


        #region TIME CALC
        public static bool IsSameDay(long time, long now = -1)
        {
            if (now <= 0) now = ToTimeStamp();
            var day1 = ToDateTime(time);
            var day2 = ToDateTime(now);
            return day1.Year == day2.Year &&
                   day1.Month == day2.Month &&
                   day1.Day == day2.Day;
        }
        #endregion
    }
}
