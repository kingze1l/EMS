using System;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Animation;

namespace EMS.Views
{
    public enum NotificationType { Success, Info, Error }

    public partial class NotificationPopup : Window
    {
        public NotificationPopup(string message, NotificationType type)
        {
            InitializeComponent();
            MessageText.Text = message;
            switch (type)
            {
                case NotificationType.Success:
                    AccentBar.Background = new SolidColorBrush(Color.FromRgb(40, 167, 69)); // green
                    break;
                case NotificationType.Error:
                    AccentBar.Background = new SolidColorBrush(Color.FromRgb(220, 53, 69)); // red
                    break;
                default:
                    AccentBar.Background = new SolidColorBrush(Color.FromRgb(74, 144, 226)); // blue
                    break;
            }
        }

        public static void ShowPopup(string message, NotificationType type, Window owner = null)
        {
            Application.Current.Dispatcher.Invoke(async () =>
            {
                var popup = new NotificationPopup(message, type);
                if (owner != null)
                {
                    var point = owner.PointToScreen(new Point(owner.Width - popup.Width - 32, 32));
                    popup.Left = point.X / (PresentationSource.FromVisual(owner)?.CompositionTarget?.TransformToDevice.M11 ?? 1);
                    popup.Top = point.Y / (PresentationSource.FromVisual(owner)?.CompositionTarget?.TransformToDevice.M22 ?? 1);
                }
                else
                {
                    popup.WindowStartupLocation = WindowStartupLocation.CenterScreen;
                }
                popup.Show();
                var fadeIn = (Storyboard)popup.Resources["FadeIn"];
                fadeIn.Begin(popup.RootBorder);
                await Task.Delay(2200);
                var fadeOut = (Storyboard)popup.Resources["FadeOut"];
                fadeOut.Completed += (s, e) => popup.Close();
                fadeOut.Begin(popup.RootBorder);
            });
        }
    }
} 