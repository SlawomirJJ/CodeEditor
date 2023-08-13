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
using System.Windows.Documents;
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
        public Style ttIdentifierStyle = new TextStyle(Brushes.Black, null, FontStyle.Regular);
        public Style ttImmConstantStyle = new TextStyle(Brushes.DarkGreen, null, FontStyle.Regular);
        public Style ttKeywordStyle = new TextStyle(Brushes.Blue, null, FontStyle.Regular);
        public Style ttInvalidStyle = new TextStyle(Brushes.Red, null, FontStyle.Regular);
        public Style ttOperatorStyle = new TextStyle(Brushes.Purple, null, FontStyle.Regular);
        public Style ttDelimiterStyle = new TextStyle(Brushes.DarkGray, null, FontStyle.Regular);
        public Style ttCommentStyle = new TextStyle(Brushes.Green, null, FontStyle.Regular);
        public Style ttUnknownStyle = new TextStyle(Brushes.Gray, null, FontStyle.Regular);
        public Style ttDirectiveStyle = new TextStyle(Brushes.DarkOrange, null, FontStyle.Regular);
        public Style ttWhiteSpaceStyle = new TextStyle(Brushes.Black, null, FontStyle.Regular);
        public Style ttVarLocDescStyle = new TextStyle(Brushes.Yellow, null, FontStyle.Regular);
        public Style ttILLabelStyle = new TextStyle(Brushes.Pink, null, FontStyle.Regular);
        public Style ttVCBlockStyle = new TextStyle(Brushes.Black, null, FontStyle.Regular);
        public Style ttDirVMASMStyle = new TextStyle(Brushes.LightYellow, Brushes.BlueViolet, FontStyle.Regular); 
        public Style ttDirSpecStyle = new TextStyle(Brushes.Turquoise, Brushes.Black, FontStyle.Regular);

        List<int> listOfStates = new List<int>();
        FastColoredTextBox textBox;
        int lastVisibleLine;
        Dictionary<FastColoredTextBox, List<TokenizerLineState>> lineStateDictionary = new Dictionary<FastColoredTextBox, List<TokenizerLineState>>();



        private void fastColoredTextBox1_VisibleRangeChangedDelayed(object sender, EventArgs e)
        {
            textBox = sender as FastColoredTextBox;
            var firstVisibleLine = textBox.VisibleRange.Start.iLine;
            lastVisibleLine = textBox.VisibleRange.End.iLine;
                System.Diagnostics.Debug.WriteLine(String.Format("Zmiana linii - od: {0} do: {1}", firstVisibleLine, lastVisibleLine));

                List<TokenizerLineState> vls;
                if (lineStateDictionary.TryGetValue(textBox, out vls))
                {
                    int i = firstVisibleLine;
                    int firstUndefinedIndex = vls.FindIndex(x => x == TokenizerLineState.tlsUndefined);
                    if (firstUndefinedIndex<0)
                    {
                        firstUndefinedIndex = 0;
                    }
                    int j = firstUndefinedIndex;
                    if (firstVisibleLine > firstUndefinedIndex)
                    {
                        while (j <= lastVisibleLine)
                        {
                            int endLine = RunUpdateTokenizerFromLine(j, vls, textBox);
                            if (j == endLine)
                                j++;
                            else
                                j = endLine;
                        }
                    }
                    else
                    {
                        while (i <= lastVisibleLine)
                        {
                            int endLine = RunUpdateTokenizerFromLine(i, vls, textBox);
                            if (i == endLine)
                                i++;
                            else
                                i = endLine;
                        }
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
                if (endState == stany[lineIndex] || lineIndex >= lastVisibleLine)
                    cont = false;
                else
                {
                    beginState = stany[lineIndex] = endState;
                    lineIndex++;
                    if (lineIndex >= textBox.LinesCount || lineIndex >= lastVisibleLine)
                        cont = false;
                }
            }
            while (cont);
            return lineIndex;
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
