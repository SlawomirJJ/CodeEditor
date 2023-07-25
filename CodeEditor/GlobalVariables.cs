using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CodeEditor
{
    public static class GlobalVariables
    {
        private static int _state = 1;
        public static int State
        {
            get { return _state; }
            set { _state = value; }
        }
    }
}
