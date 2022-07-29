using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;


namespace sample2.User_Controls
{
    /// <summary>
    /// Interaction logic for textBlock.xaml
    /// </summary>
    public partial class textBlock : UserControl
    {
        public textBlock()
        {
            InitializeComponent();
            
        }
        private void TextBlock_PreviewTouchDown(object sender, TouchEventArgs e)
        {
            TextBlock text = sender as TextBlock;
            
            Window popup = new RefillCount(text.Text, RowNum.Text,ColNum.Text);
            popup.ShowDialog();
        }

        private void TextBlock_PreviewMouseDown(object sender, MouseButtonEventArgs e)
        {
            TextBlock text = sender as TextBlock;
            Window popup = new RefillCount(text.Text, RowNum.Text, ColNum.Text);
            popup.ShowDialog();
        }
    }
}
