using Hipda.Client.Uwp.Pro.Commands;
using Hipda.Client.Uwp.Pro.Models;
using Hipda.Client.Uwp.Pro.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Windows.Storage;
using Windows.Storage.Pickers;
using Windows.Storage.Streams;

namespace Hipda.Client.Uwp.Pro.ViewModels
{
    public class QuickReplyViewModel : NotificationObject
    {
        public QuickReplyViewModel()
        {
            
        }

        public async Task<List<string>> UploadFiles(CancellationTokenSource cts, Action<int, int, string> beforeUpload, Action<string> insertCodeIntoTextBox, Action<int> afterUpload)
        {
            return await PostMessageService.UploadFiles(cts, beforeUpload, insertCodeIntoTextBox, afterUpload);
        }

        public async Task<bool> Post(CancellationTokenSource cts, string content, List<string> imageNameList, int threadId)
        {
            return await PostMessageService.PostReplyMessage(cts, content, imageNameList, threadId);
        }
    }
}
