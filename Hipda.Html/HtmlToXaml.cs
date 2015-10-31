using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace Hipda.Html
{
    public static class HtmlToXaml
    {
        public static string Convert(int threadId, string htmlContent, int maxImageCount, ref int imageCount)
        {
            //string deviceFamily = Windows.System.Profile.AnalyticsInfo.VersionInfo.DeviceFamily;

            //if (deviceFamily.Equals("Windows.Mobile"))
            //{

            //}
            //else
            //{

            //}

            var content = new StringBuilder(htmlContent);
            content.EnsureCapacity(htmlContent.Length * 2);

            content = content.Replace("[", "&#8968;");
            content = content.Replace("]", "&#8971;");
            content = content.Replace("&nbsp;", " ");
            content = content.Replace("<strong>", string.Empty);
            content = content.Replace("</strong>", string.Empty);
            content = content.Replace("↵", "&#8629;");

            // 移除无用的图片附加信息
            MatchCollection matchsForInvalidHtml1 = new Regex(@"<div class=""t_attach"".*\n.*\n.*\n*.*").Matches(content.ToString());
            if (matchsForInvalidHtml1 != null && matchsForInvalidHtml1.Count > 0)
            {
                for (int i = 0; i < matchsForInvalidHtml1.Count; i++)
                {
                    var m = matchsForInvalidHtml1[i];

                    string placeHolder = m.Groups[0].Value; // 要被替换的元素
                    content = content.Replace(placeHolder, string.Empty);
                }
            }

            // 移除无用的图片附加信息2
            MatchCollection matchsForInvalidHtml2 = new Regex(@"<p class=""imgtitle"">\n*.*\n*.*\n*.*\n*.*\n*.*\n*.*\n*.*\n*.*\n*.*\n*.*\n*.*").Matches(content.ToString());
            if (matchsForInvalidHtml2 != null && matchsForInvalidHtml2.Count > 0)
            {
                for (int i = 0; i < matchsForInvalidHtml2.Count; i++)
                {
                    var m = matchsForInvalidHtml2[i];

                    string placeHolder = m.Groups[0].Value; // 要被替换的元素
                    content = content.Replace(placeHolder, string.Empty);
                }
            }

            // 替换站内链接为按钮
            MatchCollection matchsForMyLink = new Regex(@"<a\s+href=""http:\/\/www\.hi\-pda\.com\/forum\/viewthread\.php\?[^>]*&?tid\=(\d*)[^\""]*""[^>]*>([^<]*)</a>").Matches(content.ToString());
            if (matchsForMyLink != null && matchsForMyLink.Count > 0)
            {
                for (int i = 0; i < matchsForMyLink.Count; i++)
                {
                    var m = matchsForMyLink[i];

                    string placeHolder = m.Groups[0].Value; // 要被替换的元素
                    string threadIdStr = m.Groups[1].Value;
                    string linkContent = m.Groups[2].Value;

                    string linkXaml = string.Format(@"[InlineUIContainer][local:MyLink ThreadIdStr=""{0}"" LinkContent=""{1}""][/local:MyLink][/InlineUIContainer]", threadIdStr, linkContent);
                    content = content.Replace(placeHolder, linkXaml);
                }
            }

            // 替换链接
            MatchCollection matchsForLink = new Regex(@"<a\s+href=""([^""]*)""[^>]*>([^<]*)</a>").Matches(content.ToString());
            if (matchsForLink != null && matchsForLink.Count > 0)
            {
                for (int i = 0; i < matchsForLink.Count; i++)
                {
                    var m = matchsForLink[i];

                    string placeHolder = m.Groups[0].Value; // 要被替换的元素
                    string linkUrl = m.Groups[1].Value;
                    string linkContent = m.Groups[2].Value;

                    if (!linkUrl.StartsWith("http"))
                    {
                        linkUrl = string.Format("http://www.hi-pda.com/forum/{0}", linkUrl);
                    }
                    string linkXaml = string.Format(@"[Hyperlink NavigateUri=""{0}"" Foreground=""DodgerBlue""]{1}[/Hyperlink]", linkUrl, linkContent);
                    content = content.Replace(placeHolder, linkXaml);
                }
            }

            content = content.Replace(@"<div class=""postattachlist"">", "↵");
            content = content.Replace("<br/>", "↵"); // ↵符号表示换行符
            content = content.Replace("<br />", "↵");
            content = content.Replace("<br>", "↵");
            content = content.Replace("</div>", "↵");
            content = content.Replace("</p>", "↵");

            // 替换引用文字标签
            content = content.Replace("<blockquote>", @"[/Paragraph][Paragraph Margin=""18,0,0,0"" Foreground=""Gray""][Span]");
            content = content.Replace("</blockquote>", "[/Span][/Paragraph][Paragraph]");

            // 移除无意义图片HTML
            content = content.Replace(@"src=""images/default/attachimg.gif""", string.Empty);
            content = content.Replace(@"src=""http://www.hi-pda.com/forum/images/default/attachimg.gif""", string.Empty);

            #region 解析图片
            // 站内上载的图片，通过file属性解析
            MatchCollection matchsForImage1 = new Regex(@"<img[^>]*file=""([^""]*)""[^>]*>").Matches(content.ToString());
            if (matchsForImage1 != null && matchsForImage1.Count > 0)
            {
                for (int i = 0; i < matchsForImage1.Count; i++)
                {
                    string placeHolderLabel = matchsForImage1[i].Groups[0].Value; // 要被替换的元素
                    string imgUrl = matchsForImage1[i].Groups[1].Value; // 图片URL
                    if (!imgUrl.StartsWith("http")) imgUrl = "http://www.hi-pda.com/forum/" + imgUrl;

                    string imgXaml = @"[InlineUIContainer][local:MyImage FolderName=""{0}"" Url=""{1}""][/local:MyImage][/InlineUIContainer]";
                    imgXaml = string.Format(imgXaml, threadId, imgUrl);

                    content = content.Replace(placeHolderLabel, imgXaml);
                }
            }

            // 其他图片，通过src属性解析
            MatchCollection matchsForImage2 = new Regex(@"<img[^>]*src=""([^""]*)""[^>]*>").Matches(content.ToString());
            if (matchsForImage2 != null && matchsForImage2.Count > 0)
            {
                for (int i = 0; i < matchsForImage2.Count; i++)
                {
                    var m = matchsForImage2[i];
                    string placeHolderLabel = m.Groups[0].Value; // 要被替换的元素
                    string imgUrl = m.Groups[1].Value; // 图片URL
                    if (!imgUrl.StartsWith("http")) imgUrl = "http://www.hi-pda.com/forum/" + imgUrl;

                    string imgXaml = @"[InlineUIContainer][local:MyImage FolderName=""{0}"" Url=""{1}""][/local:MyImage][/InlineUIContainer]";
                    imgXaml = string.Format(imgXaml, threadId, imgUrl);

                    content = content.Replace(placeHolderLabel, imgXaml);
                }
            }
            #endregion

            string xamlContent = content.ToString();
            xamlContent = new Regex("<[^<]*>").Replace(xamlContent, string.Empty); // 移除所有HTML标签
            xamlContent = new Regex("\r\n").Replace(xamlContent, string.Empty); // 忽略源换行
            xamlContent = new Regex("\r").Replace(xamlContent, string.Empty); // 忽略源换行
            xamlContent = new Regex("\n").Replace(xamlContent, string.Empty); // 忽略源换行
            xamlContent = new Regex(@"↵{1,}").Replace(xamlContent, "↵"); // 将多个换行符合并成一个
            xamlContent = new Regex(@"^↵").Replace(xamlContent, string.Empty); // 移除行首的换行符
            xamlContent = new Regex(@"↵$").Replace(xamlContent, string.Empty); // 移除行末的换行符
            xamlContent = xamlContent.Replace("↵", "[LineBreak/]"); // 解析换行符
            xamlContent = xamlContent.Replace("[", "<");
            xamlContent = xamlContent.Replace("]", ">");
            xamlContent = string.Format(@"<RichTextBlock xmlns=""http://schemas.microsoft.com/winfx/2006/xaml/presentation"" xmlns:local=""using:Hipda.Client.Uwp.Pro""><Paragraph>{0}</Paragraph></RichTextBlock>", xamlContent);

            return xamlContent;
        }
    }
}
