

using System;
using System.Collections.Generic;
using System.Linq;

namespace BotApplication.Models
{
    public class MessagesDomain
    {
        // 取得日期
        public string GetDate(string getLink, List<string> Date, string ArriveID)
        {
            var dt = DateTime.Now;
            var dtM = DateTime.Now.Month;
            var Today = dt.ToString("yyyy/MM/dd").Replace("/", "-");
            //var halfMonth = dt.AddMonths(6).ToString("yyyy/MM/dd").Replace("/", "-");

            // 暫存年月日,String,split
            var tempYear = string.Empty;
            var tempMonth = string.Empty;
            var tempDay = string.Empty;
            var tempString = string.Empty;
            List<string> tempDateList = new List<string>();
            string[] splitString;
            // 判斷12月或12月1日(站存變數)
            var tempD = string.Empty;
            // 拚完後的日期
            var date = string.Empty;
            // 有多個日期的判斷
            foreach (var i in Date)
            {
                var dateStr = string.Empty;
                // 判斷是否為年
                if (i == dt.Year.ToString() || i == dt.AddYears(1).Year.ToString() || i == dt.AddYears(2).Year.ToString())
                    tempYear = i;
                // 如果是年+月+日的情況
                // 2019年3月6日
                if (i.Contains("年") && i.Contains("月") && i.Contains("日"))
                {
                    tempString = i.Replace("月", "-").Replace("年", "-").Replace("日", "");
                    splitString = tempString.Split('-');
                    // 年+月+日
                    tempMonth = int.Parse(splitString[1]).ToString("00");
                    tempDay = int.Parse(splitString[2]).ToString("00");
                    tempDateList.Add(tempYear + "-" + tempMonth + "-" + tempDay);
                }// 2019年
                else if (i.Contains("年"))
                {
                    tempD = "沒有輸入日，所以預設當月的最後一天";
                    if (i.Contains("前"))
                    {
                        tempString = i.Replace("年", "").Replace("之前", "").Replace("以前", "").Replace("前", "");
                        tempDateList.Add(dt.AddYears(-1).Year + "-" + dt.Month.ToString("00") + "-" + dt.Day.ToString("00"));
                        tempDateList.Add(tempString + "-12-31");
                    } else if (i.Contains("後") || i.Contains("年"))
                    {
                        tempString = i.Replace("年", "").Replace("之後", "").Replace("以後", "").Replace("後", "");
                        tempDateList.Add(dt.AddYears(1).Year + "-01-01");
                        tempDateList.Add(tempString + "-12-31");
                    }
                }// 3月6日
                else if (i.Contains("月") && i.Contains("日"))
                {
                    tempString = i.Replace("月", "-").Replace("日", "");
                    tempD = "輸入的日當最後一天";
                    splitString = tempString.Split('-');
                    // 月+日
                    tempMonth = int.Parse(splitString[0]).ToString("00");
                    tempDay = int.Parse(splitString[1]).ToString("00");
                    // 判斷使用者是問哪一年
                    tempYear = GetYear(tempMonth, tempDay);
                    tempDateList.Add(tempYear + "-" + tempMonth + "-" + tempDay);
                }// 3月 or 3月1號
                else if (i.Contains("月"))
                {
                    tempString = i.Replace("月", "-");
                    splitString = tempString.Split('-');
                    // 3月
                    if (string.IsNullOrWhiteSpace(splitString[1]))
                    {
                        tempD = "沒有輸入日，所以預設當月的最後一天";
                        tempMonth = int.Parse(splitString[0]).ToString("00");
                        tempDay = "01";
                    }// 3月1號(號不會被判定在字串裡，因此不需做處理)
                    else if (!string.IsNullOrWhiteSpace(splitString[1]))
                    {
                        tempD = "輸入的日當最後一天";
                        tempMonth = int.Parse(splitString[0]).ToString("00");
                        tempDay = int.Parse(splitString[1]).ToString("00");
                    }
                    // 判斷使用者是問哪一年
                    tempYear = GetYear(tempMonth, tempDay);
                    tempDateList.Add(tempYear + "-" + tempMonth + "-" + tempDay);
                }// 2018-3-6、2018-3，不會有3-6的日期(因為機器人不會判定它是日期)
                else if (i.Contains("-"))
                {
                    // 範圍斷字，只有精確到日 才會跑日期範圍
                    if (i.Contains("到") || i.Contains("至"))
                    {
                        tempD = "輸入的日當最後一天";
                        tempString = i.Replace("到", ",").Replace("至", ",");
                        splitString = tempString.Split(',');
                        foreach (var s in splitString)
                        {
                            splitString = s.Split('-');
                            tempYear = int.Parse(splitString[0]).ToString("00");
                            tempMonth = int.Parse(splitString[1]).ToString("00");
                            tempDay = int.Parse(splitString[2]).ToString("00");
                            tempDateList.Add(tempYear + "-" + tempMonth + "-" + tempDay);
                        }
                    }
                    else
                    {
                        splitString = i.Split('-');
                        // 2018-3
                        if (splitString.Count() == 2)
                        {
                            tempD = "沒有輸入日，所以預設當月的最後一天";
                            tempYear = splitString[0];
                            tempMonth = int.Parse(splitString[1]).ToString("00");
                            tempDay = "01";
                        }// 2018-3-6
                        else if (splitString.Count() == 3)
                        {
                            tempD = "輸入的日當最後一天";
                            tempYear = splitString[0];
                            tempMonth = int.Parse(splitString[1]).ToString("00");
                            tempDay = int.Parse(splitString[2]).ToString("00");
                        }
                        tempDateList.Add(tempYear + "-" + tempMonth + "-" + tempDay);
                    }


                }// 2018/3/6 or 3/6
                else if (i.Contains("/"))
                {
                    // 範圍斷字，只有精確到日 才會跑日期範圍
                    if (i.Contains("到") || i.Contains("至"))
                    {
                        tempD = "輸入的日當最後一天";
                        tempString = i.Replace("到", ",").Replace("至", ",");
                        splitString = tempString.Split(',');
                        foreach (var s in splitString)
                        {
                            splitString = s.Split('/');
                            // 3/6
                            if (splitString.Count() == 2)
                            {
                                tempMonth = int.Parse(splitString[0]).ToString("00");
                                tempDay = int.Parse(splitString[1]).ToString("00");
                                tempYear = GetYear(tempMonth, tempDay);
                            }// 2018/3/6 
                            else if (splitString.Count() == 3)
                            {
                                tempYear = splitString[0];
                                tempMonth = int.Parse(splitString[1]).ToString("00");
                                tempDay = int.Parse(splitString[2]).ToString("00");
                            }
                            tempDateList.Add(tempYear + "-" + tempMonth + "-" + tempDay);
                        }
                    }
                    else
                    {
                        // 2018/11
                        splitString = i.Split('/');
                        if (splitString.Count() == 2)
                        {
                            tempD = "沒有輸入日，所以預設當月的最後一天";
                            tempYear = splitString[0];
                            tempMonth = int.Parse(splitString[1]).ToString("00");
                            tempDay = "01";
                        }// 2018/12/31
                        else if (splitString.Count() == 3)
                        {
                            tempD = "輸入的日當最後一天";
                            tempYear = splitString[0];
                            tempMonth = int.Parse(splitString[1]).ToString("00");
                            tempDay = int.Parse(splitString[2]).ToString("00");
                        }
                        tempDateList.Add(tempYear + "-" + tempMonth + "-" + tempDay);
                    }
                }

            }

            // 如果使用者只有輸入西元年
            if (tempDateList.Count() == 0)
                // 如果西元年是<=今年，則預設當天
                if (Convert.ToInt32(tempYear) <= dt.Year)
                    tempDateList.Add(tempYear + "-" + dt.Month + "-" + dt.Day);
                else // >=今年就預設當年1/1
                    tempDateList.Add(tempYear + "-01-01");
            // 日期排序
            tempDateList.Sort();
            date = string.Join(",", tempDateList);

            // 判斷是否有多個日期
            if (date.Contains(","))
            {
                var dateArray = date.Split(',');
                DateTime lastDay = DateTime.Now; ;
                // 取得最後月份的最後一天
                var tempDateEnd = DateTime.Parse(dateArray.LastOrDefault());
                // 如果+1個月份-1天有變月份表示是取得月底，EX:4/1 -> 3/31
                //if (DateTime.Parse(dateArray.LastOrDefault()).AddMonths(1).AddDays(-1).Month == tempDateEnd.Month)
                if (tempD == "輸入的日當最後一天")
                    lastDay = tempDateEnd;
                else if (tempD == "沒有輸入日，所以預設當月的最後一天")
                    lastDay = DateTime.Parse(dateArray.LastOrDefault()).AddMonths(1).AddDays(-1);

                var DateEnd = tempDateEnd.Year.ToString() + "-" + tempDateEnd.Month.ToString("00") + "-" + lastDay.Day.ToString("00");
                getLink = "https://utravel.liontravel.com/search?PageSize=3&GoDateStart=" + dateArray.FirstOrDefault() + "&GoDateEnd=" + DateEnd + "&ArriveID=" + ArriveID;

            }// 只有一個日期，系統預設半年
            else
            {
                var halfMonth = DateTime.Parse(date);
                getLink = "https://utravel.liontravel.com/search?PageSize=3&GoDateStart=" + date + "&GoDateEnd=" + halfMonth.AddMonths(6).ToString("yyyy/MM/dd").Replace("/", "-") + "&ArriveID=" + ArriveID;
            }

            return getLink;
        }

        // 取得年分
        public string GetYear(string Month, string Day)
        {
            string Year = string.Empty;
            DateTime dt = DateTime.Now;
            // 如果使用者只問月份，只需判斷月份
            if (string.IsNullOrWhiteSpace(Day))
            {
                // <當月份表示使用者是問明年
                if (Convert.ToInt32(Month) < Convert.ToInt32(dt.Month.ToString("00")))
                    Year = dt.AddYears(1).Year.ToString();
                else
                    Year = dt.Year.ToString();
            }
            else
            {
                // <=當月份表示使用者是問明年
                if (Convert.ToInt32(Month) <= Convert.ToInt32(dt.Month.ToString("00")) && Convert.ToInt32(Day) < Convert.ToInt32(dt.Day.ToString("00")))
                    Year = dt.AddYears(1).Year.ToString();
                else
                    Year = dt.Year.ToString();
            }
            return Year;
        }

        
    }
}
