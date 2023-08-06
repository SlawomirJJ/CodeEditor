using CPDev.STComp05;
using FastColoredTextBoxNS;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CodeEditor.SyntaxHighlighting.TokenizeLine
{
    public partial class TokenizeLine
    {
        FastColoredTextBox textBox;
        TokenList ret = new TokenList();
        char[] TextM;
        //ret.SourceText = TextM;
        FastColoredTextBoxNS.Range range;
        int nowPos = 0;
        int StartAt;
        bool Chdone;
        int lineIndex;
        TokenizerLineState beginState;
        TokenizerLineState endState;
       
        StringBuilder sb = new StringBuilder(1);

        public TokenizeLine(int LineIndex, TokenizerLineState BeginState, FastColoredTextBox TextBox)
        {
            lineIndex = LineIndex;
            beginState = BeginState;
            textBox = TextBox;
            ret.SourceText = TextM;
            TextM=TextBox.Text.ToCharArray();
        }
        public (TokenList, TokenizerLineState) ProcessLine()
        {
            StringBuilder sb = new StringBuilder(1);
            while (nowPos < TextM.Length)
            {
                switch (beginState)
                {
                    case TokenizerLineState.tlsUndefined:
                        break;
                    case TokenizerLineState.tlsDefault:
                        TokenizeDefault();
                        break;
                    case TokenizerLineState.tlsComment:
                        break;
                    case TokenizerLineState.tlsString:
                        TokenizeString();                      
                        break;
                    case TokenizerLineState.tlsDirective:
                        break;
                    case TokenizerLineState.tlsVMAsm:
                        break;
                    case TokenizerLineState.tlsSpecialProc:
                        break;
                    case TokenizerLineState.tlsVerifDirect:
                        break;
                }
            }
            return (ret, beginState);
        }


        
    }
}
