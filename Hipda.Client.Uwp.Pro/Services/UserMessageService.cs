using Hipda.Client.Uwp.Pro.Models;
using Hipda.Http;
using HtmlAgilityPack;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Xml;
using Windows.UI.Xaml.Data;

namespace Hipda.Client.Uwp.Pro.Services
{
    public class UserMessageService
    {
        static HttpHandle _httpClient = HttpHandle.GetInstance();

        async Task<UserMessageDataModel> LoadUserMessageDataAsync(int userId, int limitCount, CancellationTokenSource cts)
        {
            var listData = new ObservableCollection<UserMessageItemModel>();
            int total = 0;

            // 读取数据
            string url = string.Format("http://www.hi-pda.com/forum/pm.php?uid={0}&filter=privatepm&daterange=5&_={1}", userId, DateTime.Now.Ticks.ToString("x"));
            string htmlContent = await _httpClient.GetAsync(url, cts);

            // 实例化 HtmlAgilityPack.HtmlDocument 对象
            HtmlDocument doc = new HtmlDocument();

            // 载入HTML
            doc.LoadHtml(htmlContent);

            // 最先读取提醒数据
            var promptContentNode = doc.DocumentNode.Descendants().FirstOrDefault(n => n.Name.Equals("div") && n.GetAttributeValue("class", "").Equals("promptcontent"));
            PromptService.GetPromptData(promptContentNode);

            var messageListNode = doc.DocumentNode.Descendants().FirstOrDefault(n => n.GetAttributeValue("class", "").Equals("pm_list"));
            if (messageListNode != null)
            {
                var nodeList = messageListNode.Descendants().Where(n => n.GetAttributeValue("id", "").StartsWith("pm_"));
                if (nodeList != null)
                {
                    total = nodeList.Count();
                    if (limitCount != -1 && total > limitCount)
                    {
                        nodeList = nodeList.Skip(total - limitCount);
                    }

                    foreach (var item in nodeList)
                    {
                        listData.Add(GetUserMessageItem(item));
                    }
                }
            }

            // 清除此用户的私信标识为“NEW”状态的 toast temp data
            ToastService.ClearPmToastTempData(userId);

            return new UserMessageDataModel { ListData = listData, Total = total };
        }

        private UserMessageItemModel GetUserMessageItem(HtmlNode htmlNode)
        {
            int uid = 0;
            var userIdNode = htmlNode.ChildNodes[3];
            var userInfoNode = htmlNode.ChildNodes[5];
            var messageNode = htmlNode.ChildNodes[7];
            string userIdStr = userIdNode.Attributes[0].Value;
            if (userIdStr.Equals("new"))
            {
                userIdNode = htmlNode.ChildNodes[4];
                userInfoNode = htmlNode.ChildNodes[6];
                messageNode = htmlNode.ChildNodes[8];
                userIdStr = userIdNode.Attributes[0].Value;
            }

            if (!userIdStr.Equals("avatar"))
            {
                userIdStr = userIdStr.Substring("space.php?uid=".Length);
                if (userIdStr.Contains("&"))
                {
                    uid = Convert.ToInt32(userIdStr.Split('&')[0]);
                }
                else
                {
                    uid = Convert.ToInt32(userIdStr);
                }
            }

            bool isRead = !userInfoNode.InnerHtml.Contains("notice_newpm.gif");
            string str = userInfoNode.InnerText.Trim().Replace("&nbsp;", string.Empty).Replace("\n", "$");
            string[] strAry = str.Split('$');
            string username = strAry[0].Trim();
            string time = strAry[1].Trim();
            string date = time.Split(' ')[0];

            string textStr = messageNode.InnerText;
            string htmlStr = messageNode.InnerHtml;
            string xamlStr = Html.HtmlToXaml.ConvertUserMessage(htmlStr);

            return new UserMessageItemModel
            {
                Date = date,
                Time = time,
                UserId = uid,
                Username = username,
                TextStr = textStr,
                HtmlStr = htmlStr,
                XamlStr = xamlStr,
                IsRead = isRead
            };
        }

        public async Task<UserMessageDataModel> GetUserMessageData(int userId, int limitCount)
        {
            var cts = new CancellationTokenSource();
            return await LoadUserMessageDataAsync(userId, limitCount, cts);
        }

        public async Task<UserMessageItemModel> PostUserMessage(string message, int userId)
        {
            var postData = new List<KeyValuePair<string, object>>();
            postData.Add(new KeyValuePair<string, object>("formhash", AccountService.FormHash));
            postData.Add(new KeyValuePair<string, object>("handlekey", "pmreply"));
            postData.Add(new KeyValuePair<string, object>("lastdaterange", DateTime.Now.ToString("yyyy-MM-dd")));
            postData.Add(new KeyValuePair<string, object>("message", CommonService.ReplaceFaceLabel(message)));

            string url = string.Format("http://www.hi-pda.com/forum/pm.php?action=send&uid={0}&pmsubmit=yes&_={1}", userId, DateTime.Now.Ticks.ToString("x"));
            var cts = new CancellationTokenSource();
            string resultContent = await _httpClient.PostAsync(url, postData, cts);
            if (resultContent.StartsWith(@"<?xml version=""1.0"" encoding=""gbk""?><root><![CDATA[<li id=""pm_") && resultContent.Contains(@"images/default/notice_newpm.gif"))
            {
                XmlDocument xdoc = new XmlDocument();
                xdoc.LoadXml(resultContent);
                string html = xdoc.ChildNodes[1].InnerText;

                HtmlDocument doc = new HtmlDocument();
                doc.LoadHtml(html);

                var htmlNode = doc.DocumentNode.ChildNodes[0];
                return GetUserMessageItem(htmlNode);
            }
            else if (resultContent.StartsWith(@"<?xml version=""1.0"" encoding=""gbk""?><root><![CDATA[<li class=""pm_date"">") && resultContent.Contains(@"images/default/notice_newpm.gif"))
            {
                XmlDocument xdoc = new XmlDocument();
                xdoc.LoadXml(resultContent);
                string html = xdoc.ChildNodes[1].InnerText;

                HtmlDocument doc = new HtmlDocument();
                doc.LoadHtml(html);

                var htmlNode = doc.DocumentNode.ChildNodes[1];
                return GetUserMessageItem(htmlNode);
            }

            return null;
        }
    }
}
