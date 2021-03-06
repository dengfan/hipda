﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Windows.UI.Text;
using Windows.UI.Xaml.Markup;

namespace Hipda.Client.Models
{
    public class ThreadItemForSearchTitleModel : ThreadItemModelBase
    {
        public ThreadItemForSearchTitleModel(int index, string searchKeyword, string forumName, int threadId, int pageNo, string title, int attachFileType, string replyCount, string viewCount, string authorUsername, int authorUserId, string authorCreateTime, string lastReplyUsername, string lastReplyTime)
        {
            this.Index = index;
            this.SearchKeyword = searchKeyword;
            this.ForumName = forumName;
            this.ThreadId = threadId;
            this.PageNo = pageNo;
            this.Title = title;
            this.AttachFileType = attachFileType;
            this.ReplyCount = replyCount;
            this.ViewCount = viewCount;
            this.AuthorUsername = authorUsername;
            this.AuthorUserId = authorUserId;
            this.AuthorCreateTime = authorCreateTime;
            this.LastReplyUsername = lastReplyUsername;
            this.LastReplyTime = lastReplyTime;
            this.ThreadType = ThreadDataType.SearchTitle;
        }

        public int Index { get; private set; }

        public string SearchKeyword { get; private set; }

        public int PageNo { get; private set; }

        public string ReplyCount { get; private set; }

        public string ViewCount { get; private set; }

        public int AttachFileType { get; private set; }

        public string AuthorCreateTime { get; private set; }

        public string LastReplyUsername { get; private set; }

        public string LastReplyTime { get; private set; }

        public override string ToString()
        {
            return this.Title;
        }

        public string ViewInfo
        {
            get
            {
                return string.Format("({0}/{1})", ReplyCount, ViewCount);
            }
        }

        public string LastReplyInfo
        {
            get
            {
                return string.Format("{0} {1}", LastReplyUsername, LastReplyTime);
            }
        }

        public string ImageFontIcon
        {
            get
            {
                return AttachFileType == 1 ? "\uD83C\uDF04" : string.Empty;
            }
        }

        public string FileFontIcon
        {
            get
            {
                return AttachFileType == 2 ? "\uD83D\uDCCE" : string.Empty;
            }
        }

        public object TitleControl
        {
            get
            {
                string xaml = Html.HtmlToXaml.ConvertSearchThreadTitle(Title, ForumName, ImageFontIcon, FileFontIcon, ViewInfo);
                return XamlReader.Load(xaml);
            }
        }
    }
}
