
using System.Windows;
using System.Windows.Controls;

using System.Windows.Input;


namespace sample2.User_Controls
{
    /// <summary>
    /// Interaction logic for TS_TextBlock.xaml
    /// </summary>
    public partial class TS_TextBlock : UserControl
    {
        _8_Tray_Settings previous_page;
        public TS_TextBlock(int row,int col, _8_Tray_Settings previous_page)
        {
            InitializeComponent();
            this.previous_page = previous_page;
        }

        private void TextBlock_PreviewTouchDown(object sender, TouchEventArgs e)
        {
            TextBlock text = sender as TextBlock;
            Window popup = new TraySetPopUp(text.Text,RowNum.Text, ColNum.Text, previous_page);
            popup.ShowDialog();
        }

        private void TextBlock_PreviewMouseDown(object sender, MouseButtonEventArgs e)
        {
            TextBlock text = sender as TextBlock;
            Window popup = new TraySetPopUp(text.Text, RowNum.Text, ColNum.Text, previous_page);
            popup.ShowDialog();
        }
    }
}
