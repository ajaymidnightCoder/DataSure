using System.Collections.ObjectModel;
using System.ComponentModel;

namespace DataSure.Contracts.HelperServices
{
    public interface INotificationService : INotifyPropertyChanged
    {
        ReadOnlyObservableCollection<string> ValidationMessages { get; }
        void AddValidationMessage(string message);
        void ClearMessages();
    }
}
