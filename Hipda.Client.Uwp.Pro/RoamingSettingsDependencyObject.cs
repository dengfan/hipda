using Hipda.Client.Uwp.Pro.Models;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.UI.Xaml;

namespace Hipda.Client.Uwp.Pro
{
    public class RoamingSettingsDependencyObject : DependencyObject
    {
        public ObservableCollection<BlockUser> BlockUsers
        {
            get { return (ObservableCollection<BlockUser>)GetValue(BlockUsersProperty); }
            set { SetValue(BlockUsersProperty, value); }
        }

        // Using a DependencyProperty as the backing store for BlockUsers.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty BlockUsersProperty =
            DependencyProperty.Register("BlockUsers", typeof(ObservableCollection<BlockUser>), typeof(RoamingSettingsDependencyObject), new PropertyMetadata(new ObservableCollection<BlockUser>()));


        public ObservableCollection<BlockThread> BlockThreads
        {
            get { return (ObservableCollection<BlockThread>)GetValue(BlockThreadsProperty); }
            set { SetValue(BlockThreadsProperty, value); }
        }

        // Using a DependencyProperty as the backing store for BlockThreads.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty BlockThreadsProperty =
            DependencyProperty.Register("BlockThreads", typeof(ObservableCollection<BlockThread>), typeof(RoamingSettingsDependencyObject), new PropertyMetadata(new ObservableCollection<BlockThread>()));


        public ulong ImageCacheDataSize
        {
            get { return (ulong)GetValue(ImageCacheDataSizeProperty); }
            set { SetValue(ImageCacheDataSizeProperty, value); }
        }

        // Using a DependencyProperty as the backing store for ImageCacheDataSize.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty ImageCacheDataSizeProperty =
            DependencyProperty.Register("ImageCacheDataSize", typeof(ulong), typeof(RoamingSettingsDependencyObject), new PropertyMetadata(0UL));
    }
}
