using System;
using System.Collections.Generic;
using System.Text;
using System.Windows;

namespace BookDL.Infrastructure
{
    public interface IOwnerWindowProvider
    {
        Window? Owner { get; }
    }

    public interface IOwnerWindowSetter
    {
        void SetOwner(Window owner);
    }

    public class OwnerWindowProvider : IOwnerWindowProvider, IOwnerWindowSetter
    {
        private Window? _owner;
        public Window? Owner => _owner;
        public void SetOwner(Window owner) => _owner = owner;
    }
}
