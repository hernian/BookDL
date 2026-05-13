using System;
using System.Collections.Generic;
using System.Text;

namespace BookDL.Presentation
{
    public class DialogResultEventArgs : EventArgs
    {
        public bool DialogResult { get; }

        public DialogResultEventArgs(bool dialogResult)
        {
            this.DialogResult = dialogResult;
        }
    }
}
