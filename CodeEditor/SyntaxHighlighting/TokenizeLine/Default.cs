using CPDev.STComp05;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CodeEditor.SyntaxHighlighting.TokenizeLine
{
    public partial class TokenizeLine
    {
        TokenizerLineState TokenizeDefault()
        {
            while (nowPos < TextM.Length)
            {
                StartAt = nowPos;
                sb.Append(TextM, StartAt, 1);
                BasicToken bbt = ret.AddNewTokenType(sb.ToString(), STTokenType.ttInvalid, nowPos);
                bbt.LiniaKodu = lineIndex;
                sb.Remove(0, sb.Length);
                if (nowPos < TextM.Length && (TextM[nowPos] == '\'' || TextM[nowPos] == '\"'))
                {
                    return beginState = TokenizerLineState.tlsString;
                }

                nowPos++;
            }
            return beginState = TokenizerLineState.tlsDefault;
        }
    }
}
