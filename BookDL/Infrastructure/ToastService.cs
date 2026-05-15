using BookDL.Presentation;
using System.Security.Cryptography.X509Certificates;
using System.Windows;

namespace BookDL.Infrastructure
{
    public enum MessageType
    {
        Information,
        Error,
    }

    public interface IMessageService
    {
        void Show(MessageType type, string message);
    }

    public class ToastService : IMessageService
    {
        private readonly IOwnerWindowProvider _ownerProvider;

        public ToastService(IOwnerWindowProvider ownerProvider)
        {
            _ownerProvider = ownerProvider;
        }

        public void Show(MessageType type, string message)
        {
            var toast = new Toast(type, message);
            if (_ownerProvider.Owner is { } owner)
            {
                toast.Owner = owner;
            }
            toast.Show();
        }
    }
}
