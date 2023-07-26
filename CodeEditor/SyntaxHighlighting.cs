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
        FastColoredTextBox textBox;
        private void fastColoredTextBox1_TextChanged(object sender, TextChangedEventArgs e)
        {
            textBox = sender as FastColoredTextBox;

            if (textBox != null)
            {

                // Pobierz indeks bieżącej linii
                int currentLineIndex = textBox.LinesCount > 0 ? (textBox.Selection.Start.iLine) : 0;

                Tokenize(currentLineIndex, State);

                

                
            }
        }

        void Tokenize(int lineIndex, int state)
        {
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
                                Place tokenStart = new Place(token.Pozycja, token.LiniaKodu);
                                Place tokenEnd = new Place(token.Pozycja + token.Tekst.Length, token.LiniaKodu);
                                Range tokenRange = new Range(fastColoredTextBox1, tokenStart, tokenEnd);
                                BasicToken lastToken = TokenList.Lista.Last();

                                int lastIndex = textChars.Length - 1;
                                // dodać sprawdzenie czy komentarz nie jest jednolinijkowy
                                if (lastToken.Typ == STTokenType.ttComment && textChars[lastIndex - 1] == '*' && textChars[lastIndex - 1] == ')')
                                {
                                    AddNewLineWithStartingState(lineIndex + 1, 1, textBox);

                                }
                                else if (lastToken.Typ == STTokenType.ttComment && (textChars[lastIndex - 1] != '*' || textChars[lastIndex - 1] != ')'))
                                {
                                    AddNewLineWithStartingState(lineIndex + 1, 2, textBox);

                                }
                                else if (lastToken.Typ != STTokenType.ttComment) // jeszcze trzeba dodać czy nie jest stringiem
                                {
                                    AddNewLineWithStartingState(lineIndex + 1, 1, textBox);

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
                        break;
                    default:
                        break;
                }

            }
        }
        /// <summary>
        /// Adding new line with starting State or if line already exist change state in existng line 
        /// </summary>
        /// <param name="line"></param>
        /// <param name="state"></param>
        void AddNewLineWithStartingState(int line, int state, FastColoredTextBox textBox)
        {
            
            if (!(listOfStates.Count > line))
            {
                listOfStates.Add(state);
            }
            else
            {
                listOfStates[line] = state;
                Tokenize(line, state);
            }
                
        }

        TokenList DisplacementTokensAfterComment(TokenList TokenListAfterComment,int firstIndexOutOfComment)
        {
            for (int i = 0; i < TokenListAfterComment.Lista.Count; i++)
            {
                TokenListAfterComment.Lista[i].Pozycja += firstIndexOutOfComment;
            }
            return TokenListAfterComment;
        }
            
            

    }
}
