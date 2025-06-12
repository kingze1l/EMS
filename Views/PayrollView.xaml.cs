using System.Windows.Controls;
using EMS.ViewModels;

namespace EMS.Views
{
    public partial class PayrollView : UserControl
    {
        public PayrollView(PayrollViewModel viewModel)
        {
            InitializeComponent();
            DataContext = viewModel;
        }

        // For design-time support and DI
        public PayrollView()
        {
            InitializeComponent();
        }
    }
} 