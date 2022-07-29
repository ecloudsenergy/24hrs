using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Navigation;

namespace sample2.helpers
{
    public static class Navigator
    {
        public static NavigationService NavigationService { get; } = (Application.Current.MainWindow as MainWindow).Page_Holder.NavigationService;

        public static void Navigate(Page toPage)
        {
            NavigationService.Navigate(toPage);
        }

        public static void GoBack()
        {
            NavigationService.GoBack();
        }

        public static void GoForward()
        {
            NavigationService.GoForward();
        }
    }
}
