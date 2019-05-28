using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Core;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;

// The Blank Page item template is documented at https://go.microsoft.com/fwlink/?LinkId=234238

namespace Messaging_for_Windows_on_ARM
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class AppShell : Page
    {
        public AppShell()
        {
            this.InitializeComponent();
            var navmgr = SystemNavigationManager.GetForCurrentView();
            navmgr.BackRequested += Navmgr_BackRequested;
            AppFrame.Navigate(typeof(MainPage));
        }

        private void Navmgr_BackRequested(object sender, BackRequestedEventArgs e)
        {
            if (AppFrame.CanGoBack)
            {
                AppFrame.GoBack();
                e.Handled = true;
            }
        }

        private void DeviceStates_CurrentStateChanged(object sender, VisualStateChangedEventArgs e)
        {
            if (e.NewState == Medium)
            {
                if (AppFrame.CurrentSourcePageType != typeof(MainPage))
                {
                    AppFrame.GoBack();
                }
            }
        }
    }
}
