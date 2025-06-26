using System.Windows;

namespace EMS.Views
{
    public partial class DarkDialog : Window
    {
        public bool Result { get; private set; }
        public DarkDialog(string message)
        {
            InitializeComponent();
            MessageText.Text = message;
        }

        private void Ok_Click(object sender, RoutedEventArgs e)
        {
            Result = true;
            DialogResult = true;
        }

        private void Cancel_Click(object sender, RoutedEventArgs e)
        {
            Result = false;
            DialogResult = false;
        }
    }
} 