using GalaSoft.MvvmLight.Ioc;
using Hipda.Client.ViewModels;
using Microsoft.Practices.ServiceLocation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Hipda.Client
{
    public class ViewModelLocator
    {
        public ViewModelLocator()
        {
            ServiceLocator.SetLocatorProvider(() => SimpleIoc.Default);
            SimpleIoc.Default.Register<MainPageViewModel>();
        }

        private static MainPageViewModel _main;

        public static MainPageViewModel Main
        {
            get
            {
                if (_main == null)
                    _main = ServiceLocator.Current.GetInstance<MainPageViewModel>();
                return _main;
            }
        }
    }
}
