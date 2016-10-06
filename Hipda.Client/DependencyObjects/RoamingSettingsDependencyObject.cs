using Hipda.Client.Models;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.UI.Xaml;

namespace Hipda.Client
{
    public class RoamingSettingsDependencyObject : DependencyObject
    {
        public bool CanShowTopThread
        {
            get { return (bool)GetValue(CanShowTopThreadProperty); }
            set { SetValue(CanShowTopThreadProperty, value); }
        }

        // Using a DependencyProperty as the backing store for CanShowTopThread.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty CanShowTopThreadProperty =
            DependencyProperty.Register("CanShowTopThread", typeof(bool), typeof(RoamingSettingsDependencyObject), new PropertyMetadata(true));


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


    }
}
