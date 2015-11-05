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
        public static string Convert(int threadId, string htmlContent, int maxImageCount, ref int imageCount, ref int linkCount)
        {
            //string deviceFamily = Windows.System.Profile.AnalyticsInfo.VersionInfo.DeviceFamily;

            //if (deviceFamily.Equals("Windows.Mobile"))
            //{

            //}
            //else
            //{

            //}

            htmlContent = htmlContent.Replace((string)"[", (string)"&#8968;");
            htmlContent = htmlContent.Replace((string)"]", (string)"&#8971;");
            htmlContent = htmlContent.Replace((string)"&nbsp;", (string)" ");
            htmlContent = htmlContent.Replace((string)"<strong>", (string)string.Empty);
            htmlContent = htmlContent.Replace((string)"</strong>", (string)string.Empty);
            htmlContent = htmlContent.Replace((string)"↵", (string)"&#8629;");

            // 移除无用的图片附加信息
            MatchCollection matchsForInvalidHtml1 = new Regex(@"<div class=""t_attach"".*\n.*\n.*\n*.*").Matches(htmlContent);
            if (matchsForInvalidHtml1 != null && matchsForInvalidHtml1.Count > 0)
            {
                for (int i = 0; i < matchsForInvalidHtml1.Count; i++)
                {
                    var m = matchsForInvalidHtml1[i];

                    string placeHolder = m.Groups[0].Value; // 要被替换的元素
                    htmlContent = htmlContent.Replace((string)placeHolder, (string)string.Empty);
                }
            }

            // 移除无用的图片附加信息2
            MatchCollection matchsForInvalidHtml2 = new Regex(@"<p class=""imgtitle"">\n*.*\n*.*\n*.*\n*.*\n*.*\n*.*\n*.*\n*.*\n*.*\n*.*\n*.*").Matches(htmlContent);
            if (matchsForInvalidHtml2 != null && matchsForInvalidHtml2.Count > 0)
            {
                for (int i = 0; i < matchsForInvalidHtml2.Count; i++)
                {
                    var m = matchsForInvalidHtml2[i];

                    string placeHolder = m.Groups[0].Value; // 要被替换的元素
                    htmlContent = htmlContent.Replace((string)placeHolder, (string)string.Empty);
                }
            }

            // 替换站内链接为按钮
            linkCount = 0;
            MatchCollection matchsForMyLink = new Regex(@"<a\s+href=""http:\/\/www\.hi\-pda\.com\/forum\/viewthread\.php\?[^>]*&?tid\=(\d*)[^\""]*""[^>]*>(.*)</a>").Matches(htmlContent);
            if (matchsForMyLink != null && matchsForMyLink.Count > 0)
            {
                linkCount = matchsForMyLink.Count;
                for (int i = 0; i < linkCount; i++)
                {
                    var m = matchsForMyLink[i];

                    string placeHolder = m.Groups[0].Value; // 要被替换的元素
                    string threadIdStr = m.Groups[1].Value;
                    string linkContent = m.Groups[2].Value;
                    linkContent = Regex.Replace(linkContent, @"<[^>]*>", string.Empty);

                    string linkXaml = string.Format(@"[InlineUIContainer][local:MyLink Name=""MyLink_{2}"" ThreadId=""{0}"" LinkContent=""{1}""/][/InlineUIContainer]", threadIdStr, linkContent, i);
                    string regexPattern = StringToRegexPattern(placeHolder);
                    htmlContent = new Regex(regexPattern).Replace(htmlContent, linkXaml, 1); // 由于站内链接有可能重复，所以这里每次只允许替换一个
                }
            }

            // 替换链接
            MatchCollection matchsForLink = new Regex(@"<a\s+href=""([^""]*)""[^>]*>([^<]*)</a>").Matches(htmlContent);
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
                    htmlContent = htmlContent.Replace((string)placeHolder, (string)linkXaml);
                }
            }

            htmlContent = htmlContent.Replace((string)@"<div class=""postattachlist"">", (string)"↵");
            htmlContent = htmlContent.Replace((string)"<br/>", (string)"↵"); // ↵符号表示换行符
            htmlContent = htmlContent.Replace((string)"<br />", (string)"↵");
            htmlContent = htmlContent.Replace((string)"<br>", (string)"↵");
            htmlContent = htmlContent.Replace((string)"</div>", (string)"↵");
            htmlContent = htmlContent.Replace((string)"</p>", (string)"↵");

            // 替换引用文字标签
            htmlContent = htmlContent.Replace((string)"<blockquote>", (string)@"[/Paragraph][Paragraph Margin=""18,0,0,0"" Foreground=""Gray""][Span]");
            htmlContent = htmlContent.Replace((string)"</blockquote>", (string)"[/Span][/Paragraph][Paragraph]");

            // 移除无意义图片HTML
            htmlContent = htmlContent.Replace((string)@"src=""images/default/attachimg.gif""", (string)string.Empty);
            htmlContent = htmlContent.Replace((string)@"src=""http://www.hi-pda.com/forum/images/default/attachimg.gif""", (string)string.Empty);

            #region 解析图片
            // 站内上载的图片，通过file属性解析
            MatchCollection matchsForImage1 = new Regex(@"<img[^>]*file=""([^""]*)""[^>]*>").Matches(htmlContent);
            if (matchsForImage1 != null && matchsForImage1.Count > 0)
            {
                for (int i = 0; i < matchsForImage1.Count; i++)
                {
                    string placeHolderLabel = matchsForImage1[i].Groups[0].Value; // 要被替换的元素
                    string imgUrl = matchsForImage1[i].Groups[1].Value; // 图片URL
                    if (!imgUrl.StartsWith("http")) imgUrl = "http://www.hi-pda.com/forum/" + imgUrl;

                    string imgXaml = @"[InlineUIContainer][local:MyImage FolderName=""{0}"" Url=""{1}""][/local:MyImage][/InlineUIContainer]";
                    imgXaml = string.Format(imgXaml, threadId, imgUrl);

                    htmlContent = htmlContent.Replace((string)placeHolderLabel, (string)imgXaml);
                }
            }

            // 其他图片，通过src属性解析
            MatchCollection matchsForImage2 = new Regex(@"<img[^>]*src=""([^""]*)""[^>]*>").Matches(htmlContent);
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

                    htmlContent = htmlContent.Replace((string)placeHolderLabel, (string)imgXaml);
                }
            }
            #endregion

            htmlContent = new Regex("<[^>]*>").Replace(htmlContent, string.Empty); // 移除所有HTML标签
            htmlContent = new Regex("\r\n").Replace(htmlContent, string.Empty); // 忽略源换行
            htmlContent = new Regex("\r").Replace(htmlContent, string.Empty); // 忽略源换行
            htmlContent = new Regex("\n").Replace(htmlContent, string.Empty); // 忽略源换行
            htmlContent = new Regex(@"↵{1,}").Replace(htmlContent, "↵"); // 将多个换行符合并成一个
            htmlContent = new Regex(@"^↵").Replace(htmlContent, string.Empty); // 移除行首的换行符
            htmlContent = new Regex(@"↵$").Replace(htmlContent, string.Empty); // 移除行末的换行符
            htmlContent = htmlContent.Replace("↵", "[LineBreak/]"); // 解析换行符
            htmlContent = htmlContent.Replace("[", "<");
            htmlContent = htmlContent.Replace("]", ">");
            htmlContent = string.Format(@"<RichTextBlock xmlns=""http://schemas.microsoft.com/winfx/2006/xaml/presentation"" xmlns:local=""using:Hipda.Client.Uwp.Pro""><Paragraph>{0}</Paragraph></RichTextBlock>", htmlContent);

            return htmlContent;
        }

        /// <summary>
        /// 将普通字符串转换为正则表达式字符串
        /// </summary>
        /// <param name="str">普通字符串</param>
        /// <returns>正则表达式字符串</returns>
        private static string StringToRegexPattern(string str)
        {
            return str
                .Replace("^", @"\^")
                .Replace("$", @"\$")
                .Replace("|", @"\|")
                .Replace("*", @"\*")
                .Replace("+", @"\+")
                .Replace("?", @"\?")
                .Replace("[", @"\[")
                .Replace("]", @"\]")
                .Replace("(", @"\(")
                .Replace(")", @"\)");
        }
    }
}
