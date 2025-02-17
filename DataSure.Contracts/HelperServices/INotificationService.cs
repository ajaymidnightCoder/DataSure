using DataSure.Models.NotificationModel;
using System.Collections.ObjectModel;
using System.ComponentModel;

namespace DataSure.Contracts.HelperServices
{
    public interface INotificationService : INotifyPropertyChanged
    {
        ReadOnlyObservableCollection<string> ValidationMessages { get; }
        ReadOnlyObservableCollection<NotificationMessage> NotificationList { get; }
        int ProgressPercentage { get; }
        void UpdateProgress(int progress);
        void NotifyUser(string message, MessageType msgType, int progress, bool isIndented = false);
        void AddValidationMessage(string message);
        void ClearMessages();
    }
}
