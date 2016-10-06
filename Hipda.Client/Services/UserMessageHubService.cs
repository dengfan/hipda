using Hipda.Client.Models;
using Hipda.Http;
using HtmlAgilityPack;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Windows.UI.Xaml.Data;

namespace Hipda.Client.Services
{
    public class UserMessageHubService
    {
        static HttpHandle _httpClient = HttpHandle.GetInstance();

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
            PromptService.GetPromptData(promptContentNode);

            var items = doc.DocumentNode.Descendants().FirstOrDefault(n => n.Name.Equals("ul") && n.GetAttributeValue("class", "").Equals("pm_list"))?.ChildNodes;
            if (items == null)
            {
                return;
            }

            // 读取最大页码
            var pagesNode = doc.DocumentNode.Descendants().FirstOrDefault(n => n.Name.Equals("div") && n.GetAttributeValue("class", "").Equals("pages"));
            _userMessageListMaxPageNo = CommonService.GetMaxPageNo(pagesNode);

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

        public ICollectionView GetViewForUserMessageList(int startPageNo, Action afterLoaded, Action loadAllFinish)
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
    }
}
