using AutocompleteMenuNS;
using CPDev.STComp05;
using FastColoredTextBoxNS;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Xml.Linq;
using static CodeEditor.GlobalVariables;
using Range = FastColoredTextBoxNS.Range;



namespace CodeEditor
{
    public partial class Form1 : Form
    {
        //private int state;
        // Style
        public Style ttIdentifierStyle = new TextStyle(Brushes.Black, null, FontStyle.Regular);// Identyfikator
        public Style ttImmConstantStyle = new TextStyle(Brushes.DarkGreen, null, FontStyle.Regular);// Stała
        public Style ttKeywordStyle = new TextStyle(Brushes.Blue, null, FontStyle.Regular);// Słowo kluczowe
        public Style ttInvalidStyle = new TextStyle(Brushes.Red, null, FontStyle.Regular);// Niepoprawne
        public Style ttOperatorStyle = new TextStyle(Brushes.Purple, null, FontStyle.Regular); // 
        public Style ttDelimiterStyle = new TextStyle(Brushes.DarkGray, null, FontStyle.Regular);// Separator
        public Style ttCommentStyle = new TextStyle(Brushes.Green, null, FontStyle.Regular);
        public Style ttUnknownStyle = new TextStyle(Brushes.Gray, null, FontStyle.Regular);
        public Style ttDirectiveStyle = new TextStyle(Brushes.DarkOrange, null, FontStyle.Regular);
        public Style ttWhiteSpaceStyle = new TextStyle(Brushes.Black, null, FontStyle.Regular);
        public Style ttVarLocDescStyle = new TextStyle(Brushes.Yellow, null, FontStyle.Regular);// Opis lokalizacji zmiennej
        public Style ttILLabelStyle = new TextStyle(Brushes.Pink, null, FontStyle.Regular);// Etykieta IL
        public Style ttVCBlockStyle = new TextStyle(Brushes.Black, null, FontStyle.Regular);


        List<int> listOfStates = new List<int>();
        FastColoredTextBox textBox;

        Dictionary<FastColoredTextBox, List<TokenizerLineState>> lineStateDictionary = new Dictionary<FastColoredTextBox, List<TokenizerLineState>>();

        private void fastColoredTextBox1_TextChanged(object sender, TextChangedEventArgs e)
        {
            System.Diagnostics.Debug.WriteLine(String.Format("Zmiana linii - od: {0} do: {1}", e.ChangedRange.FromLine, e.ChangedRange.ToLine));

            textBox = sender as FastColoredTextBox;

            List<TokenizerLineState> vls;
            if (lineStateDictionary.TryGetValue(textBox, out vls))
            {
                int i = e.ChangedRange.FromLine;
                while (i <= e.ChangedRange.ToLine)
                {
                    int endLine = RunUpdateTokenizerFromLine(i, vls, textBox);
                    if (i == endLine)
                        i++;
                    else
                        i = endLine;
                }
            }
            else
            {
                vls = new List<TokenizerLineState>();
                vls.AddRange(new TokenizerLineState[textBox.LinesCount]);
                lineStateDictionary.Add(textBox, vls);
                RunUpdateTokenizerFromLine(0, vls, textBox);
            }
        }

        private void FastColoredTextBox1_LineRemoved(object sender, LineRemovedEventArgs e)
        {
            System.Diagnostics.Debug.WriteLine(String.Format("Usuwanie linii - index: {0}, rozmiar: {1}", e.Index, e.Count));
            textBox = (FastColoredTextBox)sender;
            List<TokenizerLineState> vls;
            if (lineStateDictionary.TryGetValue(textBox, out vls))
            {
                vls.RemoveRange(e.Index, e.Count);
            }
            else
            {
                // Nie powinno się to zdarzyć
                new InvalidOperationException("Usuwanie linii z nieistniejącego kolorowania");
            }
            RunUpdateTokenizerFromLine(e.Index - 1, vls, textBox);
        }

        private void FastColoredTextBox1_LineInserted(object sender, LineInsertedEventArgs e)
        {
            textBox = (FastColoredTextBox)sender;
            List<TokenizerLineState> vls;
            System.Diagnostics.Debug.WriteLine(String.Format("Wstawienie linii - index: {0}, rozmiar: {1}", e.Index, e.Count));
            if (lineStateDictionary.TryGetValue(textBox, out vls))
            {
                //DumpVLS("Before insert: ", vls);
                vls.InsertRange(e.Index, new TokenizerLineState[e.Count]);
                //DumpVLS("End insert: ", vls);
                // Obsłużone przez TextChanged
                // RunUpdateTokenizerFromLine(e.Index - 1, vls, textBox);
            }
            else
            {
                vls = new List<TokenizerLineState>();
                vls.AddRange(new TokenizerLineState[textBox.LinesCount]);
                lineStateDictionary.Add(textBox, vls);
                // Obsłużone przez TextChanged
                // RunUpdateTokenizerFromLine(0, vls, textBox);
            }            
        }

        private void DumpVLS(string v, List<TokenizerLineState> vls)
        {
            StringBuilder bld = new StringBuilder(v);
            for (int i = 0; i < vls.Count; i++)
            {
                if (i != 0)
                    bld.Append(", ");
                bld.AppendFormat("[{0}:{1}]", i, vls[i]);
            }
            System.Diagnostics.Debug.WriteLine(bld.ToString());
        }

        protected int RunUpdateTokenizerFromLine(int lineIndex, List<TokenizerLineState> stany, FastColoredTextBox textBox)
        {
            TokenizerLineState beginState;
            if (lineIndex - 1 < 0)
                beginState = TokenizerLineState.tlsDefault;
            else
                beginState = stany[lineIndex - 1];

            bool cont;
            do
            {
                cont = true;
                TokenizerLineState endState = TokenizeSingleLine(lineIndex, beginState, textBox);
                if (endState == stany[lineIndex])
                    cont = false;
                else
                {
                    beginState = stany[lineIndex] = endState;
                    lineIndex++;
                    if (lineIndex >= textBox.LinesCount)
                        cont = false;
                }
            }
            while (cont);
            return lineIndex;
        }
        
        
        void Tokenize(int lineIndex, int state)
        {
#if TO_JEST_TO_WYRZUCENIA_CHOCIAZ_MOZE_INSPIROWAC
            if (lineIndex >= 0 && lineIndex < textBox.LinesCount)
            {
                // Utwórz zakres dla całej linii
                FastColoredTextBoxNS.Range range = textBox.GetLine(lineIndex);

                range.ClearStyle(ttIdentifierStyle, ttImmConstantStyle, ttKeywordStyle, ttInvalidStyle, ttOperatorStyle, ttDelimiterStyle, ttCommentStyle, ttUnknownStyle, ttDirectiveStyle, ttWhiteSpaceStyle, ttVarLocDescStyle, ttILLabelStyle, ttVCBlockStyle);

                char[] textChars = range.Text.ToCharArray();
                TokenList TokenList = stTokenizer.TokenizeSTStream(textChars, lineIndex);
                if (listOfStates.Count == 0)
                {
                    listOfStates.Add(1);//dodajemy zerową linię ze stanem 1
                }
                switch (listOfStates[lineIndex])
                {
                    case 1:
                        ColorizeTokens(TokenList);
                        void ColorizeTokens(TokenList tokenList)
                        {


                            foreach (var token in tokenList.Lista)
                            {
                                
                                if (token.Typ == STTokenType.ttComment && !token.Tekst.StartsWith("//"))
                                {
                                    //actualState = 2;
                                    AddNewLineWithStartingState(lineIndex + 1, 2, textBox);
                                }
                                    
                                else if (token.Typ == STTokenType.ttImmConstant && token.BuildInTypeName == "WSTRING")
                                {
                                    //actualState = 3;
                                    AddNewLineWithStartingState(lineIndex + 1, 3, textBox);
                                }    
                                    
                                else
                                {
                                    //actualState = 1;
                                    AddNewLineWithStartingState(lineIndex + 1, 1, textBox);
                                }
                                    

                                Place tokenStart = new Place(token.Pozycja, token.LiniaKodu);
                                Place tokenEnd = new Place(token.Pozycja + token.Tekst.Length, token.LiniaKodu);
                                Range tokenRange = new Range(fastColoredTextBox1, tokenStart, tokenEnd);
                                BasicToken lastToken = TokenList.Lista.Last();
                                /*
                                int lastIndex = textChars.Length - 1;
                                if (lastToken.Typ != STTokenType.ttComment && lastToken.Typ != STTokenType.ttImmConstant)
                                {
                                    AddNewLineWithStartingState(lineIndex + 1, 1, textBox);

                                }
                                // dodać sprawdzenie czy komentarz nie jest jednolinijkowy
                                else if (lastToken.Typ == STTokenType.ttComment && textChars[lastIndex - 1] == '*' && textChars[lastIndex - 1] == ')')
                                {
                                    AddNewLineWithStartingState(lineIndex + 1, 1, textBox);

                                }
                                else if (lastToken.Typ == STTokenType.ttComment && (textChars[lastIndex - 1] != '*' && textChars[lastIndex] != ')') && !lastToken.Tekst.StartsWith("//"))
                                {
                                    AddNewLineWithStartingState(lineIndex + 1, 2, textBox);

                                }

                                // ify dla stringów
                                /*
                                else if (lastToken.Typ == STTokenType.ttImmConstant &&  textChars[lastIndex] == '\"')
                                {
                                    AddNewLineWithStartingState(lineIndex + 1, 1, textBox);

                                }   */
                                /*
                                else if (lastToken.Typ == STTokenType.ttImmConstant && lastToken.BuildInTypeName== "WSTRING" && (textChars[lastIndex] != '\"'))
                                {
                                    AddNewLineWithStartingState(lineIndex + 1, 3, textBox);

                                }
                                else
                                    AddNewLineWithStartingState(lineIndex + 1, 1, textBox);
                                */
                                switch (token.Typ)
                                {
                                    case CPDev.STComp05.STTokenType.ttIdentifier:
                                        tokenRange.SetStyle(ttIdentifierStyle);
                                        break;
                                    case CPDev.STComp05.STTokenType.ttImmConstant:
                                        
                                        if (token.BuildInTypeName == "WSTRING")
                                        {
                                            StringState(textChars, lineIndex);
                                        }   
                                        
                                        
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
                        }
                        break;
                    case 2:
                        CommentState(textChars, lineIndex);
                        void CommentState(char[] TextM, int LineOffset)
                        {
                            Place tokenStart;
                            Place tokenEnd;
                            FastColoredTextBoxNS.Range tokenRange;
                            int nowPos;
                            int firstIndexOutOfComment = 0;
                            bool firstCallOutOfComment = true;
                            bool flag = false;
                            int positionCharAfterComment = 0;
                            List<char> listOfCharsAfterComment = new List<char>();
                            for (nowPos = 0; nowPos < TextM.Length; nowPos++)
                            {

                                if (nowPos - 1 >= 0 && TextM[nowPos - 1] == '*' && TextM[nowPos] == ')')
                                {
                                    tokenStart = new Place(0, LineOffset);
                                    tokenEnd = new Place(nowPos + 2, LineOffset);
                                    tokenRange = new FastColoredTextBoxNS.Range(fastColoredTextBox1, tokenStart, tokenEnd);
                                    tokenRange.SetStyle(ttCommentStyle);
                                    flag = true;

                                    AddNewLineWithStartingState(lineIndex + 1, 1, textBox);
                                }
                                else if (flag == false)
                                {
                                    tokenStart = new Place(0, LineOffset);
                                    tokenEnd = new Place(nowPos + 1, LineOffset);
                                    tokenRange = new FastColoredTextBoxNS.Range(fastColoredTextBox1, tokenStart, tokenEnd);
                                    tokenRange.SetStyle(ttCommentStyle);
                                    AddNewLineWithStartingState(lineIndex + 1, 2, textBox);
                                }
                                else if (flag == true)
                                {
                                    if (firstCallOutOfComment == true)
                                    {
                                        firstIndexOutOfComment = nowPos;
                                        firstCallOutOfComment = false;
                                    }
                                    listOfCharsAfterComment.Add(TextM[nowPos]);
                                    char[] textCharsAfterComment = listOfCharsAfterComment.ToArray();
                                    TokenList TokenListAfterComment = stTokenizer.TokenizeSTStream(textCharsAfterComment, lineIndex);
                                    var tokenListAfterDisplacement = DisplacementTokensAfterComment(TokenListAfterComment, firstIndexOutOfComment);

                                    // Utwórz zakres dla obszaru po komentarzu i wyczyść go
                                    tokenStart = new Place(firstIndexOutOfComment, LineOffset);
                                    tokenEnd = new Place(nowPos + 1, LineOffset);
                                    var rangeAfterComment = new FastColoredTextBoxNS.Range(fastColoredTextBox1, tokenStart, tokenEnd);

                                    rangeAfterComment.ClearStyle(ttIdentifierStyle, ttImmConstantStyle, ttKeywordStyle, ttInvalidStyle, ttOperatorStyle, ttDelimiterStyle, ttCommentStyle, ttUnknownStyle, ttDirectiveStyle, ttWhiteSpaceStyle, ttVarLocDescStyle, ttILLabelStyle, ttVCBlockStyle);

                                    ColorizeTokens(tokenListAfterDisplacement);
                                    positionCharAfterComment++;
                                }

                            }
                        }

                        break;
                    case 3:
                        StringState(textChars, lineIndex);
                        void StringState(char[] TextM, int LineOffset)
                        {
                            Place tokenStart;
                            Place tokenEnd;
                            FastColoredTextBoxNS.Range tokenRange;
                            int nowPos;
                            int firstIndexOutOfComment = 0;
                            bool firstCallOutOfComment = true;
                            bool flag = false;
                            int positionCharAfterComment = 0;
                            List<char> listOfCharsAfterComment = new List<char>();
                            for (nowPos = 0; nowPos < TextM.Length; nowPos++)
                            {

                                if (TextM[nowPos] == '\"')
                                {
                                    tokenStart = new Place(0, LineOffset);
                                    tokenEnd = new Place(nowPos + 2, LineOffset);
                                    stringRange = new FastColoredTextBoxNS.Range(fastColoredTextBox1, tokenStart, tokenEnd);
                                    stringRange.SetStyle(ttImmConstantStyle);
                                    flag = true;

                                    AddNewLineWithStartingState(lineIndex + 1, 1, textBox);
                                }
                                else if (flag == false)
                                {
                                    tokenStart = new Place(0, LineOffset);
                                    tokenEnd = new Place(nowPos + 1, LineOffset);
                                    stringRange = new FastColoredTextBoxNS.Range(fastColoredTextBox1, tokenStart, tokenEnd);
                                    stringRange.SetStyle(ttImmConstantStyle);
                                    AddNewLineWithStartingState(lineIndex + 1, 3, textBox);
                                    caretPlace = fastColoredTextBox1.Selection.Start;
                                }
                                else if (flag == true)
                                {
                                    if (firstCallOutOfComment == true)
                                    {
                                        firstIndexOutOfComment = nowPos;
                                        firstCallOutOfComment = false;
                                    }
                                    listOfCharsAfterComment.Add(TextM[nowPos]);
                                    char[] textCharsAfterComment = listOfCharsAfterComment.ToArray();
                                    TokenList TokenListAfterComment = stTokenizer.TokenizeSTStream(textCharsAfterComment, lineIndex);
                                    var tokenListAfterDisplacement = DisplacementTokensAfterComment(TokenListAfterComment, firstIndexOutOfComment);

                                    // Utwórz zakres dla obszaru po stringu i wyczyść go
                                    tokenStart = new Place(firstIndexOutOfComment, LineOffset);
                                    tokenEnd = new Place(nowPos + 1, LineOffset);
                                    var rangeAfterComment = new FastColoredTextBoxNS.Range(fastColoredTextBox1, tokenStart, tokenEnd);

                                    rangeAfterComment.ClearStyle(ttIdentifierStyle, ttImmConstantStyle, ttKeywordStyle, ttInvalidStyle, ttOperatorStyle, ttDelimiterStyle, ttCommentStyle, ttUnknownStyle, ttDirectiveStyle, ttWhiteSpaceStyle, ttVarLocDescStyle, ttILLabelStyle, ttVCBlockStyle);

                                    ColorizeTokens(tokenListAfterDisplacement);
                                    positionCharAfterComment++;
                                    AddNewLineWithStartingState(lineIndex + 1, 1, textBox);
                                }

                            }
                        }
                        break;
                    default:
                        break;
                }

            }
#endif
        }        
    }

    public enum TokenizerLineState
    {
        tlsUndefined = 0,
        tlsDefault = 1,
        tlsComment = 2,
        tlsDirective = 4,
        tlsVMAsm = 5,
        tlsSpecialProc = 6,
        tlsVerifDirect = 7
    }
}
