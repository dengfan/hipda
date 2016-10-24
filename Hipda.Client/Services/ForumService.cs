using Hipda.Client.Models;
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
    public class ForumService
    {
        static List<ForumCategoryModel> _forumData = new List<ForumCategoryModel>();
        static HttpHandle _httpClient = HttpHandle.GetInstance();

        private async Task LoadForumDataAsync(CancellationTokenSource cts)
        {
            if (_forumData.Count > 0)
            {
                return;
            }

            // 读取数据
            string url = "http://www.hi-pda.com/forum/search.php";
            string htmlContent = await _httpClient.GetAsync(url, cts);

            // 实例化 HtmlAgilityPack.HtmlDocument 对象
            HtmlDocument doc = new HtmlDocument();

            // 载入HTML
            doc.LoadHtml(htmlContent);
            var data = doc.DocumentNode;

            var selectNode = data.Descendants().FirstOrDefault(n => n.Name.Equals("select") && n.GetAttributeValue("id", "").Equals("srchfid"));
            if (selectNode == null)
            {
                return;
            }

            var groups = selectNode.ChildNodes.Where(n => n.Name.Equals("optgroup"));
            if (groups == null)
            {
                return;
            }

            foreach (var group in groups)
            {
                string forumGroupName = group.Attributes[0].Value.Replace("--", string.Empty);
                var forumGroup = new ForumCategoryModel { ForumGroupName = forumGroupName };

                var groupItems = group.ChildNodes.Where(n => n.Name.Equals("option"));
                if (groupItems == null)
                {
                    return;
                }

                foreach (var item in groupItems)
                {
                    int forumId = Convert.ToInt32(item.Attributes[0].Value);
                    string forumName = item.NextSibling.InnerText;
                    forumName = forumName.Replace("&nbsp;", string.Empty);
                    forumName = forumName.Trim();

                    forumGroup.Forums.Add(new ForumModel { Id = forumId, Name = forumName });
                }

                _forumData.Add(forumGroup);
            }
        }

        public async Task<List<ForumCategoryModel>> GetForumData()
        {
            var cts = new CancellationTokenSource();
            await LoadForumDataAsync(cts);
            return _forumData;
        }
    }
}
