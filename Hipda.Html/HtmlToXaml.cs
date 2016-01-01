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
        public static string ConvertPost(int threadId, string htmlContent, int maxImageCount, ref int imageCount)
        {
            //string deviceFamily = Windows.System.Profile.AnalyticsInfo.VersionInfo.DeviceFamily;

            //if (deviceFamily.Equals("Windows.Mobile"))
            //{

            //}
            //else
            //{

            //}

            htmlContent = htmlContent.Replace("[", "&#8968;");
            htmlContent = htmlContent.Replace("]", "&#8971;");
            htmlContent = htmlContent.Replace("&nbsp;", " ");
            htmlContent = htmlContent.Replace("↵", "&#8629;");
            htmlContent = htmlContent.Replace("<strong>", string.Empty);
            htmlContent = htmlContent.Replace("</strong>", string.Empty);
            htmlContent = htmlContent.Replace(@"<div class=""postattachlist"">", "↵");
            htmlContent = htmlContent.Replace("<br/>", "↵"); // ↵符号表示换行符
            htmlContent = htmlContent.Replace("<br />", "↵");
            htmlContent = htmlContent.Replace("<br>", "↵");
            htmlContent = htmlContent.Replace("</div>", "↵");
            htmlContent = htmlContent.Replace("</p>", "↵");
            htmlContent = htmlContent.Replace("</ul>", "↵");
            htmlContent = htmlContent.Replace("</li>", "↵");
            htmlContent = htmlContent.Replace("</td>", "↵");
            htmlContent = Regex.Replace(htmlContent, @"<span[^>]*>", string.Empty);
            htmlContent = Regex.Replace(htmlContent, @"</span>", string.Empty);

            // 移除无用的图片附加信息
            MatchCollection matchsForInvalidHtml1 = new Regex(@"<div class=""t_attach"".*\n.*\n.*\n*.*").Matches(htmlContent);
            if (matchsForInvalidHtml1 != null && matchsForInvalidHtml1.Count > 0)
            {
                for (int i = 0; i < matchsForInvalidHtml1.Count; i++)
                {
                    var m = matchsForInvalidHtml1[i];

                    string placeHolder = m.Groups[0].Value; // 要被替换的元素
                    htmlContent = htmlContent.Replace(placeHolder, string.Empty);
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
                    htmlContent = htmlContent.Replace(placeHolder, string.Empty);
                }
            }

            // 替换引用链接为按钮
            MatchCollection matchsForRefLink = new Regex(@"<a\s+href=""http:\/\/www\.hi\-pda\.com\/forum\/redirect\.php\?goto\=findpost&amp;pid\=(\d*)&amp;ptid\=(\d*)[^\""]*""[^>]*>(<img\s[^>]*>|\d+#)</a>").Matches(htmlContent);
            if (matchsForRefLink != null && matchsForRefLink.Count > 0)
            {
                for (int i = 0; i < matchsForRefLink.Count; i++)
                {
                    var m = matchsForRefLink[i];

                    string placeHolder = m.Groups[0].Value; // 要被替换的元素
                    string postIdStr = m.Groups[1].Value;
                    string threadIdStr = m.Groups[2].Value;
                    string linkContent = m.Groups[3].Value;

                    string linkXaml = string.Empty;
                    if (linkContent.StartsWith("<img"))
                    {
                        linkXaml = string.Format(@"[InlineUIContainer][local:MyRefLink2 PostId=""{0}"" ThreadId=""{1}""/][/InlineUIContainer]", postIdStr, threadIdStr);
                    }
                    else
                    {
                        linkXaml = string.Format(@"[InlineUIContainer][local:MyRefLink1 PostId=""{0}"" ThreadId=""{1}"" LinkContent=""{2}""/][/InlineUIContainer]", postIdStr, threadIdStr, linkContent);
                    }

                    htmlContent = htmlContent.Replace(placeHolder, linkXaml);
                }
            }

            // 替换站内链接为按钮
            MatchCollection matchsForMyLink = new Regex(@"<a\s+href=""http:\/\/www\.hi\-pda\.com\/forum\/viewthread\.php\?[^>]*&?tid\=(\d*)[^\""]*""[^>]*>(.*)</a>").Matches(htmlContent);
            if (matchsForMyLink != null && matchsForMyLink.Count > 0)
            {
                for (int i = 0; i < matchsForMyLink.Count; i++)
                {
                    var m = matchsForMyLink[i];

                    string placeHolder = m.Groups[0].Value; // 要被替换的元素
                    string threadIdStr = m.Groups[1].Value;
                    string linkContent = m.Groups[2].Value;
                    linkContent = Regex.Replace(linkContent, @"<[^>]*>", string.Empty);

                    string linkXaml = string.Format(@"[InlineUIContainer][local:MyLink ThreadId=""{0}"" LinkContent=""{1}""/][/InlineUIContainer]", threadIdStr, linkContent);
                    htmlContent = htmlContent.Replace(placeHolder, linkXaml);
                }
            }

            // 替换链接
            MatchCollection matchsForLink = new Regex(@"<a\s+href=""([^""]*)""[^>]*>([^<#]*)</a>").Matches(htmlContent);
            if (matchsForLink != null && matchsForLink.Count > 0)
            {
                for (int i = 0; i < matchsForLink.Count; i++)
                {
                    var m = matchsForLink[i];

                    string placeHolder = m.Groups[0].Value; // 要被替换的元素
                    string linkUrl = m.Groups[1].Value;
                    string linkContent = m.Groups[2].Value;

                    if (!linkUrl.Contains(":"))
                    {
                        linkUrl = string.Format("http://www.hi-pda.com/forum/{0}", linkUrl);
                    }
                    string linkXaml = string.Format(@"[Hyperlink NavigateUri=""{0}"" Foreground=""DodgerBlue""]{1}[/Hyperlink]", linkUrl, linkContent);
                    htmlContent = htmlContent.Replace(placeHolder, linkXaml);
                }
            }

            // 替换小字
            MatchCollection matchsForSmallFont = new Regex(@"<font size=""1"">([^<]*)</font>").Matches(htmlContent);
            if (matchsForSmallFont != null && matchsForSmallFont.Count > 0)
            {
                for (int i = 0; i < matchsForSmallFont.Count; i++)
                {
                    var m = matchsForSmallFont[i];

                    string placeHolder = m.Groups[0].Value; // 要被替换的元素
                    string fontText = m.Groups[1].Value;
                    
                    string fontXaml = string.Format(@"[Span FontSize=""{{ThemeResource ToolTipContentThemeFontSize}}""]{0}[/Span]", fontText);
                    htmlContent = htmlContent.Replace(placeHolder, fontXaml);
                }
            }

            // 替换带颜色的font标签
            MatchCollection matchsForColorText = new Regex(@"<font color=""([#0-9a-zA-Z]*)"">([^<]*)</font>").Matches(htmlContent);
            if (matchsForColorText != null && matchsForColorText.Count > 0)
            {
                for (int i = 0; i < matchsForColorText.Count; i++)
                {
                    var m = matchsForColorText[i];

                    string placeHolder = m.Groups[0].Value; // 要被替换的元素
                    string colorName = m.Groups[1].Value.ToLower().Trim();
                    string textContent = m.Groups[2].Value;

                    string infoXaml = string.Format(@"[Span Foreground=""{1}""]{0}[/Span]", textContent, colorName);
                    if (colorName.Equals("#000") || colorName.Equals("#000000") || colorName.Equals("black") || (colorName.StartsWith("#") && colorName.Length != 4 && colorName.Length != 7))
                    {
                        infoXaml = string.Format(@"[Span]{0}[/Span]", textContent);
                    }
                    htmlContent = htmlContent.Replace(placeHolder, infoXaml);
                }
            }

            // 替换"最后编辑时间"
            MatchCollection matchsForLastEditInfo = new Regex(@"<i class=""pstatus"">(.*)</i>").Matches(htmlContent);
            if (matchsForLastEditInfo != null && matchsForLastEditInfo.Count > 0)
            {
                for (int i = 0; i < matchsForLastEditInfo.Count; i++)
                {
                    var m = matchsForLastEditInfo[i];

                    string placeHolder = m.Groups[0].Value; // 要被替换的元素
                    string infoContent = m.Groups[1].Value.Trim();

                    string infoXaml = string.Format(@"[Run Text=""{0}"" Foreground=""DimGray"" FontSize=""12""/][LineBreak/]", infoContent);
                    htmlContent = htmlContent.Replace(placeHolder, infoXaml);
                }
            }

            // 替换引用文字标签
            htmlContent = htmlContent.Replace("<blockquote>", @"[/Paragraph][Paragraph Margin=""20,0,20,0"" Foreground=""DimGray""][Span FontStyle=""Italic""]");
            htmlContent = htmlContent.Replace("</blockquote>", "[/Span][/Paragraph][Paragraph]");

            // 移除无意义图片HTML
            htmlContent = htmlContent.Replace(@"src=""images/default/attachimg.gif""", string.Empty);
            htmlContent = htmlContent.Replace(@"src=""http://www.hi-pda.com/forum/images/default/attachimg.gif""", string.Empty);

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

                    string imgXaml = @"[InlineUIContainer][local:MyImage FolderName=""{0}"" Url=""{1}""/][/InlineUIContainer]";
                    imgXaml = string.Format(imgXaml, threadId, imgUrl);

                    htmlContent = htmlContent.Replace(placeHolderLabel, imgXaml);
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

                    string imgXaml = @"[InlineUIContainer][local:MyImage FolderName=""{0}"" Url=""{1}""/][/InlineUIContainer]";
                    imgXaml = string.Format(imgXaml, threadId, imgUrl);

                    htmlContent = htmlContent.Replace(placeHolderLabel, imgXaml);
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

        public static string ConvertUserInfo(string htmlContent)
        {
            htmlContent = htmlContent.Replace("[", "&#8968;");
            htmlContent = htmlContent.Replace("]", "&#8971;");
            htmlContent = htmlContent.Replace("&nbsp;", " ");
            htmlContent = htmlContent.Replace("↵", "&#8629;");
            htmlContent = htmlContent.Replace("<strong>", string.Empty);
            htmlContent = htmlContent.Replace("</strong>", string.Empty);
            htmlContent = htmlContent.Replace("<br/>", "↵"); // ↵符号表示换行符
            htmlContent = htmlContent.Replace("<br />", "↵");
            htmlContent = htmlContent.Replace("<br>", "↵");
            htmlContent = htmlContent.Replace("</div>", "↵");
            htmlContent = htmlContent.Replace("</p>", "↵");
            htmlContent = htmlContent.Replace("</ul>", "↵");
            htmlContent = htmlContent.Replace("</li>", "↵");
            htmlContent = htmlContent.Replace("</td>", "↵");
            htmlContent = Regex.Replace(htmlContent, @"<span[^>]*>", string.Empty);
            htmlContent = Regex.Replace(htmlContent, @"</span>", string.Empty);
            htmlContent = htmlContent.Replace(@"<h3 class=""blocktitle lightlink"">", @"[LineBreak/][Bold Foreground=""DimGray""]");
            htmlContent = htmlContent.Replace("</h3>", "[/Bold][LineBreak/]");

            // 替换链接
            MatchCollection matchsForLink = new Regex(@"<a\s+href=""([^""]*)""[^>]*>([^<#]*)</a>").Matches(htmlContent);
            if (matchsForLink != null && matchsForLink.Count > 0)
            {
                for (int i = 0; i < matchsForLink.Count; i++)
                {
                    var m = matchsForLink[i];

                    string placeHolder = m.Groups[0].Value; // 要被替换的元素
                    string linkUrl = m.Groups[1].Value;
                    string linkContent = m.Groups[2].Value;

                    if (!linkUrl.Contains(":"))
                    {
                        linkUrl = string.Format("http://www.hi-pda.com/forum/{0}", linkUrl);
                    }
                    string linkXaml = string.Format(@"[Hyperlink NavigateUri=""{0}"" Foreground=""DodgerBlue""]{1}[/Hyperlink]", linkUrl, linkContent);
                    htmlContent = htmlContent.Replace(placeHolder, linkXaml);
                }
            }

            // 移除无意义图片HTML
            htmlContent = htmlContent.Replace(@"src=""images/default/attachimg.gif""", string.Empty);
            htmlContent = htmlContent.Replace(@"src=""http://www.hi-pda.com/forum/images/default/attachimg.gif""", string.Empty);

            #region 解析图片
            // 图片，通过src属性解析
            MatchCollection matchsForImage = new Regex(@"<img[^>]*src=""([^""]*)""[^>]*>").Matches(htmlContent);
            if (matchsForImage != null && matchsForImage.Count > 0)
            {
                for (int i = 0; i < matchsForImage.Count; i++)
                {
                    var m = matchsForImage[i];
                    string placeHolderLabel = m.Groups[0].Value; // 要被替换的元素
                    string imgUrl = m.Groups[1].Value; // 图片URL
                    if (!imgUrl.StartsWith("http")) imgUrl = "http://www.hi-pda.com/forum/" + imgUrl;

                    string imgXaml = @"[InlineUIContainer][local:MyImage FolderName=""0"" Url=""{0}""/][/InlineUIContainer]";
                    imgXaml = string.Format(imgXaml, imgUrl);

                    htmlContent = htmlContent.Replace(placeHolderLabel, imgXaml);
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

        public static string ConvertUserMessage(string htmlContent)
        {
            htmlContent = htmlContent.Replace("[", "&#8968;");
            htmlContent = htmlContent.Replace("]", "&#8971;");
            htmlContent = htmlContent.Replace("&nbsp;", " ");
            htmlContent = htmlContent.Replace("↵", "&#8629;");
            htmlContent = htmlContent.Replace("<strong>", string.Empty);
            htmlContent = htmlContent.Replace("</strong>", string.Empty);
            htmlContent = htmlContent.Replace("<br/>", "↵"); // ↵符号表示换行符
            htmlContent = htmlContent.Replace("<br />", "↵");
            htmlContent = htmlContent.Replace("<br>", "↵");
            htmlContent = htmlContent.Replace("</div>", "↵");
            htmlContent = htmlContent.Replace("</p>", "↵");
            htmlContent = htmlContent.Replace("</ul>", "↵");
            htmlContent = htmlContent.Replace("</li>", "↵");
            htmlContent = htmlContent.Replace("</td>", "↵");
            htmlContent = Regex.Replace(htmlContent, @"<span[^>]*>", string.Empty);
            htmlContent = Regex.Replace(htmlContent, @"</span>", string.Empty);

            // 替换站内链接为按钮
            MatchCollection matchsForMyLink = new Regex(@"<a\s+href=""http:\/\/www\.hi\-pda\.com\/forum\/viewthread\.php\?[^>]*&?tid\=(\d*)[^\""]*""[^>]*>(.*)</a>").Matches(htmlContent);
            if (matchsForMyLink != null && matchsForMyLink.Count > 0)
            {
                for (int i = 0; i < matchsForMyLink.Count; i++)
                {
                    var m = matchsForMyLink[i];

                    string placeHolder = m.Groups[0].Value; // 要被替换的元素
                    string threadIdStr = m.Groups[1].Value;
                    string linkContent = m.Groups[2].Value;
                    linkContent = Regex.Replace(linkContent, @"<[^>]*>", string.Empty);

                    string linkXaml = string.Format(@"[InlineUIContainer][local:MyLink ThreadId=""{0}"" LinkContent=""{1}""/][/InlineUIContainer]", threadIdStr, linkContent);
                    htmlContent = htmlContent.Replace(placeHolder, linkXaml);
                }
            }

            // 替换链接
            MatchCollection matchsForLink = new Regex(@"<a\s+href=""([^""]*)""[^>]*>([^<#]*)</a>").Matches(htmlContent);
            if (matchsForLink != null && matchsForLink.Count > 0)
            {
                for (int i = 0; i < matchsForLink.Count; i++)
                {
                    var m = matchsForLink[i];

                    string placeHolder = m.Groups[0].Value; // 要被替换的元素
                    string linkUrl = m.Groups[1].Value;
                    string linkContent = m.Groups[2].Value;

                    if (!linkUrl.Contains(":"))
                    {
                        linkUrl = string.Format("http://www.hi-pda.com/forum/{0}", linkUrl);
                    }
                    string linkXaml = string.Format(@"[Hyperlink NavigateUri=""{0}"" Foreground=""DodgerBlue""]{1}[/Hyperlink]", linkUrl, linkContent);
                    htmlContent = htmlContent.Replace(placeHolder, linkXaml);
                }
            }

            #region 解析图片
            // 图片，通过src属性解析
            MatchCollection matchsForImage2 = new Regex(@"<img[^>]*src=""([^""]*)""[^>]*>").Matches(htmlContent);
            if (matchsForImage2 != null && matchsForImage2.Count > 0)
            {
                for (int i = 0; i < matchsForImage2.Count; i++)
                {
                    var m = matchsForImage2[i];
                    string placeHolderLabel = m.Groups[0].Value; // 要被替换的元素
                    string imgUrl = m.Groups[1].Value; // 图片URL
                    if (!imgUrl.StartsWith("http")) imgUrl = "http://www.hi-pda.com/forum/" + imgUrl;

                    string imgXaml = @"[InlineUIContainer][local:MyImage FolderName=""0"" Url=""{0}""/][/InlineUIContainer]";
                    imgXaml = string.Format(imgXaml, imgUrl);

                    htmlContent = htmlContent.Replace(placeHolderLabel, imgXaml);
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

        public static string ConvertSearchThreadTitle(string title, string imageFontIcon, string fileFontIcon, string viewInfo)
        {
            string xaml = @"<TextBlock xmlns=""http://schemas.microsoft.com/winfx/2006/xaml/presentation"" Foreground=""{{ThemeResource SystemControlForegroundBaseMediumBrush}}"" TextWrapping=""Wrap""><Run FontFamily=""Segoe MDL2 Assets"" Foreground=""OrangeRed"" Text=""{1}"" /><Run FontFamily=""Segoe MDL2 Assets"" Foreground=""DeepSkyBlue"" Text=""{2}"" /> {0} <Run Text=""{3}"" Foreground=""{{ThemeResource SystemControlBackgroundAccentBrush}}"" /></TextBlock>";

            MatchCollection matchsForSearchKeywords = new Regex(@"<em style=""color:red;"">([^>#]*)</em>").Matches(title);
            if (matchsForSearchKeywords != null && matchsForSearchKeywords.Count > 0)
            {
                for (int j = 0; j < matchsForSearchKeywords.Count; j++)
                {
                    var m = matchsForSearchKeywords[j];

                    string placeHolder = m.Groups[0].Value; // 要被替换的元素
                    string k = m.Groups[1].Value;

                    string linkXaml = string.Format(@"<Run Foreground=""Red"">{0}</Run>", k);
                    title = title.Replace(placeHolder, linkXaml);
                }
            }

            return string.Format(xaml, title, imageFontIcon, fileFontIcon, viewInfo);
        }

        public static string ConvertSearchResultSummary(string titleHtml, string searchResultSummaryHtml, string viewInfo)
        {
            string searchResultHtml = string.Format(@"<Run>{0}</Run> <Run Text=""{1}"" Foreground=""{{ThemeResource SystemControlBackgroundAccentBrush}}"" /><LineBreak/><Span FontStyle=""Italic"" Foreground=""{{ThemeResource SystemControlForegroundBaseMediumLowBrush}}"" FontSize=""{{ThemeResource ToolTipContentThemeFontSize}}""><Run>{2}</Run></Span>", titleHtml, viewInfo, searchResultSummaryHtml);
            searchResultHtml = searchResultHtml.Replace("&", "&amp;")
                .Replace("\n", string.Empty)
                .Replace("\r", string.Empty);

            string xaml = @"<TextBlock xmlns=""http://schemas.microsoft.com/winfx/2006/xaml/presentation"" Foreground=""{{ThemeResource SystemControlForegroundBaseMediumBrush}}"" TextWrapping=""Wrap"">{0}</TextBlock>";

            MatchCollection matchsForInvalidStr = new Regex(@"\[[^\]]*\]").Matches(searchResultHtml);
            if (matchsForInvalidStr != null && matchsForInvalidStr.Count > 0)
            {
                for (int j = 0; j < matchsForInvalidStr.Count; j++)
                {
                    var m = matchsForInvalidStr[j];

                    string placeHolder = m.Groups[0].Value; // 要被替换的元素
                    searchResultHtml = searchResultHtml.Replace(placeHolder, " ");
                }
            }

            MatchCollection matchsForSearchKeywords = new Regex(@"<em style=""color:red;"">([^>#]*)</em>").Matches(searchResultHtml);
            if (matchsForSearchKeywords != null && matchsForSearchKeywords.Count > 0)
            {
                for (int j = 0; j < matchsForSearchKeywords.Count; j++)
                {
                    var m = matchsForSearchKeywords[j];

                    string placeHolder = m.Groups[0].Value; // 要被替换的元素
                    string k = m.Groups[1].Value;

                    string linkXaml = string.Format(@"</Run><Run Foreground=""Red"">{0}</Run><Run>", k);
                    searchResultHtml = searchResultHtml.Replace(placeHolder, linkXaml);
                }
            }

            return string.Format(xaml, searchResultHtml);
        }
    }
}
