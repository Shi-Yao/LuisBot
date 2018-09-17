using System;
using System.Collections.Generic;
using System.Configuration;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using System.Web.Http;
using Bot_Application.DataModel;
using LionTourBot.Model.ViewModel;
using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Bot.Connector;
using Newtonsoft.Json;

namespace Bot_Application
{
    [BotAuthentication]
    public class MessagesController : ApiController
    {
        string strReply = string.Empty;
        /// <summary>
        /// POST: api/Messages
        /// Receive a message from a user and reply to it
        /// </summary>
        public async Task<HttpResponseMessage> Post([FromBody]Activity activity)
        {
            // 參數宣告
            Activity reply = new Activity();
            var responses = new HttpResponseMessage();
            var IntentString = string.Empty;
            var IntentMaxScore = 0d;
            var getLink = string.Empty;
            DateTime dt = DateTime.Now;

            if (activity.Type == ActivityTypes.Message)
            {
                ConnectorClient connector = new ConnectorClient(new Uri(activity.ServiceUrl));
                // 防止使用者輸入中文數字
                var check = CheckNumber(activity.Text);
                if (!check)
                {
                    strReply = "目前輸入的數字只能輸入啊拉伯數字唷";
                    reply = activity.CreateReply(strReply);
                    await connector.Conversations.ReplyToActivityAsync(reply);
                    responses = Request.CreateResponse(HttpStatusCode.OK);
                    return responses;
                }
                // 設定LUIS Key和Password
                string strLuisKey = ConfigurationManager.AppSettings["LUISAPIKey"].ToString();
                string strLuisAppId = ConfigurationManager.AppSettings["LUISAppId"].ToString();
                // 轉換中文年
                activity.Text = DelYear(activity.Text);
                string strMessage = HttpUtility.UrlEncode(activity.Text);
                // API連線設定
                //string strLuisUrl = $"https://api.projectoxford.ai/luis/v1/application?id={strLuisAppId}&subscription-key={strLuisKey}&q={strMessage}";
                string strLuisUrl = $"https://westus.api.cognitive.microsoft.com/luis/v2.0/apps/{strLuisAppId}?subscription-key={strLuisKey}&verbose=true&timezoneOffset=0&q={strMessage}";
                // 收到文字訊息後，往LUIS送
                WebRequest request = WebRequest.Create(strLuisUrl);
                HttpWebResponse response = (HttpWebResponse)request.GetResponse();
                Stream dataStream = response.GetResponseStream();
                StreamReader reader = new StreamReader(dataStream);
                string json = reader.ReadToEnd();
                Utterance objLUISRes = Newtonsoft.Json.JsonConvert.DeserializeObject<Utterance>(json);

                // 擷取Intent與Entity，機器人回覆字串，ArriveID
                IEnumerable<Arrive> ArriveNameTemp = new List<Arrive>();
                IEnumerable<Arrive> ArriveIDTemp = new List<Arrive>();
                IEnumerable<Arrive> ArriveIslandTemp = new List<Arrive>();
                string ArriveID = string.Empty;

                // BOT遇到不懂問題回覆
                string replyUnknowQuest = "請詢問旅遊相關問題，或是想去的國家與城市";

                // 首先判斷使用者輸入的Intent哪個比較高
                foreach (var i in objLUISRes.intents)
                {
                    var IntentMinScore = Convert.ToDouble(i.score);
                    if (IntentMinScore > IntentMaxScore)
                    {
                        IntentMaxScore = IntentMinScore;
                        IntentString = i.intent;
                    }
                }

                // 判斷句子Intent
                if (IntentString == "None")
                {
                    strReply = replyUnknowQuest;
                    reply = activity.CreateReply(strReply);
                    await connector.Conversations.ReplyToActivityAsync(reply);
                    responses = Request.CreateResponse(HttpStatusCode.OK);
                    return responses;

                }
                else if (IntentString == "詢問旅遊問題")
                {
                    strReply = "你是在" + IntentString;
                    reply = activity.CreateReply(strReply);
                }
                else if (IntentString == "詢問旅遊時間")
                {
                    strReply = "你是在" + IntentString;
                    strReply += "\n目前暫無提供此服務~請重新詢問";
                    reply = activity.CreateReply(strReply);
                    await connector.Conversations.ReplyToActivityAsync(reply);
                    responses = Request.CreateResponse(HttpStatusCode.OK);
                    return responses;
                }
                else if (IntentString == "詢問票價、價錢")
                {
                    strReply = "你是在" + IntentString;
                    strReply += "\n目前暫無提供此服務~請重新詢問";
                    reply = activity.CreateReply(strReply);
                    await connector.Conversations.ReplyToActivityAsync(reply);
                    responses = Request.CreateResponse(HttpStatusCode.OK);
                    return responses;
                }
                else if (IntentString == "詢問是否好玩")
                {
                    strReply = "你是在" + IntentString;
                    strReply += "\n目前暫無提供此服務~請重新詢問";
                    reply = activity.CreateReply(strReply);
                    await connector.Conversations.ReplyToActivityAsync(reply);
                    responses = Request.CreateResponse(HttpStatusCode.OK);
                    return responses;
                }


                // 取得出發地及目的地，先預設3筆，以及近半年行程
                var strEntityDep = objLUISRes.entities.Where(w => w.type == "地點::出發地").Select(s => s.entity).FirstOrDefault();
                var strEntityName = objLUISRes.entities.Where(w => w.type == "地點::目的地").Select(s => s.entity).ToList(); // 可能多個

                // 判斷語句是否擷取出目的地關鍵字，有的話需要讀取Json，並拼出ArriveID
                if (strEntityName != null)
                {
                    // 讀取Json，並將關鍵字目的地轉換成ArriveID
                    using (StreamReader r = new StreamReader(@"E:\Bot Application\Bot Application\ArriveID.json", Encoding.Default))
                    {
                        string ArriveIDJson = r.ReadToEnd();
                        var result = JsonConvert.DeserializeObject<RootObject>(ArriveIDJson).ArriveID;
                        // 先判斷ArriveName，層級小優先
                        foreach (var i in strEntityName)
                        {
                            ArriveNameTemp = result.Where(s => s.ArriveName == i).ToList();
                            if (ArriveNameTemp.Count() == 0)
                            {
                                // 如果沒有四川、九州、北海道，再去找國家
                                ArriveIDTemp = result.Where(s => s.CountryName == i).ToList();
                                if (ArriveIDTemp.Count() == 0)
                                {
                                    // 如果沒有國家再去找島嶼
                                    ArriveIslandTemp = result.Where(s => s.IslandName == i).ToList();
                                    if (ArriveIslandTemp.Count() == 0)
                                    {
                                        strReply = "目前詢問的地點暫無提供服務唷~";
                                        reply = activity.CreateReply(strReply);
                                        await connector.Conversations.ReplyToActivityAsync(reply);
                                        responses = Request.CreateResponse(HttpStatusCode.OK);
                                        return responses;
                                    }
                                    //ArriveID += "" + "-" + ArriveIslandTemp.FirstOrDefault().CountryCode + "-" + ArriveIslandTemp.FirstOrDefault().IslandID + ",";
                                    ArriveID += "" + "--" + ArriveIslandTemp.FirstOrDefault().IslandID + ",";
                                    continue;
                                }
                                ArriveID += "" + "-" + ArriveIDTemp.FirstOrDefault().CountryCode + "-" + ArriveIDTemp.FirstOrDefault().IslandID + ",";
                            }
                            else
                            {
                                ArriveID += ArriveNameTemp.FirstOrDefault().ArriveID + "-" + ArriveNameTemp.FirstOrDefault().CountryCode + "-" + ArriveNameTemp.FirstOrDefault().IslandID + ",";
                            }
                        }
                    }
                }

                var messagesDomain = new BotApplication.Models.MessagesDomain();
                // 判斷使用者有輸入日期關鍵字(精確到輸入日)
                var DateDay = objLUISRes.entities.Where(w => w.type == "builtin.datetimeV2.date").Select(s => s.entity).ToList();
                // 判斷使用者有輸入日期關鍵字(僅只輸入月 or XXX到XXX or XXX至XXX)
                var DateRange = objLUISRes.entities.Where(w => w.type == "builtin.datetimeV2.daterange").Select(s => s.entity).ToList();
                // 處理使用者輸入的日期格式
                if (DateDay.Count() != 0)
                    getLink = messagesDomain.GetDate(getLink, DateDay, ArriveID);
                else if (DateRange.Count() != 0)
                    getLink = messagesDomain.GetDate(getLink, DateRange, ArriveID);

                // 表示使用者沒有輸入日期，預設今天~半年行程給使用者
                if (DateDay.Count == 0 && DateRange.Count() == 0)
                    getLink = "https://utravel.liontravel.com/search?PageSize=3&GoDateStart=" + dt.ToString("yyyy/MM/dd").Replace("/", "-") + "&GoDateEnd=" + dt.AddMonths(6).ToString("yyyy/MM/dd").Replace("/", "-") + "&ArriveID=" + ArriveID;


                if (!string.IsNullOrWhiteSpace(strEntityDep))
                {
                    // 判斷出發地，測試預設3種，且出發地皆為台灣縣市
                    var tempDepstr = string.Empty;
                    switch (strEntityDep)
                    {
                        case "台北":
                            tempDepstr = string.Format("{0}", "&DepartureID=TPE");
                            break;
                        case "高雄":
                            tempDepstr = string.Format("{0}", "&DepartureID=KHH");
                            break;
                        case "基隆":
                            tempDepstr = string.Format("{0}", "&DepartureID=KLU");
                            break;
                        default:
                            tempDepstr = string.Format("{0}", "&DepartureID=");
                            break;
                    }
                    getLink += tempDepstr;
                }

                // 遊玩天數
                getLink += "&Days=";
                var getDays = objLUISRes.entities.Where(w => w.type == "遊玩天數").Select(s => s.entity.Split(' ')[0]).ToList();
                getLink += string.Join(",", getDays);

                // 判定連結年分
                if (getLink.Contains(dt.Year.ToString()) && getLink.Contains(dt.AddYears(1).Year.ToString()))
                    strReply += "\n此為今年~明年的行程，若想要獲得更精準的年月份查詢結果\n請於句子中加入今年or 明年 or 2018西元年格式\n提供以下行程給您參考:" + getLink;
                else if (getLink.Contains(dt.Year.ToString()))
                    strReply += "\n此為今年的行程，若想要獲得更精準的年月份查詢結果\n請於句子中加入今年or 明年 or 2018西元年格式\n提供以下行程給您參考:" + getLink;
                else if (getLink.Contains(dt.AddYears(1).Year.ToString()))
                    strReply += "\n此為明年的行程，若想要獲得更精準的年月份查詢結果\n請於句子中加入今年or 明年 or 2018西元年格式\n提供以下行程給您參考:" + getLink;


                reply = activity.CreateReply(strReply);
                await connector.Conversations.ReplyToActivityAsync(reply);
            }
            else
            {
                HandleSystemMessage(activity);
            }
            responses = Request.CreateResponse(HttpStatusCode.OK);
            return responses;
        }

        // 限制使用者數字只能輸入啊拉伯數字
        public bool CheckNumber(string text)
        {
            bool check = true;
            bool check_tier2 = true;
            var textTemp = string.Empty;
            var AreaNameTemp = new List<ArriveName>();
            int cnt1 = 0;
            int cnt2 = 0;
            // 限制使用者數字只能輸入啊拉伯數字
            IEnumerable<string> arrary = new string[]{ "一", "二", "三", "四", "五", "六", "七", "八", "九", "十",
                                "壹", "貳", "參", "肆", "伍", "陸", "柒", "捌", "玖", "拾" };

            foreach (var num in arrary)
            {
                if (text.Contains(num))
                {
                    // 如果有arrary裡面的字，要先判斷特定國家的名字是否有數字，EX:九州
                    using (StreamReader r = new StreamReader(@"E:\Bot Application\Bot Application\Area.json", Encoding.Default))
                    {
                        string ArriveName = r.ReadToEnd();
                        var result = JsonConvert.DeserializeObject<Root>(ArriveName).AreaName;
                        for (int i = 0; i <= text.Length - 2; i++)
                        {
                            textTemp = text.Substring(i, 2);
                            // 如果兩兩比對的文字中有array的中文數字
                            if (textTemp.Contains(num))
                            {
                                cnt1++;
                                //check_tier2 = false;
                                // 如果該中文數字沒有出現在json表示不是區域
                                if (result.Where(s => s.Name == textTemp).Any())
                                {
                                    cnt2++;
                                    //check_tier2 = true;
                                    //check = true;
                                    //break;
                                }
                            }
                        }
                    }
                }
            }
            // 如果在兩兩比對的文字中的數字是中文那就break
            if (cnt1 != 2 * cnt2)
                check = false;
            else
                check = true;

            return check;
        }

        // 去除今年、明年、後年
        public string DelYear(string text)
        {
            DateTime dt = DateTime.Now;
            if (text.Contains("今年"))
                text = text.Replace("今年", dt.Year.ToString() + "年");
            else if (text.Contains("明年"))
                text = text.Replace("明年", dt.AddYears(1).Year + "年");
            else if (text.Contains("後年"))
                text = text.Replace("後年", dt.AddYears(2).Year + "年");

            return text;
        }



        private Activity HandleSystemMessage(Activity message)
        {
            if (message.Type == ActivityTypes.DeleteUserData)
            {
                // Implement user deletion here
                // If we handle user deletion, return a real message
            }
            else if (message.Type == ActivityTypes.ConversationUpdate)
            {
                // Handle conversation state changes, like members being added and removed
                // Use Activity.MembersAdded and Activity.MembersRemoved and Activity.Action for info
                // Not available in all channels
            }
            else if (message.Type == ActivityTypes.ContactRelationUpdate)
            {
                // Handle add/remove from contact lists
                // Activity.From + Activity.Action represent what happened
            }
            else if (message.Type == ActivityTypes.Typing)
            {
                // Handle knowing tha the user is typing
            }
            else if (message.Type == ActivityTypes.Ping)
            {
            }

            return null;
        }
    }

}