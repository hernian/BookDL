using CommunityToolkit.Mvvm.Messaging;

namespace BookDL.Infrastructure
{
    public enum ToastType
    {
        Error,
        Information
    }

    public record ToastMessage(ToastType Type, string Text)
    {
        public static void SendInformation(string text) => Send(ToastType.Information, text);
        public static void SendError(string text) => Send(ToastType.Error, text);

        public static void Send(ToastType type, string text)
        {
            var toast = new ToastMessage(type, text);
            WeakReferenceMessenger.Default.Send(toast);
        }
    }
}
