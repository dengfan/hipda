using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Windows.System;

namespace Hipda.Html
{
    public static class HtmlToXaml
    {
        /// <summary>
        /// 过滤掉不能出现在XML中的字符
        /// </summary>
        /// <param name="txt"></param>
        /// <returns></returns>
        public static string ReplaceHexadecimalSymbols(string txt)
        {
            string r = "[\x00-\x08\x0B\x0C\x0E-\x1F]";
            r = Regex.Replace(txt, r, "", RegexOptions.Compiled);
            r = r.Replace("&", "&amp;");
            return r;
        }

        public static string[] ConvertPost(int postId, int threadId, int forumId, string forumName, string htmlContent, Dictionary<int, string[]> floorNoDic, ref Dictionary<string, string> inAppLinkUrlDic)
        {
            //string deviceFamily = Windows.System.Profile.AnalyticsInfo.VersionInfo.DeviceFamily;

            //if (deviceFamily.Equals("Windows.Mobile"))
            //{

            //}
            //else
            //{

            //}

            htmlContent = htmlContent.Replace("&lt;", "&amp;amp;lt;");
            htmlContent = htmlContent.Replace("&gt;", "&amp;amp;gt;");
            htmlContent = htmlContent.Replace("&amp;lt;", "&amp;amp;lt;");
            htmlContent = htmlContent.Replace("&amp;gt;", "&amp;amp;gt;");
            htmlContent = htmlContent.Replace("&nbsp;", " ");
            htmlContent = htmlContent.Replace("↵", "&#8629;");
            
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
            var matchsForInvalidHtml1 = new Regex(@"<div class=""t_attach"".*\n.*\n.*\n*.*").Matches(htmlContent);
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
            var matchsForInvalidHtml2 = new Regex(@"<p class=""imgtitle"">\n*.*\n*.*\n*.*\n*.*\n*.*\n*.*\n*.*\n*.*\n*.*\n*.*\n*.*").Matches(htmlContent);
            if (matchsForInvalidHtml2 != null && matchsForInvalidHtml2.Count > 0)
            {
                for (int i = 0; i < matchsForInvalidHtml2.Count; i++)
                {
                    var m = matchsForInvalidHtml2[i];

                    string placeHolder = m.Groups[0].Value; // 要被替换的元素
                    htmlContent = htmlContent.Replace(placeHolder, string.Empty);
                }
            }

            // 替换回复
            var matchsForRefLink = new Regex("<strong>回复\\s*<a href=\"[^\"]*pid=([0-9]+)[^\"]*\"[^>]*>([\\s\\S]*?)<\\/a>\\s*<i[^>]*>([\\s\\S]*?)<\\/i>\\s*</strong>").Matches(htmlContent);
            if (matchsForRefLink != null && matchsForRefLink.Count > 0)
            {
                for (int i = 0; i < matchsForRefLink.Count; i++)
                {
                    var m = matchsForRefLink[i];

                    string placeHolder = m.Groups[0].Value; // 要被替换的元素
                    int replyPostId = Convert.ToInt32(m.Groups[1].Value);
                    string floorNoStr = m.Groups[2].Value;
                    string username = m.Groups[3].Value;
                    string xaml = ConvertReply(username, replyPostId, floorNoStr, threadId, floorNoDic);
                    xaml = $@"</Paragraph></RichTextBlock>{xaml}<RichTextBlock xml:space=""preserve"" LineHeight=""{{Binding LineHeight,Source={{StaticResource MyLocalSettings}}}}""><Paragraph>";
                    xaml = WebUtility.HtmlEncode(xaml);
                    htmlContent = htmlContent.Replace(placeHolder, xaml);
                }
            }

            htmlContent = htmlContent.Replace("<strong>", string.Empty);
            htmlContent = htmlContent.Replace("</strong>", string.Empty);

            // 替换引用
            var matchsForQuote = new Regex(@"<blockquote>([\s\S]*?)<\/blockquote>").Matches(htmlContent);
            if (matchsForQuote != null && matchsForQuote.Count > 0)
            {
                for (int i = 0; i < matchsForQuote.Count; i++)
                {
                    var m = matchsForQuote[i];

                    string placeHolder = m.Groups[0].Value; // 要被替换的元素
                    string xaml = ConvertQuote(m.Groups[1].Value.Trim(), threadId, forumId, forumName, floorNoDic);
                    xaml = $@"</Paragraph></RichTextBlock>{xaml}<RichTextBlock xml:space=""preserve"" LineHeight=""{{Binding LineHeight,Source={{StaticResource MyLocalSettings}}}}""><Paragraph>";
                    xaml = WebUtility.HtmlEncode(xaml);
                    htmlContent = htmlContent.Replace(placeHolder, xaml);
                }
            }

            // 替换 flash js
            var matchsForFlashJs = new Regex("<script type=\"text/javascript\" reload=\"1\">document.write[^<]*\\s'src',\\s'([^']*)'[^<]*</script>").Matches(htmlContent);
            if (matchsForFlashJs != null && matchsForFlashJs.Count > 0)
            {
                for (int i = 0; i < matchsForFlashJs.Count; i++)
                {
                    var m = matchsForFlashJs[i];

                    string placeHolder = m.Groups[0].Value; // 要被替换的元素
                    string url = m.Groups[1].Value;

                    string xaml = $@"<Hyperlink NavigateUri=""{url}"" FontWeight=""Bold"" Foreground=""RoyalBlue"">【视频Flash地址：{url}】</Hyperlink>";
                    xaml = WebUtility.HtmlEncode(xaml);
                    htmlContent = htmlContent.Replace(placeHolder, xaml);
                }
            }

            // 替换链接
            int inAppLinkCount = 0;
            var matchsForLink = new Regex("<a\\s+href=\"([^\"]*)\"[^>]*>([\\s\\S]*?)<\\/a>").Matches(htmlContent);
            if (matchsForLink != null && matchsForLink.Count > 0)
            {
                for (int i = 0; i < matchsForLink.Count; i++)
                {
                    var m = matchsForLink[i];

                    string placeHolder = m.Groups[0].Value; // 要被替换的元素

                    string linkUrl = m.Groups[1].Value;
                    if (!linkUrl.Contains(":"))
                    {
                        linkUrl = string.Format("http://www.hi-pda.com/forum/{0}", linkUrl);
                    }

                    string linkContent = m.Groups[2].Value;
                    linkContent = new Regex("<[^>]*>").Replace(linkContent, string.Empty);

                    
                    if (linkUrl.StartsWith("http://www.hi-pda.com/forum/") || linkUrl.StartsWith("http://hi-pda.com/forum/"))
                    {
                        inAppLinkCount++;
                        var key = $"InAppLink_{threadId}_{postId}_{inAppLinkCount}";
                        string xaml = $@"<Hyperlink Name=""{key}"">{linkContent}</Hyperlink>";
                        if (!inAppLinkUrlDic.ContainsKey(key))
                        {
                            inAppLinkUrlDic.Add(key, linkUrl);
                        }
                        xaml = WebUtility.HtmlEncode(xaml);
                        htmlContent = new Regex(placeHolder.Replace("?", "\\?").Replace("[", "\\[").Replace("]", "\\]").Replace("(", "\\(").Replace(")", "\\)")).Replace(htmlContent, xaml, 1);
                    }
                    else
                    {
                        string xaml = $@"<Hyperlink NavigateUri=""{linkUrl}"" Foreground=""DodgerBlue"">{linkContent}</Hyperlink>";
                        xaml = WebUtility.HtmlEncode(xaml);
                        htmlContent = htmlContent.Replace(placeHolder, xaml);
                    }
                    
                }
            }

            // 替换小字
            var matchsForSmallFont = new Regex(@"<font size=""1"">([^<]*)</font>").Matches(htmlContent);
            if (matchsForSmallFont != null && matchsForSmallFont.Count > 0)
            {
                for (int i = 0; i < matchsForSmallFont.Count; i++)
                {
                    var m = matchsForSmallFont[i];

                    string placeHolder = m.Groups[0].Value; // 要被替换的元素
                    string fontText = m.Groups[1].Value;

                    string xaml = $@"<Span FontSize=""{{Binding FontSize2,Source={{StaticResource MyLocalSettings}}}}"">{fontText}</Span>";
                    fontText = WebUtility.HtmlEncode(fontText);
                    htmlContent = htmlContent.Replace(placeHolder, xaml);
                }
            }

            // 替换带颜色的font标签
            var matchsForColorText = new Regex(@"<font color=""([#0-9a-zA-Z]*)"">([^<]*)</font>").Matches(htmlContent);
            if (matchsForColorText != null && matchsForColorText.Count > 0)
            {
                for (int i = 0; i < matchsForColorText.Count; i++)
                {
                    var m = matchsForColorText[i];

                    string placeHolder = m.Groups[0].Value; // 要被替换的元素
                    string colorName = m.Groups[1].Value.ToLower().Trim();
                    string textContent = m.Groups[2].Value;

                    string xaml = $@"<Span Foreground=""{colorName}"">{textContent}</Span>";
                    if (colorName.Equals("#000") || colorName.Equals("#000000") || colorName.Equals("black") || (colorName.StartsWith("#") && colorName.Length != 4 && colorName.Length != 7))
                    {
                        xaml = $"<Span>{textContent}</Span>";
                    }
                    xaml = WebUtility.HtmlEncode(xaml);
                    htmlContent = htmlContent.Replace(placeHolder, xaml);
                }
            }

            // 替换"最后编辑时间"
            var matchsForLastEditInfo = new Regex("<i class=\"pstatus\">([\\s\\S]*?)</i>").Matches(htmlContent);
            if (matchsForLastEditInfo != null && matchsForLastEditInfo.Count == 1)
            {
                var m = matchsForLastEditInfo[0];
                string placeHolder = m.Groups[0].Value; // 要被替换的元素
                string infoContent = m.Groups[1].Value.Trim();

                string xaml = $@"<Run Text=""{infoContent}"" Foreground=""{{ThemeResource SystemControlForegroundBaseLowBrush}}"" FontSize=""{{Binding FontSize2,Source={{StaticResource MyLocalSettings}}}}""/><LineBreak/>";
                xaml = WebUtility.HtmlEncode(xaml);
                htmlContent = htmlContent.Replace(placeHolder, xaml);
            }

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

                    string xaml = $@"<InlineUIContainer><c:MyImage Url=""{imgUrl}""/></InlineUIContainer>";
                    xaml = WebUtility.HtmlEncode(xaml);
                    htmlContent = htmlContent.Replace(placeHolderLabel, xaml);
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
                    if (imgUrl.Equals("http://img.hi-pda.com/forum/images/default/attachimg.gif")) continue;
                    if (!imgUrl.StartsWith("http")) imgUrl = "http://www.hi-pda.com/forum/" + imgUrl;

                    string xaml = $@"<InlineUIContainer><c:MyImage Url=""{imgUrl}""/></InlineUIContainer>";
                    xaml = WebUtility.HtmlEncode(xaml);
                    htmlContent = htmlContent.Replace(placeHolderLabel, xaml);
                }
            }
            #endregion

            htmlContent = new Regex("<[^>]*>").Replace(htmlContent, string.Empty); // 移除所有HTML标签
            htmlContent = htmlContent.Replace("\r\n", string.Empty); // 忽略源换行
            htmlContent = htmlContent.Replace("\r", string.Empty); // 忽略源换行
            htmlContent = htmlContent.Replace("\n", string.Empty); // 忽略源换行
            htmlContent = new Regex(@"↵{1,}").Replace(htmlContent, "↵"); // 将多个换行符合并成一个
            htmlContent = new Regex(@"^↵").Replace(htmlContent, string.Empty); // 移除行首的换行符
            htmlContent = new Regex(@"↵$").Replace(htmlContent, string.Empty); // 移除行末的换行符
            htmlContent = htmlContent.Replace("↵", "\r\n"); // 解析换行符

            string xamlStr = 
                $@"<StackPanel xmlns=""http://schemas.microsoft.com/winfx/2006/xaml/presentation"" xmlns:c=""using:Hipda.Client.Controls"">
                    <RichTextBlock xml:space=""preserve"" LineHeight=""{{Binding LineHeight,Source={{StaticResource MyLocalSettings}}}}""><Paragraph>{htmlContent}</Paragraph></RichTextBlock>
                </StackPanel>";
            xamlStr = WebUtility.HtmlDecode(WebUtility.HtmlDecode(xamlStr));
            xamlStr = xamlStr.Replace("<Paragraph>\r\n", "<Paragraph>");
            xamlStr = xamlStr.Replace("<LineBreak/>\r\n</Paragraph>", "</Paragraph>");
            xamlStr = xamlStr.Replace("<RichTextBlock xml:space=\"preserve\" LineHeight=\"{Binding LineHeight,Source={StaticResource MyLocalSettings}}\"><Paragraph></Paragraph></RichTextBlock>", string.Empty);
            xamlStr = xamlStr.Replace("<RichTextBlock xml:space=\"preserve\" LineHeight=\"{Binding LineHeight,Source={StaticResource MyLocalSettings}}\"><Paragraph>\r\n</Paragraph></RichTextBlock>", string.Empty);
            xamlStr = ReplaceHexadecimalSymbols(xamlStr);
            xamlStr = xamlStr.Replace("&amp;lt;", "&lt;").Replace("&amp;gt;", "&gt;");
            return new string[] { xamlStr, inAppLinkCount.ToString() };
        }

        private static string ConvertReply(string username, int replyPostId, string floorNoStr, int threadId, Dictionary<int, string[]> floorNoDic)
        {
            string xamlStr = string.Empty;

            // 有缓存POST数据，通常是按页码顺序加载
            if (floorNoDic.ContainsKey(replyPostId))
            {
                string[] ary = floorNoDic[replyPostId];
                string authorUserIdStr = ary[0];

                xamlStr =
                    $@"<StackPanel Orientation=""Horizontal"">
                    <TextBlock Text=""回复"" FontWeight=""Bold"" VerticalAlignment=""Center""/>
                    <c:MyQuoteLink Margin=""4"" FontWeight=""Bold"" UserId=""{authorUserIdStr}"" Username=""{username}"" PostId=""{replyPostId}"" ThreadId=""{threadId}"" LinkContent=""{floorNoStr}""/>
                    <TextBlock Text=""{username}"" FontWeight=""Bold"" VerticalAlignment=""Center""/>
                </StackPanel>";
            }
            else // 无缓存POST数据，通常是直接跳转到指定页，从而导致相关的POST数据并未缓存
            {
                xamlStr =
                    $@"<StackPanel Orientation=""Horizontal"">
                    <TextBlock Text=""回复"" FontWeight=""Bold"" VerticalAlignment=""Center""/>
                    <c:MyQuoteLink Margin=""4"" FontWeight=""Bold"" PostId=""{replyPostId}"" ThreadId=""{threadId}"" LinkContent=""{floorNoStr}""/>
                    <TextBlock Text=""{username}"" FontWeight=""Bold"" VerticalAlignment=""Center""/>
                </StackPanel>";
            }

            xamlStr = ReplaceHexadecimalSymbols(xamlStr);
            return xamlStr;
        }

        private static string ConvertQuote(string htmlContent, int threadId, int forumId, string forumName, Dictionary<int, string[]> floorNoDic)
        {
            string xamlStr = string.Empty;
            string quoteContent = string.Empty;
            string quoteInfo = string.Empty;
            string username = string.Empty;
            int quotePostId = 0;

            int i = htmlContent.IndexOf("<font size=\"2\">");
            if (i == -1)
            {
                return ConvertQuote2(htmlContent);
            }

            quoteContent = htmlContent.Substring(0, i).Trim();
            quoteContent = new Regex("<[^>]*>").Replace(quoteContent, string.Empty);

            quoteInfo = htmlContent.Substring(i).Trim();

            // 获取 PostId
            var matchsForPostId = new Regex("pid=([0-9]+)").Matches(quoteInfo);
            if (matchsForPostId != null && matchsForPostId.Count == 1)
            {
                int.TryParse(matchsForPostId[0].Groups[1].ToString(), out quotePostId);
            }
            else
            {
                matchsForPostId = new Regex("PostId=\\\"([0-9]+)\\\"").Matches(quoteInfo);
                if (matchsForPostId != null && matchsForPostId.Count == 1)
                {
                    int.TryParse(matchsForPostId[0].Groups[1].ToString(), out quotePostId);
                }
            }

            if (quotePostId == 0)
            {
                return ConvertQuote2(htmlContent);
            }

            // 获取用户名
            var matchsForUsername = new Regex("<font color=\"#999999\">([^\\s]*)\\s").Matches(quoteInfo);
            if (matchsForUsername != null && matchsForUsername.Count == 1)
            {
                username = matchsForUsername[0].Groups[1].ToString();
            }

            // 有缓存POST数据，通常是按页码顺序加载
            if (floorNoDic.ContainsKey(quotePostId))
            {
                string[] ary = floorNoDic[quotePostId];
                string authorUserIdStr = ary[0];
                string floorNoStr = ary[1];

                xamlStr =
                    $@"<Grid Margin=""0,0,0,4"" Padding=""8"" Background=""{{ThemeResource SystemChromeLowColor}}"" BorderThickness=""0,0,1,1"" BorderBrush=""{{ThemeResource SystemChromeMediumColor}}"" CornerRadius=""4"">
                    <ContentControl Margin=""0,16,0,0"" Style=""{{Binding FontContrastRatio,Source={{StaticResource MyLocalSettings}},Converter={{StaticResource FontContrastRatioToContentControlForeground2StyleConverter}}}}"">
                        <RichTextBlock>
                            <Paragraph Margin=""36,0,0,0""><Run FontWeight=""Bold"">{username}</Run></Paragraph>
                            <Paragraph>{quoteContent}</Paragraph>
                        </RichTextBlock>
                    </ContentControl>
                    <c:MyQuoteLink UserId=""{authorUserIdStr}"" Username=""{username}"" PostId=""{quotePostId}"" ThreadId=""{threadId}"" LinkContent=""{floorNoStr}#"" FontWeight=""Bold"" HorizontalAlignment=""Right"" VerticalAlignment=""Top""/>
                    <c:MyAvatarForReply MyWidth=""30"" UserId=""{authorUserIdStr}"" Username=""{username}"" ForumId=""{forumId}"" ForumName=""{forumName}"" HorizontalAlignment=""Left"" VerticalAlignment=""Top""/>
                </Grid>";
            }
            else // 无缓存POST数据，通常是直接跳转到指定页，从而导致相关的POST数据并未缓存
            {
                xamlStr =
                    $@"<Grid Margin=""0,0,0,4"" Padding=""8"" Background=""{{ThemeResource SystemChromeLowColor}}"" BorderThickness=""0,0,1,1"" BorderBrush=""{{ThemeResource SystemChromeMediumColor}}"" CornerRadius=""4"">
                    <ContentControl Margin=""0,16,0,0"" Style=""{{Binding FontContrastRatio,Source={{StaticResource MyLocalSettings}},Converter={{StaticResource FontContrastRatioToContentControlForeground2StyleConverter}}}}"">
                        <RichTextBlock>
                            <Paragraph><Run FontWeight=""Bold"">{username}</Run></Paragraph>
                            <Paragraph>{quoteContent}</Paragraph>
                        </RichTextBlock>
                    </ContentControl>
                    <c:MyQuoteLink PostId=""{quotePostId}"" ThreadId=""{threadId}"" LinkContent=""查看引用详情"" HorizontalAlignment=""Right"" VerticalAlignment=""Top""/>
                </Grid>";
            }

            xamlStr = ReplaceHexadecimalSymbols(xamlStr);
            return xamlStr;
        }

        private static string ConvertQuote2(string htmlContent)
        {
            htmlContent = new Regex("<[^>]*>").Replace(htmlContent, string.Empty);
            string xamlStr = 
                $@"<Grid Margin=""0,0,0,4"" Padding=""8"" Background=""{{ThemeResource SystemChromeLowColor}}"" BorderThickness=""0,0,1,1"" BorderBrush=""{{ThemeResource SystemChromeMediumColor}}"" CornerRadius=""4"">
                    <ContentControl Style=""{{Binding FontContrastRatio,Source={{StaticResource MyLocalSettings}},Converter={{StaticResource FontContrastRatioToContentControlForeground2StyleConverter}}}}"">
                        <RichTextBlock><Paragraph>{htmlContent}</Paragraph></RichTextBlock>
                    </ContentControl>
                </Grid>";

            xamlStr = ReplaceHexadecimalSymbols(xamlStr);
            return xamlStr;
        }

        private static async void PostErrorEmailToDeveloper(string errorTitle, string errorDetails)
        {
            string uriStr = @"mailto:appxking@outlook.com?subject=【{0}】发送异常详情给开发者，以帮助开发者更好的解决问题&body={1}";
            uriStr = string.Format(uriStr, errorTitle, errorDetails);

            Uri uri = new Uri(uriStr, UriKind.Absolute);
            await Launcher.LaunchUriAsync(uri);
        }

        public static string ConvertUserInfo(string htmlContent)
        {
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
            htmlContent = htmlContent.Replace(@"<h3 class=""blocktitle lightlink"">", @"[LineBreak/][Bold Foreground=""{ThemeResource SystemControlBackgroundAccentBrush}""]");
            htmlContent = htmlContent.Replace("</h3>", "[/Bold][LineBreak/]");
            htmlContent = htmlContent.Replace(@"<img src=""images/default/online_buddy.gif"" title=""当前在线"" class=""online_buddy"">", " \uD83D\uDCA1 "); // 当前在线小图标
            htmlContent = htmlContent.Replace(@"<img src=""images/rank/seller/0.gif"" border=""0"" class=""absmiddle"">", " \uD83D\uDC94 "); // 买家信用小图标
            htmlContent = htmlContent.Replace(@"<img src=""images/rank/buyer/0.gif"" border=""0"" class=""absmiddle"">", " \uD83D\uDC95 "); // 卖家信用小图标

            // 星星小图标
            MatchCollection matchsForStarImage = new Regex(@"<img src=""images/default/star_level1.gif"" alt=""Rank: [0-9]*"">").Matches(htmlContent);
            if (matchsForStarImage != null && matchsForStarImage.Count > 0)
            {
                for (int i = 0; i < matchsForStarImage.Count; i++)
                {
                    var m = matchsForStarImage[i];

                    string placeHolder = m.Groups[0].Value; // 要被替换的元素
                    htmlContent = htmlContent.Replace(placeHolder, "\uD83C\uDF20");
                }
            }

            // 月亮小图标
            MatchCollection matchsForMoonImage = new Regex(@"<img src=""images/default/star_level2.gif"" alt=""Rank: [0-9]*"">").Matches(htmlContent);
            if (matchsForMoonImage != null && matchsForMoonImage.Count > 0)
            {
                for (int i = 0; i < matchsForMoonImage.Count; i++)
                {
                    var m = matchsForMoonImage[i];

                    string placeHolder = m.Groups[0].Value; // 要被替换的元素
                    htmlContent = htmlContent.Replace(placeHolder, "\uD83C\uDF19");
                }
            }

            // 太阳小图标
            MatchCollection matchsForSunImage = new Regex(@"<img src=""images/default/star_level3.gif"" alt=""Rank: [0-9]*"">").Matches(htmlContent);
            if (matchsForSunImage != null && matchsForSunImage.Count > 0)
            {
                for (int i = 0; i < matchsForSunImage.Count; i++)
                {
                    var m = matchsForSunImage[i];

                    string placeHolder = m.Groups[0].Value; // 要被替换的元素
                    htmlContent = htmlContent.Replace(placeHolder, "\uD83C\uDF1E");
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
                    string xaml = $@"<Hyperlink NavigateUri=""{linkUrl}"" Foreground=""{{ThemeResource SystemControlBackgroundAccentBrush}}"">{linkContent}</Hyperlink>";
                    xaml = WebUtility.HtmlEncode(xaml);
                    htmlContent = htmlContent.Replace(placeHolder, xaml);
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

                    string xaml = $@"<InlineUIContainer><c:MyImage Url=""{imgUrl}""/></InlineUIContainer>";
                    xaml = WebUtility.HtmlEncode(xaml);

                    htmlContent = htmlContent.Replace(placeHolderLabel, xaml);
                }
            }
            #endregion

            htmlContent = new Regex("<[^>]*>").Replace(htmlContent, string.Empty); // 移除所有HTML标签
            htmlContent = htmlContent.Replace("\r\n", string.Empty); // 忽略源换行
            htmlContent = htmlContent.Replace("\r", string.Empty); // 忽略源换行
            htmlContent = htmlContent.Replace("\n", string.Empty); // 忽略源换行
            htmlContent = new Regex(@"↵{1,}").Replace(htmlContent, "↵"); // 将多个换行符合并成一个
            htmlContent = new Regex(@"^↵").Replace(htmlContent, string.Empty); // 移除行首的换行符
            htmlContent = new Regex(@"↵$").Replace(htmlContent, string.Empty); // 移除行末的换行符
            htmlContent = htmlContent.Replace("↵", "\r\n"); // 解析换行符
            htmlContent = htmlContent.Replace("[", "<");
            htmlContent = htmlContent.Replace("]", ">");

            string xamlStr = string.Format(@"<RichTextBlock xml:space=""preserve"" xmlns=""http://schemas.microsoft.com/winfx/2006/xaml/presentation"" LineHeight=""{{Binding LineHeight,Source={{StaticResource MyLocalSettings}}}}"" xmlns:c=""using:Hipda.Client.Controls""><Paragraph>{0}</Paragraph></RichTextBlock>", htmlContent);
            xamlStr = WebUtility.HtmlDecode(xamlStr);
            xamlStr = ReplaceHexadecimalSymbols(xamlStr);
            return xamlStr;
        }

        public static string[] ConvertUserMessage(string htmlContent, string username, string timeStr, ref Dictionary<string, string> inAppLinkUrlDic)
        {
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

            // 替换链接
            int inAppLinkCount = 0;
            var matchsForLink = new Regex("<a\\s+href=\"([^\"]*)\"[^>]*>([\\s\\S]*?)<\\/a>").Matches(htmlContent);
            if (matchsForLink != null && matchsForLink.Count > 0)
            {
                for (int i = 0; i < matchsForLink.Count; i++)
                {
                    var m = matchsForLink[i];

                    string placeHolder = m.Groups[0].Value; // 要被替换的元素

                    string linkUrl = m.Groups[1].Value;
                    if (!linkUrl.Contains(":"))
                    {
                        linkUrl = string.Format("http://www.hi-pda.com/forum/{0}", linkUrl);
                    }

                    string linkContent = m.Groups[2].Value;
                    linkContent = new Regex("<[^>]*>").Replace(linkContent, string.Empty);

                    if (linkUrl.StartsWith("http://www.hi-pda.com/forum/") || linkUrl.StartsWith("http://hi-pda.com/forum/"))
                    {
                        inAppLinkCount++;
                        var key = $"InAppLink_{username}_{timeStr}_{inAppLinkCount}";
                        string xaml = $@"<Hyperlink Name=""{key}"" Foreground=""White"">{linkContent}</Hyperlink>";
                        if (!inAppLinkUrlDic.ContainsKey(key))
                        {
                            inAppLinkUrlDic.Add(key, linkUrl);
                        }
                        xaml = WebUtility.HtmlEncode(xaml);
                        htmlContent = new Regex(placeHolder.Replace("?", "\\?").Replace("[", "\\[").Replace("]", "\\]").Replace("(", "\\(").Replace(")", "\\)")).Replace(htmlContent, xaml, 1);
                    }
                    else
                    {
                        string xaml = $@"<Hyperlink NavigateUri=""{linkUrl}"" Foreground=""DodgerBlue"">{linkContent}</Hyperlink>";
                        xaml = WebUtility.HtmlEncode(xaml);
                        htmlContent = htmlContent.Replace(placeHolder, xaml);
                    }
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

                    string xaml = $@"<InlineUIContainer><c:MyImage Url=""{imgUrl}""/></InlineUIContainer>";
                    xaml = WebUtility.HtmlEncode(xaml);

                    htmlContent = htmlContent.Replace(placeHolderLabel, xaml);
                }
            }
            #endregion

            htmlContent = new Regex("<[^>]*>").Replace(htmlContent, string.Empty); // 移除所有HTML标签
            htmlContent = htmlContent.Replace("\r\n", string.Empty); // 忽略源换行
            htmlContent = htmlContent.Replace("\r", string.Empty); // 忽略源换行
            htmlContent = htmlContent.Replace("\n", string.Empty); // 忽略源换行
            htmlContent = new Regex(@"↵{1,}").Replace(htmlContent, "↵"); // 将多个换行符合并成一个
            htmlContent = new Regex(@"^↵").Replace(htmlContent, string.Empty); // 移除行首的换行符
            htmlContent = new Regex(@"↵$").Replace(htmlContent, string.Empty); // 移除行末的换行符
            htmlContent = htmlContent.Replace("↵", "\r\n"); // 解析换行符
            htmlContent = htmlContent.Replace("[", "<");
            htmlContent = htmlContent.Replace("]", ">");

            string xamlStr = string.Format(@"<RichTextBlock xml:space=""preserve"" xmlns=""http://schemas.microsoft.com/winfx/2006/xaml/presentation"" xmlns:c=""using:Hipda.Client.Controls""><Paragraph>{0}</Paragraph></RichTextBlock>", htmlContent);
            xamlStr = WebUtility.HtmlDecode(xamlStr);
            xamlStr = ReplaceHexadecimalSymbols(xamlStr);
            return new string[] { xamlStr, inAppLinkCount.ToString() };
        }

        public static string ConvertSearchThreadTitle(string title, string forumName, string imageFontIcon, string fileFontIcon, string viewInfo)
        {
            string xamlStr = @"<TextBlock xmlns=""http://schemas.microsoft.com/winfx/2006/xaml/presentation"" Style=""{{Binding FontContrastRatio,Source={{StaticResource MyLocalSettings}},Converter={{StaticResource FontContrastRatioToTextBlockForeground1StyleConverter}}}}"" TextWrapping=""Wrap"">[{4}] {0} <Run FontFamily=""Segoe MDL2 Assets"" Foreground=""OrangeRed"" Text=""{1}"" /> <Run FontFamily=""Segoe MDL2 Assets"" Foreground=""DeepSkyBlue"" Text=""{2}"" /> <Run Text=""{3}"" Foreground=""{{ThemeResource SystemControlBackgroundAccentBrush}}"" /></TextBlock>";

            var matchs = new Regex(@"<em style=""color:red;"">([^>#]*)</em>").Matches(title);
            if (matchs != null && matchs.Count > 0)
            {
                for (int j = 0; j < matchs.Count; j++)
                {
                    var m = matchs[j];

                    string placeHolder = m.Groups[0].Value; // 要被替换的元素
                    string k = m.Groups[1].Value;

                    string linkXaml = string.Format(@"<Run Foreground=""Red"">{0}</Run>", k);
                    title = title.Replace(placeHolder, linkXaml);
                }
            }

            xamlStr = string.Format(xamlStr, title, imageFontIcon, fileFontIcon, viewInfo, forumName);
            xamlStr = ReplaceHexadecimalSymbols(xamlStr);
            return xamlStr;
        }

        public static string ConvertSearchResultSummary(string titleHtml, string forumName, string searchResultSummaryHtml, string viewInfo)
        {
            string searchResultHtml = string.Format(@"<Run>{0}</Run> <Run Text=""{1}"" Foreground=""{{ThemeResource SystemControlBackgroundAccentBrush}}"" /><LineBreak/><Span FontStyle=""Italic"" Foreground=""{{ThemeResource SystemControlForegroundBaseMediumLowBrush}}"" FontSize=""{{Binding FontSize2,Source={{StaticResource MyLocalSettings}}}}""><Run>{2}</Run></Span>", titleHtml, viewInfo, searchResultSummaryHtml);
            searchResultHtml = searchResultHtml.Replace("\n", string.Empty).Replace("\r", string.Empty);

            string xamlStr = @"<TextBlock xmlns=""http://schemas.microsoft.com/winfx/2006/xaml/presentation"" Style=""{{Binding FontContrastRatio,Source={{StaticResource MyLocalSettings}},Converter={{StaticResource FontContrastRatioToTextBlockForeground1StyleConverter}}}}"" TextWrapping=""Wrap"">[{1}] {0}</TextBlock>";

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

            xamlStr = string.Format(xamlStr, searchResultHtml, forumName);
            xamlStr = ReplaceHexadecimalSymbols(xamlStr);
            return xamlStr;
        }
    }
}
