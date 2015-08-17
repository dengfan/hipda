using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace HipdaUwpLite.Common
{
    public static class HtmlHelper
    {
        public static string HtmlToXaml(string htmlContent, int maxImageCount, ref int imageCount)
        {
            string deviceFamily = Windows.System.Profile.AnalyticsInfo.VersionInfo.DeviceFamily;

            if (deviceFamily.Equals("Windows.Mobile"))
            {
                string imagePlaceHolder = @"↵[Span Foreground=""Gray"" FontSize=""12""]--- 为节省流量，图片{0}已被智能忽略 ---[/Span]";

                var content = new StringBuilder(htmlContent);
                content.EnsureCapacity(htmlContent.Length * 2);

                content = content.Replace("[", "&#8968;");
                content = content.Replace("]", "&#8971;");
                content = content.Replace("&nbsp;", " ");
                content = content.Replace("↵", "&#8629;");

                // 移除无用的图片附加信息
                MatchCollection matchsForInvalidHtml1 = new Regex(@"<div class=""t_attach"".*\n.*\n.*\n\s*<div class=""t_smallfont"">.*").Matches(content.ToString());
                if (matchsForInvalidHtml1 != null && matchsForInvalidHtml1.Count > 0)
                {
                    for (int i = 0; i < matchsForInvalidHtml1.Count; i++)
                    {
                        var m = matchsForInvalidHtml1[i];

                        string placeHolder = m.Groups[0].Value; // 要被替换的元素
                        content = content.Replace(placeHolder, string.Empty);
                    }
                }

                // 替换链接
                MatchCollection matchsForLink = new Regex(@"<a\s+href=""([^""]*)""[^>]*>([^>#]*)</a>").Matches(content.ToString());
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

                // 将HTML字符串转换为RichTextBlock XAML字符串
                // 替换上载图片
                MatchCollection matchsForImage1 = new Regex(@"<img[^>]*file=""([^""]*)""\swidth=""([\d]*)""[^>]*>").Matches(content.ToString());
                if (matchsForImage1 != null && matchsForImage1.Count > 0)
                {
                    for (int i = 0; i < matchsForImage1.Count; i++)
                    {
                        imageCount++;

                        string placeHolderLabel = matchsForImage1[i].Groups[0].Value; // 要被替换的元素
                        string imgUrl = matchsForImage1[i].Groups[1].Value; // 图片URL
                        int width = Convert.ToInt16(matchsForImage1[i].Groups[2].Value); // 图片宽度

                        string imgXaml = @"[InlineUIContainer][Image Stretch=""None"" Margin=""0,10,0,5""][Image.Source][BitmapImage UriSource=""{0}"" /][/Image.Source][/Image][/InlineUIContainer]";

                        if (imageCount <= maxImageCount)
                        {
                            if (width > 220)
                            {
                                imgXaml = @"↵[InlineUIContainer][Image Stretch=""Uniform"" Margin=""0,10,0,5"" Width=""220""][Image.Source][BitmapImage DecodePixelWidth=""220"" UriSource=""{0}"" /][/Image.Source][/Image][/InlineUIContainer]";
                            }

                            imgUrl = "http://www.hi-pda.com/forum/" + imgUrl;
                            imgXaml = string.Format(imgXaml, imgUrl);
                        }
                        else
                        {
                            imgXaml = imagePlaceHolder;
                            imgXaml = string.Format(imgXaml, imageCount);
                        }

                        content = content.Replace(placeHolderLabel, imgXaml);
                    }
                }

                // 替换图片，已知图片宽度，主要是用编辑器引用网络图片，及少量带宽度的直接复制到编辑器里的图片
                MatchCollection matchsForImage2 = new Regex(@"<img\swidth=""([\d]*)""[^>]*src=""([^""]*)""[^>]*>").Matches(content.ToString());
                if (matchsForImage2 != null && matchsForImage2.Count > 0)
                {
                    for (int i = 0; i < matchsForImage2.Count; i++)
                    {
                        imageCount++;

                        var m = matchsForImage2[i];
                        string placeHolderLabel = m.Groups[0].Value; // 要被替换的元素
                        int width = Convert.ToInt16(matchsForImage2[i].Groups[1].Value); // 图片宽度
                        string imgUrl = m.Groups[2].Value; // 图片URL
                        string imgXaml = string.Empty;

                        if (imageCount <= maxImageCount)
                        {
                            imgXaml = @"[InlineUIContainer][Image Stretch=""None"" Margin=""0,10,0,5""][Image.Source][BitmapImage UriSource=""{0}"" /][/Image.Source][/Image][/InlineUIContainer]";
                            if (width > 220)
                            {
                                imgXaml = @"↵[InlineUIContainer][Image Stretch=""Uniform"" Margin=""0,10,0,5"" Width=""220""][Image.Source][BitmapImage DecodePixelWidth=""220"" UriSource=""{0}"" /][/Image.Source][/Image][/InlineUIContainer]";
                            }

                            if (!imgUrl.StartsWith("http")) imgUrl = "http://www.hi-pda.com/forum/" + imgUrl;
                            imgXaml = string.Format(imgXaml, imgUrl);
                        }
                        else
                        {
                            imgXaml = imagePlaceHolder;
                            imgXaml = string.Format(imgXaml, imageCount);
                        }

                        content = content.Replace(placeHolderLabel, imgXaml);
                    }
                }

                // 替换图片，未知图片宽度，来自网站自带的一些小ICON及表情图标 和 直接复制到编辑器里的图片
                MatchCollection matchsForImage3 = new Regex(@"<img[^>]*src=""([^""]*)""[^>]*>").Matches(content.ToString());
                if (matchsForImage3 != null && matchsForImage3.Count > 0)
                {
                    for (int i = 0; i < matchsForImage3.Count; i++)
                    {
                        var m = matchsForImage3[i];
                        string placeHolderLabel = m.Groups[0].Value; // 要被替换的元素
                        string imgUrl = m.Groups[1].Value; // 图片URL
                                                           //string imgXaml = @"[InlineUIContainer][Image Stretch=""None"" MaxWidth=""400""][Image.Source][BitmapImage UriSource=""{0}"" /][/Image.Source][/Image][/InlineUIContainer]";
                        string imgXaml = string.Empty;

                        if (imgUrl.EndsWith("/back.gif") || imgUrl.StartsWith("images/smilies/") || imgUrl.Contains("images/attachicons")) // 论坛自带
                        {
                            imgXaml = @"[InlineUIContainer][Image Stretch=""None""][Image.Source][BitmapImage UriSource=""{0}"" /][/Image.Source][/Image][/InlineUIContainer]";

                            if (!imgUrl.StartsWith("http")) imgUrl = "http://www.hi-pda.com/forum/" + imgUrl;
                            imgXaml = string.Format(imgXaml, imgUrl);
                        }
                        else
                        {
                            imageCount++;

                            //if (imageCount <= maxImageCount)
                            //{
                            //    imgXaml = @"↵[InlineUIContainer][Image Stretch=""Uniform"" Margin=""0,10,0,5"" Width=""220""][Image.Source][BitmapImage DecodePixelWidth=""220"" UriSource=""{0}"" /][/Image.Source][/Image][/InlineUIContainer]";

                            //    if (!imgUrl.StartsWith("http")) imgUrl = "http://www.hi-pda.com/forum/" + imgUrl;
                            //    imgXaml = string.Format(imgXaml, imgUrl);
                            //}
                            //else
                            //{
                            imgXaml = imagePlaceHolder;
                            imgXaml = string.Format(imgXaml, imageCount == 0 ? 1 : imageCount);
                            //}
                        }

                        content = content.Replace(placeHolderLabel, imgXaml);
                    }
                }

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
                xamlContent = string.Format(@"<RichTextBlock xmlns=""http://schemas.microsoft.com/winfx/2006/xaml/presentation""><Paragraph>{0}</Paragraph></RichTextBlock>", xamlContent);

                return xamlContent;
            }
            else
            {
                string imagePlaceHolder = @"↵[Span Foreground=""Gray"" FontSize=""12""]--- 为节省流量，图片{0}已被智能忽略 ---[/Span]";

                var content = new StringBuilder(htmlContent);
                content.EnsureCapacity(htmlContent.Length * 2);

                content = content.Replace("[", "&#8968;");
                content = content.Replace("]", "&#8971;");
                content = content.Replace("&nbsp;", " ");
                content = content.Replace("↵", "&#8629;");

                // 移除无用的图片附加信息
                MatchCollection matchsForInvalidHtml1 = new Regex(@"<div class=""t_attach"".*\n.*\n.*\n\s*<div class=""t_smallfont"">.*").Matches(content.ToString());
                if (matchsForInvalidHtml1 != null && matchsForInvalidHtml1.Count > 0)
                {
                    for (int i = 0; i < matchsForInvalidHtml1.Count; i++)
                    {
                        var m = matchsForInvalidHtml1[i];

                        string placeHolder = m.Groups[0].Value; // 要被替换的元素
                        content = content.Replace(placeHolder, string.Empty);
                    }
                }

                // 替换链接
                MatchCollection matchsForLink = new Regex(@"<a\s+href=""([^""]*)""[^>]*>([^>#]*)</a>").Matches(content.ToString());
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

                // 将HTML字符串转换为RichTextBlock XAML字符串
                // 替换上载图片
                MatchCollection matchsForImage1 = new Regex(@"<img[^>]*file=""([^""]*)""\swidth=""([\d]*)""[^>]*>").Matches(content.ToString());
                if (matchsForImage1 != null && matchsForImage1.Count > 0)
                {
                    for (int i = 0; i < matchsForImage1.Count; i++)
                    {
                        imageCount++;

                        string placeHolderLabel = matchsForImage1[i].Groups[0].Value; // 要被替换的元素
                        string imgUrl = matchsForImage1[i].Groups[1].Value; // 图片URL
                        int width = Convert.ToInt16(matchsForImage1[i].Groups[2].Value); // 图片宽度

                        string imgXaml = @"[InlineUIContainer][Image Stretch=""None"" Margin=""0,10,0,5""][Image.Source][BitmapImage UriSource=""{0}"" /][/Image.Source][/Image][/InlineUIContainer]";

                        if (imageCount <= maxImageCount)
                        {
                            if (width >= 600)
                            {
                                imgXaml = @"↵[InlineUIContainer][Image Stretch=""Uniform"" Margin=""0,10,0,5"" Width=""600""][Image.Source][BitmapImage DecodePixelWidth=""600"" UriSource=""{0}"" /][/Image.Source][/Image][/InlineUIContainer]";
                            }

                            imgUrl = "http://www.hi-pda.com/forum/" + imgUrl;
                            imgXaml = string.Format(imgXaml, imgUrl);
                        }
                        else
                        {
                            imgXaml = imagePlaceHolder;
                            imgXaml = string.Format(imgXaml, imageCount);
                        }

                        content = content.Replace(placeHolderLabel, imgXaml);
                    }
                }

                // 替换图片，已知图片宽度，主要是用编辑器引用网络图片，及少量带宽度的直接复制到编辑器里的图片
                MatchCollection matchsForImage2 = new Regex(@"<img\swidth=""([\d]*)""[^>]*src=""([^""]*)""[^>]*>").Matches(content.ToString());
                if (matchsForImage2 != null && matchsForImage2.Count > 0)
                {
                    for (int i = 0; i < matchsForImage2.Count; i++)
                    {
                        imageCount++;

                        var m = matchsForImage2[i];
                        string placeHolderLabel = m.Groups[0].Value; // 要被替换的元素
                        int width = Convert.ToInt16(matchsForImage2[i].Groups[1].Value); // 图片宽度
                        string imgUrl = m.Groups[2].Value; // 图片URL
                        string imgXaml = string.Empty;

                        if (imageCount <= maxImageCount)
                        {
                            imgXaml = @"[InlineUIContainer][Image Stretch=""None"" Margin=""0,10,0,5""][Image.Source][BitmapImage UriSource=""{0}"" /][/Image.Source][/Image][/InlineUIContainer]";
                            if (width >= 600)
                            {
                                imgXaml = @"↵[InlineUIContainer][Image Stretch=""Uniform"" Margin=""0,10,0,5"" Width=""600""][Image.Source][BitmapImage DecodePixelWidth=""600"" UriSource=""{0}"" /][/Image.Source][/Image][/InlineUIContainer]";
                            }

                            if (!imgUrl.StartsWith("http")) imgUrl = "http://www.hi-pda.com/forum/" + imgUrl;
                            imgXaml = string.Format(imgXaml, imgUrl);
                        }
                        else
                        {
                            imgXaml = imagePlaceHolder;
                            imgXaml = string.Format(imgXaml, imageCount);
                        }

                        content = content.Replace(placeHolderLabel, imgXaml);
                    }
                }

                // 替换图片，未知图片宽度，来自网站自带的一些小ICON及表情图标 和 直接复制到编辑器里的图片
                MatchCollection matchsForImage3 = new Regex(@"<img[^>]*src=""([^""]*)""[^>]*>").Matches(content.ToString());
                if (matchsForImage3 != null && matchsForImage3.Count > 0)
                {
                    for (int i = 0; i < matchsForImage3.Count; i++)
                    {
                        var m = matchsForImage3[i];
                        string placeHolderLabel = m.Groups[0].Value; // 要被替换的元素
                        string imgUrl = m.Groups[1].Value; // 图片URL
                                                           //string imgXaml = @"[InlineUIContainer][Image Stretch=""None"" MaxWidth=""400""][Image.Source][BitmapImage UriSource=""{0}"" /][/Image.Source][/Image][/InlineUIContainer]";
                        string imgXaml = string.Empty;

                        if (imgUrl.EndsWith("/back.gif") || imgUrl.StartsWith("images/smilies/") || imgUrl.Contains("images/attachicons")) // 论坛自带
                        {
                            imgXaml = @"[InlineUIContainer][Image Stretch=""None""][Image.Source][BitmapImage UriSource=""{0}"" /][/Image.Source][/Image][/InlineUIContainer]";

                            if (!imgUrl.StartsWith("http")) imgUrl = "http://www.hi-pda.com/forum/" + imgUrl;
                            imgXaml = string.Format(imgXaml, imgUrl);
                        }
                        else
                        {
                            imageCount++;

                            imgXaml = imagePlaceHolder;
                            imgXaml = string.Format(imgXaml, imageCount == 0 ? 1 : imageCount);
                        }

                        content = content.Replace(placeHolderLabel, imgXaml);
                    }
                }

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
                xamlContent = string.Format(@"<RichTextBlock xmlns=""http://schemas.microsoft.com/winfx/2006/xaml/presentation""><Paragraph>{0}</Paragraph></RichTextBlock>", xamlContent);

                return xamlContent;
            }
        }
    }
}
