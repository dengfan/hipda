using Hipda.Http;
using HtmlAgilityPack;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Hipda.Client.Services
{
    public class UserInfoService
    {
        static HttpHandle _httpClient = HttpHandle.GetInstance();
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
            PromptService.GetPromptData(promptContentNode);

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
    }
}
