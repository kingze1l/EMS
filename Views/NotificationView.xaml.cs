using System.Windows;
using System.Windows.Controls;
using EMS.Models;
using EMS.Services;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using System.Linq;
using System.Windows.Threading;
using EMS.Views;

namespace EMS.Views
{
    public partial class NotificationView : UserControl
    {
        private readonly INotificationService _notificationService;
        private readonly string _userId;
        private ObservableCollection<Notification> _notifications;
        private readonly DispatcherTimer _refreshTimer;

        public NotificationView(INotificationService notificationService, string userId)
        {
            InitializeComponent();
            _notificationService = notificationService;
            _userId = userId;
            _notifications = new ObservableCollection<Notification>();
            NotificationsList.ItemsSource = _notifications;
            
            // Setup refresh timer
            _refreshTimer = new DispatcherTimer
            {
                Interval = System.TimeSpan.FromSeconds(30) // Refresh every 30 seconds
            };
            _refreshTimer.Tick += async (s, e) => await LoadNotifications();
            
            Loaded += NotificationView_Loaded;
            Unloaded += NotificationView_Unloaded;
        }

        private async void NotificationView_Loaded(object sender, RoutedEventArgs e)
        {
            _refreshTimer.Start();
            await LoadNotifications();
        }

        private void NotificationView_Unloaded(object sender, RoutedEventArgs e)
        {
            _refreshTimer.Stop();
        }

        private async Task LoadNotifications()
        {
            try
            {
                var notifications = await _notificationService.GetUserNotificationsAsync(_userId);
                _notifications.Clear();
                foreach (var notification in notifications)
                {
                    _notifications.Add(notification);
                }

                var unreadCount = await _notificationService.GetUnreadCountAsync(_userId);
                UnreadCount.Text = $"{unreadCount} unread notification{(unreadCount != 1 ? "s" : "")}";
            }
            catch (System.Exception ex)
            {
                NotificationPopup.ShowPopup($"Error loading notifications: {ex.Message}", NotificationType.Error, Application.Current.MainWindow);
            }
        }

        private async void ViewDetails_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button button && button.DataContext is Notification notification)
            {
                if (!string.IsNullOrEmpty(notification.ActionUrl))
                {
                    // TODO: Navigate to the action URL
                    NotificationPopup.ShowPopup($"Would navigate to: {notification.ActionUrl}", NotificationType.Info, Application.Current.MainWindow);
                }

                if (!notification.IsRead)
                {
                    try
                    {
                        await _notificationService.MarkAsReadAsync(notification.Id);
                        notification.IsRead = true;
                        await LoadNotifications(); // Refresh the list
                    }
                    catch (System.Exception ex)
                    {
                        NotificationPopup.ShowPopup($"Error marking notification as read: {ex.Message}", NotificationType.Error, Application.Current.MainWindow);
                    }
                }
            }
        }
    }
} 