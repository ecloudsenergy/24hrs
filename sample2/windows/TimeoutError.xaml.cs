
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;

namespace sample2.windows
{
    /// <summary>
    /// Interaction logic for TimeoutError.xaml
    /// </summary>
    public partial class TimeoutError : Window
    {
        public delegate void Next_Page();
        public Next_Page gotoPage, retryBtnfn;

        public TimeoutError(string retryBtnText = "", string sendBackText = "", string information = "", string header = "")
        {
            InitializeComponent();
            retryBtnfn = this.Close;
            if (retryBtnText == "")
                Retry.text_button.Text = "Retry";
            else
            {
                Retry.text_button.Text = retryBtnText;
                Retry.PreviewTouchDown += Retry_PreviewTouchDown;
                Retry.PreviewMouseDown += Retry_PreviewMouseDown;
            }
            Retry.text_button.FontSize = 18;

            if (sendBackText == "")
                SendToNextScreen.text_button.Text = "Cancel Order";
            else
                SendToNextScreen.text_button.Text = sendBackText;
            
            SendToNextScreen.text_button.FontSize = 18;
            if (information != "")
                Message_Content.Text = information;
            if (header != "")
                Box_Header.Text = header;

            SendToNextScreen.PreviewMouseDown += SendToNextScreen_PreviewMouseDown;
            SendToNextScreen.PreviewTouchDown += SendToNextScreen_PreviewTouchDown; ;
        }

        private void Retry_PreviewMouseDown(object sender, MouseButtonEventArgs e)
        {
            retryBtnfn?.Invoke();
        }

        private void Retry_PreviewTouchDown(object sender, TouchEventArgs e)
        {
            retryBtnfn?.Invoke();
        }

        private void SendToNextScreen_PreviewTouchDown(object sender, TouchEventArgs e)
        {
            gotoPage?.Invoke();
        }

        private void SendToNextScreen_PreviewMouseDown(object sender, MouseButtonEventArgs e)
        {
            gotoPage?.Invoke();
        }
    }
}
