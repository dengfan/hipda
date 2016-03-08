using Hipda.Client.Uwp.Pro.ViewModels;
using HtmlAgilityPack;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Hipda.Client.Uwp.Pro.Services
{
    public class PromptService
    {
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
                    promtpViewModel.PromptNoticeCountInToastTempData = ToastService.GetNoticeCountFromNoticeToastTempData();

                    ToastService.UpdateBadge(promtpViewModel.PromptAllWithoutPromptPm + promtpViewModel.PromptPm);
                }
            }
            catch (Exception e)
            {
                string errorDetails = string.Format("{0}", e.Message);
                CommonService.PostErrorEmailToDeveloper("提醒数据解析出现异常", errorDetails);
            }
        }
    }
}
