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
        public AttachFileItemModel(int fileType, string id, string name, string fileExName, bool isUsed)
        {
            this.Id = id;
            this.Content = name;
            this.FileType = fileType;
            this.FileExName = fileExName;
            this.IsUsed = isUsed;
        }

        public string Id { get; private set; }
        public string Content { get; private set; }
        public int FileType { get; private set; }
        public string FileExName { get; private set; }
        public bool IsUsed { get; private set; }
    }
}
