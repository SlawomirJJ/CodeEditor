using CPDev.STComp05;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace CodeEditor.SyntaxHighlighting.TokenizeLine
{
    public partial class TokenizeLine
    {
        void TokenizeString()
        {
            StartAt = nowPos;

                char beginStringCharacter = TextM[nowPos];
                sb.Append(beginStringCharacter);
                nowPos++;
                while (nowPos + 1 < TextM.Length && TextM[nowPos] == beginStringCharacter && TextM[nowPos + 1] == beginStringCharacter)
                {
                    sb.Append(beginStringCharacter);
                    nowPos += 2;
                }
                while (nowPos < TextM.Length && TextM[nowPos] != beginStringCharacter)
                {
                    char az = TextM[nowPos];
                    if (az == '$')
                    {
                        nowPos++;
                        char ay = '\u0000';
                        if (nowPos < TextM.Length)
                            ay = TextM[nowPos];
                        switch (ay)
                        {
                            case '$':
                            case '\'':
                            case '\"':
                                sb.Append(ay);
                                break;
                            case 'L':
                            case 'l':
                                sb.Append('\n');
                                break;
                            case 'N':
                            case 'n':
                                sb.Append("\r\n");
                                break;
                            case 'P':
                            case 'p':
                                sb.Append('\u000B');
                                break;
                            case 'R':
                            case 'r':
                                sb.Append('\r');
                                break;
                            case 't':
                            case 'T':
                                sb.Append('\t');
                                break;
                            default:
                                {
                                    nowPos++;
                                    char ax = '\u0000';
                                    if (nowPos < TextM.Length)
                                        ax = TextM[nowPos];

                                    if (STUtils.IsHexChar(ay) && STUtils.IsHexChar(ax))
                                    {
                                        sb.Append(STUtils.DCharToChar(ay, ax));
                                    }
                                    else
                                    {
                                        sb.Append(az);
                                        nowPos -= 2;
                                        if (nowPos <= 0)
                                            nowPos = 0;
                                        if (STUtils.IsHexChar(ay))
                                            throw new ArgumentException("Error");
                                        else
                                            throw new ArgumentException("Error");
                                    }
                                }
                                break;
                        }
                    }
                    else
                    {

                        sb.Append(az);
                    }
                    nowPos++;
                    while (nowPos + 1 < TextM.Length && TextM[nowPos] == beginStringCharacter && TextM[nowPos + 1] == beginStringCharacter)
                    {
                        sb.Append(beginStringCharacter);
                        nowPos += 2;
                    }
                }
                nowPos++;
                sb.Append(beginStringCharacter);
                {
                    BasicToken bbt = new BasicToken(sb.ToString(), STTokenType.ttImmConstant, StartAt);
                    bbt.BuildInTypeName = beginStringCharacter == '\'' ? "STRING" : "WSTRING";
                    bbt.LiniaKodu = nowPos;
                    ret.Lista.Add(bbt); // AddNewTokenType(sb.ToString(), STTokenType.ttString, StartAt);
                }
                sb.Remove(0, sb.Length);
                Chdone = true;

        }
    }
}
