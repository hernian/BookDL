using System;
using System.Collections.Generic;
using System.Text;
using OpenQA.Selenium.DevTools.V146.IndexedDB;
using Serilog;

namespace BookDL.Infrastructure
{
    public class TagLog<T>
    {
        private readonly string _tag;
        public TagLog()
        {
            _tag = nameof(T);
        }

        public void Debug(string msg)
        {
            Log.Debug(_tag + msg);
        }
        public void Error(string msg)
        {
            Log.Error(_tag + msg);
        }
        public void Error(Exception ex, string msg)
        {
            Log.Error(ex, _tag + msg);
        }

        public void Information(string msg)
        {
            Log.Information(_tag + msg);
        }
    }

}
