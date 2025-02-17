using CommunityToolkit.Mvvm.ComponentModel;
using System.Collections.ObjectModel;
using Microsoft.Maui.ApplicationModel;
using DataSure.Contracts.HelperServices;
using DataSure.Models.NotificationModel;

namespace DataSure.Helper
{
    public partial class NotificationService : ObservableObject, INotificationService
    {
        private readonly ObservableCollection<string> _validationMessages;
        public ReadOnlyObservableCollection<string> ValidationMessages { get; }

        private readonly ObservableCollection<NotificationMessage> _notificationList;
        public ReadOnlyObservableCollection<NotificationMessage> NotificationList { get; }

        [ObservableProperty]
        private int progressPercentage; 

        public NotificationService()
        {
            _validationMessages = [];
            ValidationMessages = new ReadOnlyObservableCollection<string>(_validationMessages);

            _notificationList = [];
            NotificationList = new ReadOnlyObservableCollection<NotificationMessage>(_notificationList);
        }

        public void UpdateProgress(int progress)
        {
            MainThread.BeginInvokeOnMainThread(() =>
            {
                ProgressPercentage = progress;
                OnPropertyChanged(nameof(ProgressPercentage)); // ✅ Ensures Blazor UI updates
            });
        }

        public void NotifyUser(string message, MessageType msgType, int progress, bool isIndented = false)
        {
            var notification = new NotificationMessage(message, msgType, isIndented, progress);
            MainThread.BeginInvokeOnMainThread(() =>
            {
                _notificationList.Add(notification);
                OnPropertyChanged(nameof(NotificationList));

                UpdateProgress(progress);
            });
        }

        public void AddValidationMessage(string message)
        {
            MainThread.BeginInvokeOnMainThread(() =>
            {
                _validationMessages.Add(message);
                OnPropertyChanged(nameof(ValidationMessages));
            });
        }

        public void ClearMessages()
        {
            MainThread.BeginInvokeOnMainThread(() =>
            {
                _validationMessages.Clear();
                _notificationList.Clear();
                ProgressPercentage = 0; // ✅ Reset progress bar when clearing messages
                OnPropertyChanged(nameof(ValidationMessages));
                OnPropertyChanged(nameof(NotificationList));
            });
        }
    }
}
