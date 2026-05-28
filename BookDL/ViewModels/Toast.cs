using BookDL.Infrastructure;
using CommunityToolkit.Mvvm.Messaging;
using System.Windows;

namespace BookDL.Presentation
{
    public class Toast : IDisposable
    {
        private readonly Window _owner;
        private bool disposedValue;

        public Toast(Window owner)
        {
            _owner = owner;
            WeakReferenceMessenger.Default.Register<ToastMessage>(this, OnToastMessage);
        }

        private void OnToastMessage(object recipient, ToastMessage toast)
        {
            if (_owner.IsActive && _owner.IsVisible)
            {
                ShowToast(toast);
            }
        }

        private void ShowToast(ToastMessage toast)
        {
            var toastWindow = new ToastWindow(toast)
            {
                Owner = _owner,
                WindowStartupLocation = WindowStartupLocation.CenterOwner,
            };
            toastWindow.Show();
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                    WeakReferenceMessenger.Default.Unregister<ToastMessage>(this);
                }
                disposedValue = true;
            }
        }

        public void Dispose()
        {
            Dispose(disposing: true);
            GC.SuppressFinalize(this);
        }
    }
}
