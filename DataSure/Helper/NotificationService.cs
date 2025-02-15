using CommunityToolkit.Mvvm.ComponentModel;
using System.Collections.ObjectModel;
using Microsoft.Maui.ApplicationModel;
using DataSure.Contracts.HelperServices;

namespace DataSure.Helper
{
    public partial class NotificationService : ObservableObject, INotificationService
    {
        private readonly ObservableCollection<string> _validationMessages;
        public ReadOnlyObservableCollection<string> ValidationMessages { get; }

        public NotificationService()
        {
            _validationMessages = new ObservableCollection<string>();
            ValidationMessages = new ReadOnlyObservableCollection<string>(_validationMessages);
        }

        public void AddValidationMessage(string message)
        {
            if (MainThread.IsMainThread)
            {
                _validationMessages.Add(message);
                OnPropertyChanged(nameof(ValidationMessages));
            }
            else
            {
                MainThread.BeginInvokeOnMainThread(() =>
                {
                    _validationMessages.Add(message);
                    OnPropertyChanged(nameof(ValidationMessages));
                });
            }
        }

        public void ClearMessages()
        {
            MainThread.BeginInvokeOnMainThread(() =>
            {
                _validationMessages.Clear();
                OnPropertyChanged(nameof(ValidationMessages));
            });
        }
    }
}
