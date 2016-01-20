using Hipda.Client.Uwp.Pro.Models;
using Hipda.Client.Uwp.Pro.ViewModels;
using Hipda.Http;
using HtmlAgilityPack;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using Windows.Data.Xml.Dom;
using Windows.Storage;
using Windows.UI;
using Windows.UI.Notifications;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Media;

namespace Hipda.Client.Uwp.Pro.Services
{
    public class DataService
    {
        static HttpHandle _httpClient = HttpHandle.GetInstance();

        #region page number
        public static int GetMaxPageNo(HtmlNode pagesNode)
        {
            int maxPageNo = 1;

            try
            {
                if (pagesNode != null)
                {
                    var nodeList = pagesNode.Descendants().Where(n => n.Name.Equals("a") || n.Name.Equals("strong")).ToList();
                    nodeList.RemoveAll(n => n.InnerText.Equals("下一页"));
                    string lastPageNodeValue = nodeList.Last().InnerText.Replace("... ", string.Empty);
                    maxPageNo = Convert.ToInt32(lastPageNodeValue);
                }
            }
            catch (Exception e)
            {
                string errorDetails = string.Format("{0}", e.Message);
                Common.PostErrorEmailToDeveloper("页码数据解析出现异常", errorDetails);
            }

            return maxPageNo;
        }
        #endregion

        #region thread
        public static ObservableCollection<ThreadItemModelBase> ThreadHistoryData = new ObservableCollection<ThreadItemModelBase>();

        Style GetReadStatusStyle(int threadId)
        {
            string styleName = IsRead(threadId) ? "ReadColorStyle" : "UnReadColorStyle";
            return (Style)App.Current.Resources[styleName];
        }

        public bool IsRead(int threadId)
        {
            return ThreadHistoryData.Count(h => h.ThreadId == threadId) > 0;
        }
        #endregion

        #region user
        static Dictionary<int, string> _userInfoData = new Dictionary<int, string>();

        async Task LoadUserDataAsync(int userId, CancellationTokenSource cts)
        {
            if (_userInfoData.ContainsKey(userId))
            {
                return;
            }

            // 读取数据
            string url = string.Format("http://www.hi-pda.com/forum/space.php?uid={0}&_={1}", userId, DateTime.Now.Ticks.ToString("x"));
            string htmlContent = await _httpClient.GetAsync(url, cts);

            // 实例化 HtmlAgilityPack.HtmlDocument 对象
            HtmlDocument doc = new HtmlDocument();

            // 载入HTML
            doc.LoadHtml(htmlContent);

            // 最先读取提醒数据
            var promptContentNode = doc.DocumentNode.Descendants().FirstOrDefault(n => n.Name.Equals("div") && n.GetAttributeValue("class", "").Equals("promptcontent"));
            GetPromptData(promptContentNode);

            var node = doc.DocumentNode.Descendants().FirstOrDefault(n => n.Name.Equals("div") && n.GetAttributeValue("id", "").Equals("profilecontent"));
            if (node != null)
            {
                string xaml = Html.HtmlToXaml.ConvertUserInfo(node.InnerHtml);
                _userInfoData.Add(userId, xaml);
            }
        }

        public async Task<string> GetXamlForUserInfo(int userId)
        {
            if (!_userInfoData.ContainsKey(userId))
            {
                var cts = new CancellationTokenSource();
                await LoadUserDataAsync(userId, cts);
            }

            return _userInfoData[userId];
        }

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
            GetPromptData(promptContentNode);

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
            ClearPmToastTempData(userId);

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
            postData.Add(new KeyValuePair<string, object>("message", FaceService.FaceReplace(message)));

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

            return null;
        }
        #endregion

        #region notice
        async Task<List<NoticeItemModel>> LoadNoticeDataAsync(CancellationTokenSource cts)
        {
            var data = new List<NoticeItemModel>();

            string url = string.Format("http://www.hi-pda.com/forum/notice.php?_={0}", DateTime.Now.Ticks.ToString("x"));
            string htmlStr = await _httpClient.GetAsync(url, cts);

            // 实例化 HtmlAgilityPack.HtmlDocument 对象
            HtmlDocument doc = new HtmlDocument();

            // 载入HTML
            doc.LoadHtml(htmlStr);

            var items = doc.DocumentNode.Descendants().FirstOrDefault(n => n.Name.Equals("ul") && n.GetAttributeValue("class", "").Equals("feed"))?.ChildNodes;
            if (items == null)
            {
                return null;
            }

            foreach (var item in items)
            {
                NoticeType noticeType;
                bool isNew = false;
                string userId = string.Empty;
                string username = string.Empty;
                string actionTime = string.Empty;
                string threadId = string.Empty;
                string threadTitle = string.Empty;
                string originalText = string.Empty;
                string actionText = string.Empty;
                string repostStr = string.Empty;
                string postId = string.Empty;

                HtmlNode userLinkNode;
                HtmlNode threadLinkNode;

                var divNode = item.ChildNodes[0];
                string nodeClass = divNode.Attributes[0].Value.Trim().ToLower();
                switch (nodeClass)
                {
                    case "f_quote":
                    case "f_reply":
                        noticeType = NoticeType.QuoteOrReply;
                        userLinkNode = divNode.ChildNodes[0];
                        userId = userLinkNode.Attributes[0].Value.Substring("http://www.hi-pda.com/forum/space.php?from=notice&uid=".Length).Split('&')[0];
                        username = userLinkNode.InnerText.Trim();

                        threadLinkNode = divNode.ChildNodes[2];
                        threadId = threadLinkNode.Attributes[0].Value.Substring("http://www.hi-pda.com/forum/viewthread.php?from=notice&tid=".Length).Split('&')[0];
                        threadTitle = threadLinkNode.InnerText.Trim();

                        actionTime = divNode.ChildNodes[4].InnerText.Trim();

                        HtmlNode actionContentNode;
                        HtmlNode buttonsNode;
                        if (divNode.ChildNodes[5].Name.Equals("img"))
                        {
                            isNew = true;

                            actionContentNode = divNode.ChildNodes[7];
                            buttonsNode = divNode.ChildNodes[9];
                        }
                        else
                        {
                            actionContentNode = divNode.ChildNodes[6];
                            buttonsNode = divNode.ChildNodes[8];
                        }
                        
                        originalText = actionContentNode.ChildNodes[0].ChildNodes[1].ChildNodes[0]
                            .InnerText.Trim()
                            .Replace("\r", " ")
                            .Replace("\n", " ");
                        actionText = actionContentNode.ChildNodes[0].ChildNodes[1].ChildNodes[2]
                            .InnerText.Trim()
                            .Replace("\r", " ")
                            .Replace("\n", " ");
                        
                        var replyLinkNode = buttonsNode.ChildNodes[0];
                        repostStr = replyLinkNode.Attributes[0].Value.Substring("http://www.hi-pda.com/forum/post.php?from=notice&action=reply&fid=2&tid=1778684&reppost=".Length).Split('&')[0];
                        var viewLinkNode = buttonsNode.ChildNodes[2];
                        postId = viewLinkNode.Attributes[0].Value.Substring("http://www.hi-pda.com/forum/redirect.php?from=notice&goto=findpost&pid=".Length).Split('&')[0];

                        data.Add(new NoticeItemModel(noticeType, isNew, username, actionTime, new string[] {
                            userId,         // 0
                            threadId,       // 1
                            threadTitle,    // 2
                            originalText,   // 3
                            actionText,     // 4
                            repostStr,      // 5
                            postId          // 6
                        }));
                        break;
                    case "f_thread":
                        noticeType = NoticeType.Thread;
                        var nodes = divNode.ChildNodes;
                        var usernames = new List<string>();
                        var usernameNodes = nodes.Where(n => n.Name.Equals("a") && n.Attributes[0].Value.StartsWith("space.php?username="));
                        foreach (var n in usernameNodes)
                        {
                            usernames.Add(n.InnerText.Trim());
                        }
                        username = string.Join(",", usernames);

                        threadLinkNode = nodes.FirstOrDefault(n => n.Name.Equals("a") && n.Attributes[0].Value.StartsWith("http://www.hi-pda.com/forum/redirect.php?from=notice&goto=findpost&pid="));
                        string linkUrlStr = threadLinkNode.Attributes[0].Value.Substring("http://www.hi-pda.com/forum/redirect.php?from=notice&goto=findpost&pid=".Length).Replace("ptid=", string.Empty);
                        string[] idsAry = linkUrlStr.Split('&');
                        postId = idsAry[0];
                        threadId = idsAry[1];
                        threadTitle = threadLinkNode.InnerText.Trim();

                        actionTime = nodes.FirstOrDefault(n => n.Name.Equals("em")).InnerText.Trim();
                        isNew = nodes.Count(n => n.Name.Equals("img") && n.GetAttributeValue("alt", "").Equals("NEW")) == 1;

                        data.Add(new NoticeItemModel(noticeType, isNew, username, actionTime, new string[] {
                            threadId,
                            threadTitle,
                            postId
                        }));
                        break;
                    case "f_buddy":
                        noticeType = NoticeType.Buddy;
                        userLinkNode = divNode.ChildNodes[0];
                        userId = userLinkNode.Attributes[0].Value.Substring("http://www.hi-pda.com/forum/space.php?from=notice&uid=".Length);
                        username = userLinkNode.InnerText.Trim();
                        actionTime = divNode.ChildNodes[2].InnerText.Trim();
                        isNew = divNode.ChildNodes[3].Name.Equals("img");

                        data.Add(new NoticeItemModel(noticeType, isNew, username, actionTime, new string[] {
                            userId
                        }));
                        break;
                }
            }

            #region 还原通知数据的“NEW”状态后，清除 toast notice data
            var noticeToastTempData = GetNoticeToastTempData();
            if (noticeToastTempData != null)
            {
                foreach (var i in noticeToastTempData)
                {
                    foreach (var j in data)
                    {
                        string key = string.Format("{0}#{1}", (int)j.NoticeType, j.ActionTime);
                        if (key.Equals(i) && j.IsNew == false)
                        {
                            j.IsNew = true;
                            break;
                        }
                    }
                }
            }

            ClearNoticeToastTempData();
            #endregion

            return data;
        }

        public async Task<List<NoticeItemModel>> GetNoticeData()
        {
            var cts = new CancellationTokenSource();
            return await LoadNoticeDataAsync(cts);
        }
        #endregion

        #region pm
        static List<UserMessageListItemModel> _userMessageListData = new List<UserMessageListItemModel>();
        int _userMessageListMaxPageNo = 1;

        async Task LoadUserMessageListDataAsync(int pageNo, CancellationTokenSource cts)
        {
            if (pageNo == 1)
            {
                // 如果是新打开，则清空所有短消息列表数据
                _userMessageListData.Clear();
            }

            if (pageNo > _userMessageListMaxPageNo)
            {
                return;
            }

            // 读取数据
            string url = string.Format("http://www.hi-pda.com/forum/pm.php?filter=privatepm&page={0}&_={1}", pageNo, DateTime.Now.Ticks.ToString("x"));
            string htmlStr = await _httpClient.GetAsync(url, cts);

            // 实例化 HtmlAgilityPack.HtmlDocument 对象
            HtmlDocument doc = new HtmlDocument();

            // 载入HTML
            doc.LoadHtml(htmlStr);

            // 最先读取提醒数据
            var promptContentNode = doc.DocumentNode.Descendants().FirstOrDefault(n => n.Name.Equals("div") && n.GetAttributeValue("class", "").Equals("promptcontent"));
            GetPromptData(promptContentNode);

            var items = doc.DocumentNode.Descendants().FirstOrDefault(n => n.Name.Equals("ul") && n.GetAttributeValue("class", "").Equals("pm_list"))?.ChildNodes;
            if (items == null)
            {
                return;
            }

            // 读取最大页码
            var pagesNode = doc.DocumentNode.Descendants().FirstOrDefault(n => n.Name.Equals("div") && n.GetAttributeValue("class", "").Equals("pages"));
            _userMessageListMaxPageNo = GetMaxPageNo(pagesNode);

            int i = _userMessageListData.Count;
            foreach (var item in items)
            {
                var isNew = item.ChildNodes[3].ChildNodes.Count >= 4 && item.ChildNodes[3].ChildNodes[3].Name.Equals("img");
                var linkNode = item.ChildNodes[3].ChildNodes[1].ChildNodes[0];
                int userId = Convert.ToInt32(linkNode.Attributes[0].Value.Substring("space.php?uid=".Length).Split('&')[0]);
                string username = linkNode.InnerText.Trim();
                string lastMessageTime = item.ChildNodes[3].ChildNodes[2].InnerText.Trim().Replace("&nbsp;", string.Empty);
                string lastMessageText = item.ChildNodes[5].InnerText.Trim();

                var userMessageListItem = new UserMessageListItemModel(i, isNew, pageNo, userId, username, lastMessageTime, lastMessageText);
                _userMessageListData.Add(userMessageListItem);

                i++;
            }
        }

        public async Task<int> LoadMoreUserMessageListAsync(int pageNo, Action afterLoaded)
        {
            var cts = new CancellationTokenSource();
            await LoadUserMessageListDataAsync(pageNo, cts);

            if (afterLoaded != null) afterLoaded();

            return _userMessageListData.Count;
        }

        public static UserMessageListItemModel GetUserMessageListItemByIndex(int index)
        {
            return _userMessageListData.FirstOrDefault(li => li.Index == index);
        }

        public int GetUserMessageListMaxPageNo()
        {
            return _userMessageListMaxPageNo;
        }

        public ICollectionView GetViewForUserMessageList(int startPageNo, Action afterLoaded)
        {
            var cvs = new CollectionViewSource();
            cvs.Source = new GeneratorIncrementalLoadingClass<UserMessageListItemModel>(
                startPageNo,
                async pageNo =>
                {
                    // 加载分页数据，并写入静态类中
                    // 返回的是本次加载的数据量
                    return await LoadMoreUserMessageListAsync(pageNo, afterLoaded);
                },
                (index) =>
                {
                    // 从静态类中返回需要显示出来的数据
                    return GetUserMessageListItemByIndex(index);
                },
                () =>
                {
                    return GetUserMessageListMaxPageNo();
                });

            return cvs.View;
        }

        public void ClearUserMessageListData()
        {
            _userMessageListData.Clear();
        }

        async Task<bool> DeleteUserMessageListItemAsync(List<int> delUserIdList, CancellationTokenSource cts)
        {
            var postData = new List<KeyValuePair<string, object>>();
            postData.Add(new KeyValuePair<string, object>("readopt", "0"));
            foreach (int userId in delUserIdList)
            {
                postData.Add(new KeyValuePair<string, object>("uid[]", userId.ToString()));
            }

            // 读取数据
            string url = string.Format("http://www.hi-pda.com/forum/pm.php?action=del&filter=privatepm&page=1&_={0}", DateTime.Now.Ticks.ToString("x"));
            string resultContent = await _httpClient.PostAsync(url, postData, cts);
            if (resultContent.StartsWith("<!DOCTYPE html"))
            {
                return true;
            }

            return false;
        }

        public async Task<bool> DeleteUserMessageListItem(List<int> delUserIdList)
        {
            var cts = new CancellationTokenSource();
            return await DeleteUserMessageListItemAsync(delUserIdList, cts);
        }
        #endregion

        #region toast temp data
        List<string> GetNoticeToastTempData()
        {
            string _containerKey = "HIPDA";
            string _dataKey = "NoticeToastTempData";
            var _container = ApplicationData.Current.LocalSettings.CreateContainer(_containerKey, ApplicationDataCreateDisposition.Always);
            if (_container.Values.ContainsKey(_dataKey))
            {
                return _container.Values[_dataKey].ToString().Split(',').ToList();
            }

            return null;
        }

        void ClearNoticeToastTempData()
        {
            string _containerKey = "HIPDA";
            string _dataKey = "NoticeToastTempData";
            var _container = ApplicationData.Current.LocalSettings.CreateContainer(_containerKey, ApplicationDataCreateDisposition.Always);
            _container.Values.Remove(_dataKey);

            UpdateBadge();
        }

        void ClearPmToastTempData(int userId)
        {
            string _containerKey = "HIPDA";
            string _dataKey = "PmToastTempData";
            var _container = ApplicationData.Current.LocalSettings.CreateContainer(_containerKey, ApplicationDataCreateDisposition.Always);
            string value = _container.Values[_dataKey]?.ToString();
            if (!string.IsNullOrEmpty(value))
            {
                var list = value.Split(',').ToList();
                list.RemoveAll(u => u.Equals(userId.ToString()));
                _container.Values[_dataKey] = string.Join(",", list);

                UpdateBadge();
            }
        }

        static int GetNoticeCountFromNoticeToastTempData()
        {
            string _containerKey = "HIPDA";
            string _dataKey = "NoticeToastTempData";
            var _container = ApplicationData.Current.LocalSettings.CreateContainer(_containerKey, ApplicationDataCreateDisposition.Always);
            if (_container.Values.ContainsKey(_dataKey))
            {
                return _container.Values[_dataKey].ToString().Split(',').ToList().Count(i => i.Length > 0);
            }

            return 0;
        }

        static int GetPmCountFromPmToastTempData()
        {
            string _containerKey = "HIPDA";
            string _dataKey = "PmToastTempData";
            var _container = ApplicationData.Current.LocalSettings.CreateContainer(_containerKey, ApplicationDataCreateDisposition.Always);
            if (_container.Values.ContainsKey(_dataKey))
            {
                return _container.Values[_dataKey].ToString().Split(',').ToList().Count(i => i.Length > 0);
            }

            return 0;
        }

        static void UpdateBadge(int count)
        {
            Debug.WriteLine("更新 badge 数量 开始");
            XmlDocument badgeXml = BadgeUpdateManager.GetTemplateContent(BadgeTemplateType.BadgeNumber);
            XmlElement badgeElement = (XmlElement)badgeXml.SelectSingleNode("/badge");
            badgeElement.SetAttribute("value", count.ToString());
            BadgeNotification badgeNotification = new BadgeNotification(badgeXml);
            BadgeUpdater badgeUpdater = BadgeUpdateManager.CreateBadgeUpdaterForApplication();
            badgeUpdater.Update(badgeNotification);
            Debug.WriteLine(string.Format("更新 badge 数量 {0} 结束", count));
        }

        static void UpdateBadge()
        {
            Debug.WriteLine("更新 badge 数量 开始");
            int count = GetNoticeCountFromNoticeToastTempData();
            count += GetPmCountFromPmToastTempData();

            UpdateBadge(count);
        }
        #endregion

        #region prompt
        public static void GetPromptData(HtmlNode promptContentNode)
        {
            try
            {
                if (promptContentNode != null)
                {
                    var promtpViewModel = MainPageViewModel.GetInstance();
                    var ulNode = promptContentNode.ChildNodes[1];
                    promtpViewModel.PromptPm = Convert.ToInt32(ulNode.ChildNodes[0].InnerText.Trim().Substring("私人消息 (".Length).Replace(")", string.Empty));
                    promtpViewModel.PromptAnnouncePm = Convert.ToInt32(ulNode.ChildNodes[1].InnerText.Trim().Substring("公共消息 (".Length).Replace(")", string.Empty));
                    promtpViewModel.PromptSystemPm = Convert.ToInt32(ulNode.ChildNodes[2].InnerText.Trim().Substring("系统消息 (".Length).Replace(")", string.Empty));
                    promtpViewModel.PromptFriend = Convert.ToInt32(ulNode.ChildNodes[3].InnerText.Trim().Substring("好友消息 (".Length).Replace(")", string.Empty));
                    promtpViewModel.PromptThreads = Convert.ToInt32(ulNode.ChildNodes[4].InnerText.Trim().Substring("帖子消息 (".Length).Replace(")", string.Empty));
                    promtpViewModel.PromptNoticeCountInToastTempData = GetNoticeCountFromNoticeToastTempData();

                    if (promtpViewModel.PromptAllWithoutPromptPm + promtpViewModel.PromptPm > 0)
                    {
                        UpdateBadge(promtpViewModel.PromptAllWithoutPromptPm + promtpViewModel.PromptPm);
                    }
                }
            }
            catch (Exception e)
            {
                string errorDetails = string.Format("{0}", e.Message);
                Common.PostErrorEmailToDeveloper("提醒数据解析出现异常", errorDetails);
            }
        }
        #endregion
    }
}
