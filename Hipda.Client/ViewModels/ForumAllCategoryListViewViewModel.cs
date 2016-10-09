using GalaSoft.MvvmLight;
using Hipda.Client.Models;
using Hipda.Client.Services;
using System.Collections.Generic;

namespace Hipda.Client.ViewModels
{
    public class ForumAllCategoryListViewViewModel : ViewModelBase
    {
        ForumService _ds;

        private List<ForumCategoryModel> _forumAllCategoryList = new List<ForumCategoryModel>();

        public List<ForumCategoryModel> ForumAllCategoryList
        {
            get { return _forumAllCategoryList; }
            set
            {
                Set(ref _forumAllCategoryList, value);
            }
        }

        public ForumAllCategoryListViewViewModel()
        {
            _ds = new ForumService();
            GetData();
        }

        async void GetData()
        {
            ForumAllCategoryList = await _ds.GetForumData();
        }
    }
}
