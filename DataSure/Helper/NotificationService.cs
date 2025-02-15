using CommunityToolkit.Mvvm.ComponentModel;
using System.Collections.ObjectModel;
using Microsoft.Maui.ApplicationModel;
using DataSure.Contracts.HelperServices;

namespace DataSure.Helper
{
    public partial class NotificationService : ObservableObject, INotificationService
    {
        [ObservableProperty]
        private ObservableCollection<string> validationMessages;

        public NotificationService()
        {
            validationMessages = new ObservableCollection<string>();
        }

        public void AddValidationMessage(string message)
        {
            if (MainThread.IsMainThread)
            {
                ValidationMessages.Add(message);
                OnPropertyChanged(nameof(ValidationMessages)); // Notify UI
            }
            else
            {
                MainThread.BeginInvokeOnMainThread(() =>
                {
                    ValidationMessages.Add(message);
                    OnPropertyChanged(nameof(ValidationMessages));
                });
            }
        }

        public void ClearMessages()
        {
            MainThread.BeginInvokeOnMainThread(() =>
            {
                ValidationMessages.Clear();
                OnPropertyChanged(nameof(ValidationMessages));
            });
        }
    }
}
