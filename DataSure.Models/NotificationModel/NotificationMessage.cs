namespace DataSure.Models.NotificationModel
{

    public enum MessageType
    {
        Error,
        Success,
        Neutral
    }

    public class NotificationMessage
    {
        public string Message { get; set; }
        public MessageType MsgType { get; set; }
        public bool IsIndented { get; set; }
        public int Progress { get; set; }

        public NotificationMessage(string message, MessageType msgType, bool isIndented, int progress)
        {
            Message = message;
            MsgType = msgType;
            IsIndented = isIndented;
            Progress = progress;
        }
    }

}
