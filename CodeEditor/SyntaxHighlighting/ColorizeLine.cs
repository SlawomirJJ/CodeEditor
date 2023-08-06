using CodeEditor.SyntaxHighlighting.TokenizeLine;
using CPDev.STComp05;
using FastColoredTextBoxNS;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Xml;

namespace CodeEditor
{
    public partial class Form1 : Form
    {
        public TokenizerLineState TokenizeSingleLine(int lineIndex, TokenizerLineState beginState, FastColoredTextBox textBox)
        {
            // Utwórz zakres dla całej linii
            FastColoredTextBoxNS.Range range = textBox.GetLine(lineIndex);

            range.ClearStyle(ttIdentifierStyle, ttImmConstantStyle, ttKeywordStyle, ttInvalidStyle, ttOperatorStyle, ttDelimiterStyle, ttCommentStyle, ttUnknownStyle, ttDirectiveStyle, ttWhiteSpaceStyle, ttVarLocDescStyle, ttILLabelStyle, ttVCBlockStyle);

            var tokenizeLine = new TokenizeLine(lineIndex, beginState, textBox);

            (TokenList tokenlist, TokenizerLineState endState) = tokenizeLine.ProcessLine();

            foreach (var token in tokenlist.Lista)
            {
                Place tokenStart = new Place(token.Pozycja, token.LiniaKodu);
                Place tokenEnd = new Place(token.Pozycja + token.Tekst.Length, token.LiniaKodu);
                Range tokenRange = new Range(fastColoredTextBox1, tokenStart, tokenEnd);

                switch (token.Typ)
                {
                    case CPDev.STComp05.STTokenType.ttIdentifier:
                        tokenRange.SetStyle(ttIdentifierStyle);
                        break;
                    case CPDev.STComp05.STTokenType.ttImmConstant:
                        tokenRange.SetStyle(ttImmConstantStyle);
                        break;
                    case CPDev.STComp05.STTokenType.ttKeyword:
                        tokenRange.SetStyle(ttKeywordStyle);
                        break;
                    case CPDev.STComp05.STTokenType.ttInvalid:
                        tokenRange.SetStyle(ttInvalidStyle);
                        break;
                    case CPDev.STComp05.STTokenType.ttOperator:
                        tokenRange.SetStyle(ttOperatorStyle);
                        break;
                    case CPDev.STComp05.STTokenType.ttDelimiter:
                        tokenRange.SetStyle(ttDelimiterStyle);
                        break;
                    case CPDev.STComp05.STTokenType.ttComment:
                        tokenRange.SetStyle(ttCommentStyle);
                        break;
                    case CPDev.STComp05.STTokenType.ttUnknown:
                        tokenRange.SetStyle(ttUnknownStyle);
                        break;
                    case CPDev.STComp05.STTokenType.ttDirective:
                        tokenRange.SetStyle(ttDirectiveStyle);
                        break;
                    case CPDev.STComp05.STTokenType.ttWhiteSpace:
                        tokenRange.SetStyle(ttWhiteSpaceStyle);
                        break;
                    case CPDev.STComp05.STTokenType.ttVarLocDesc:
                        tokenRange.SetStyle(ttVarLocDescStyle);
                        break;
                    case CPDev.STComp05.STTokenType.ttILLabel:
                        tokenRange.SetStyle(ttILLabelStyle);
                        break;
                    case CPDev.STComp05.STTokenType.ttVCBlock:
                        tokenRange.SetStyle(ttVCBlockStyle);
                        break;
                    default:
                        break;

                }

            }
            return endState;
        }
    }
}
