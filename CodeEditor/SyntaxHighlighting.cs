using AutocompleteMenuNS;
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
        
        

        STTokenizer stTokenizer = new STTokenizer();
        List<int> listOfStates = new List<int>();
        private void fastColoredTextBox1_TextChanged(object sender, TextChangedEventArgs e)
        {
            FastColoredTextBox textBox = sender as FastColoredTextBox;

            if (textBox != null)
            {

                // Pobierz indeks bieżącej linii
                int currentLineIndex = textBox.LinesCount > 0 ? (textBox.Selection.Start.iLine) : 0;

                tokenize(currentLineIndex, State);

                void tokenize(int LineIndex, int state)
                {
                    // Utwórz zakres dla całej linii
                    FastColoredTextBoxNS.Range range = textBox.GetLine(LineIndex);

                    range.ClearStyle(ttIdentifierStyle, ttImmConstantStyle, ttKeywordStyle, ttInvalidStyle, ttOperatorStyle, ttDelimiterStyle, ttCommentStyle, ttUnknownStyle, ttDirectiveStyle, ttWhiteSpaceStyle, ttVarLocDescStyle, ttILLabelStyle, ttVCBlockStyle);

                    char[] textChars = range.Text.ToCharArray();
                    TokenList TokenList = stTokenizer.TokenizeSTStream(textChars, LineIndex);
                    if(listOfStates.Count == 0)
                    {
                        listOfStates.Add(1);//dodajemy zerową linię ze stanem 1
                    }
                    switch (listOfStates[currentLineIndex])
                    {
                        case 1:
                            ColorizeTokens(TokenList);
                            void ColorizeTokens(TokenList tokenList)
                            { 
                                
                            
                            foreach (var token in tokenList.Lista)
                            {
                                Place tokenStart = new Place(token.Pozycja, token.LiniaKodu);
                                Place tokenEnd = new Place(token.Pozycja + token.Tekst.Length, token.LiniaKodu);
                                Range tokenRange = new Range(fastColoredTextBox1, tokenStart, tokenEnd);
                                BasicToken lastToken = TokenList.Lista.Last();

                                int lastIndex = textChars.Length - 1;
                                // dodać sprawdzenie czy komentarz nie jest jednolinijkowy
                                if (lastToken.Typ == STTokenType.ttComment && textChars[lastIndex-1]=='*' && textChars[lastIndex - 1] == ')')
                                {
                                    AddNewLineWithStartingState(currentLineIndex + 1, 1);

                                }
                                else if (lastToken.Typ == STTokenType.ttComment && (textChars[lastIndex - 1] != '*' || textChars[lastIndex - 1] != ')'))
                                {
                                    AddNewLineWithStartingState(currentLineIndex + 1, 2);

                                }
                                else if (lastToken.Typ != STTokenType.ttComment ) // jeszcze trzeba dodać czy nie jest stringiem
                                {
                                    AddNewLineWithStartingState(currentLineIndex + 1, 1);

                                }
                                // dodać ify dla stringów

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
                            }
                            break;
                        case 2:
                            CommentState(textChars, LineIndex);
                            void CommentState(char[] TextM, int LineOffset)
                            {
                                Place tokenStart;
                                Place tokenEnd;
                                FastColoredTextBoxNS.Range tokenRange;
                                int nowPos;
                                int firstIndexOutOfComment;
                                bool firstCallOutOfComment=true;
                                bool flag = false;
                                List<char> listOfCharsAfterComment = new List<char>();
                                for (nowPos = 0; nowPos < TextM.Length; nowPos++)
                                {

                                    if (TextM[nowPos] == '*' && nowPos + 1 < TextM.Length && TextM[nowPos + 1] == ')')
                                    {
                                        tokenStart = new Place(0, LineOffset);
                                        tokenEnd = new Place(nowPos + 2, LineOffset);
                                        tokenRange = new FastColoredTextBoxNS.Range(fastColoredTextBox1, tokenStart, tokenEnd);
                                        tokenRange.SetStyle(ttCommentStyle);
                                        flag = true;

                                        AddNewLineWithStartingState(currentLineIndex + 1, 1);
                                    }
                                    else if (flag == true)
                                    {
                                        if (firstCallOutOfComment == true)
                                        {
                                            firstIndexOutOfComment = nowPos;
                                            firstCallOutOfComment = false;
                                        }
                                        // uwzględnić przesunięcie pozycji 
                                        listOfCharsAfterComment.Add(TextM[nowPos]);
                                        char[] textCharsAfterComment = listOfCharsAfterComment.ToArray();
                                        TokenList TokenListAfterComment = stTokenizer.TokenizeSTStream(textCharsAfterComment, LineIndex);
                                        ColorizeTokens(TokenListAfterComment);
                                    }
                                    else if (flag == false)
                                    {
                                        tokenStart = new Place(0, LineOffset);
                                        tokenEnd = new Place(nowPos + 1, LineOffset);
                                        tokenRange = new FastColoredTextBoxNS.Range(fastColoredTextBox1, tokenStart, tokenEnd);
                                        tokenRange.SetStyle(ttCommentStyle);
                                        AddNewLineWithStartingState(currentLineIndex + 1, 2);
                                    }
                                       
                                }
                            }

                            break;
                        case 3:
                            break;
                        default:
                            break;
                    }

                }

                
            }
        }
        /// <summary>
        /// Adding new line with starting State or if line already exist change state in existng line 
        /// </summary>
        /// <param name="line"></param>
        /// <param name="state"></param>
        void AddNewLineWithStartingState(int line, int state)
        {
            
            if (!(listOfStates.Count > line))
            {
                listOfStates.Add(state);
            }
            else
                listOfStates[line] = state;
        }



    }
}
