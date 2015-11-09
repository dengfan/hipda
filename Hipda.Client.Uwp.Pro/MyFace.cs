using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.UI.Popups;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Documents;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Media.Imaging;

// The Templated Control item template is documented at http://go.microsoft.com/fwlink/?LinkId=234235

namespace Hipda.Client.Uwp.Pro
{
    public sealed class MyFace : Control
    {
        private Grid _grid1;

        public MyFace()
        {
            this.DefaultStyleKey = typeof(MyFace);
        }


        public Uri FaceUri
        {
            get { return (Uri)GetValue(FaceUriProperty); }
            set { SetValue(FaceUriProperty, value); }
        }

        // Using a DependencyProperty as the backing store for FaceUri.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty FaceUriProperty =
            DependencyProperty.Register("FaceUri", typeof(Uri), typeof(MyFace), new PropertyMetadata(null, new PropertyChangedCallback(OnFaceUriChanged)));


        protected override void OnApplyTemplate()
        {
            base.OnApplyTemplate();

            _grid1 = GetTemplateChild("grid1") as Grid;
        }

        private static void OnFaceUriChanged(DependencyObject d,DependencyPropertyChangedEventArgs e)
        {
            var instance = d as MyFace;

            if (instance._grid1 != null)
            {
                BitmapImage bi = new BitmapImage();
                bi.UriSource = (Uri)e.NewValue;
                ImageBrush ib = new ImageBrush();
                ib.ImageSource = bi;
                instance._grid1.Background = ib;
            }
        }


        protected async override void OnHolding(HoldingRoutedEventArgs e)
        {
            base.OnHolding(e);

            await new MessageDialog(FaceUri.ToString()).ShowAsync();
        }

        protected async override void OnRightTapped(RightTappedRoutedEventArgs e)
        {
            base.OnRightTapped(e);
            await new MessageDialog(FaceUri.ToString()).ShowAsync();
        }
    }
}
