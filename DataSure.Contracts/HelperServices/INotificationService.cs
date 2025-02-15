namespace DataSure.Contracts.HelperServices
{
    public interface INotificationService
    {
        void AddValidationMessage(string message);
        void ClearMessages();
    }
}
