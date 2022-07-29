using sample2.windows;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;

namespace sample2
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        public App() : base()
        {
            this.Dispatcher.UnhandledException += OnDispatcherUnhandledException;
        }

        void OnDispatcherUnhandledException(object sender, System.Windows.Threading.DispatcherUnhandledExceptionEventArgs e)
        {
            MessageBox.Show("Unhandled exception occurred: \n" + e.Exception.Message + "\n Data:" + e.Exception.Data
               + "\n InnerException:" + e.Exception.InnerException
               + "\n Source:" + e.Exception.Source
               + "\n StackTrace:" + e.Exception.StackTrace
               + "\n TargetSite:" + e.Exception.TargetSite, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }
}
