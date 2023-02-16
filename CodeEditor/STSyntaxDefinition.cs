using FastColoredTextBoxNS;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CodeEditor
{
    internal class STSyntaxDefinition
    {
        // struktura Char przechowywanie znaków tekstu
        
        public struct Char
        {
            public char c;
            public StyleIndex style;
        }

        public readonly Style[] Styles = new Style[sizeof(ushort) * 8];
    }
}
