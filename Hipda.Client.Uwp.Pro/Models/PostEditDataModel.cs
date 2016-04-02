using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Hipda.Client.Uwp.Pro.Models
{
    public class PostEditDataModel
    {
        public PostEditDataModel(int postId, int threadId, string title, string content, ObservableCollection<AttachFileItemModel> attachFileList)
        {
            this.PostId = postId;
            this.ThreadId = threadId;
            this.Title = title;
            this.Content = content;
            this.AttachFileList = attachFileList;
        }

        public int PostId { get; private set; }

        public int ThreadId { get; private set; }

        public string Title { get; private set; }

        public string Content { get; private set; }

        public ObservableCollection<AttachFileItemModel> AttachFileList { get; private set; }
    }

    public class AttachFileItemModel
    {
        public AttachFileItemModel(int fileType, string id, string name, bool isUsed)
        {
            this.Id = id;
            this.Content = name;
            this.FileType = fileType;
            this.IsUsed = isUsed;
        }

        /// <summary>
        /// 0 表示图片 1 表示文件
        /// </summary>
        public int FileType { get; private set; }

        /// <summary>
        /// 附件的编号，用于插入到编辑内容中
        /// </summary>
        public string Id { get; private set; }

        /// <summary>
        /// 图片地址 或 文件名称
        /// </summary>
        public string Content { get; private set; }

        /// <summary>
        /// 是否已使用
        /// </summary>
        public bool IsUsed { get; private set; }
    }
}
