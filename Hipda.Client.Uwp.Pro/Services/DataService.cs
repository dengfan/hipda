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
    }
}
