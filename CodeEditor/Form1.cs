using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

using System.IO;
using System.Text.RegularExpressions;
using FastColoredTextBoxNS;
using CodeEditor.Properties;
using CPDev.STComp05;
using System.Xml.Linq;

namespace CodeEditor
{
    public partial class Form1 : Form
    {
        Image closeImage, closeImageAct;
        public Form1()
        {
            InitializeComponent();
        }


        private void SaveDlg()
        {
            //new save file dialog
            SaveFileDialog saveFileDialog = new SaveFileDialog();
            // filter
            saveFileDialog.Filter = "ST File|*.ST";
            // if after showing dialog, user clicked ok
            if (saveFileDialog.ShowDialog() == DialogResult.OK)
            {
                StreamWriter sw = new StreamWriter(saveFileDialog.FileName);
                //sw.Write(fastColoredTextBox1.Text);
                // Ustawienie właściwości Tag na ścieżkę do pliku
                TabPage SelectedTab = tabPage.SelectedTab;
                SelectedTab.Tag = sw.ToString();
                fastColoredTextBox1.Tag = sw.ToString();
                sw.Close();
            }
        }

        private void saveAsToolStripMenuItem_Click(object sender, EventArgs e)
        {
            SaveDlg();
        }

        private void newToolStripMenuItem_Click(object sender, EventArgs e)
        {
            // Tworzenie nowej zakładki
            TabPage newTabPage = new TabPage();

            // Tworzenie kontrolki FastColoredTextBox z plikiem
            FastColoredTextBox fastColoredTextBox1 = new FastColoredTextBox();

            // Dodanie kontrolki FastColoredTextBox do nowej zakładki
            newTabPage.Controls.Add(fastColoredTextBox1);
            AutocompleteMenuNS.AutocompleteMenu acm = new AutocompleteMenuNS.AutocompleteMenu();
            acm.SetAutocompleteMenu(fastColoredTextBox1, acm);
            acm.Items = this.autocompleteMenu1.Items;
            acm.Colors = this.autocompleteMenu1.Colors;
            acm.Font = this.autocompleteMenu1.Font;

            // Dodanie nowej zakładki do kontroli TabControl
            tabPage.TabPages.Add(newTabPage);

            //nadanie nazwy
            int k = 0;
            int i = 0;
            while (true)
            {
                k = 0;
                for (int j = 0; j < tabPage.TabCount-1; j++)
                {
                    if (tabPage.TabPages[j].Text == "newFile" + (i + 1))
                    {
                        continue;
                    }
                    else
                    {
                        k++;
                    }                       
                }
                if (k == tabPage.TabCount - 1)
                {
                    newTabPage.Text = "newFile" + (i + 1);
                    break;
                }
                i++;              
            }

            // Ustawienie nowej zakładki jako aktualnie wybranej
            tabPage.SelectedTab = newTabPage;

            //obsługa zdarzenia TextChanged
            fastColoredTextBox1.TextChanged += new EventHandler<TextChangedEventArgs>(fastColoredTextBox1_TextChanged);
            fastColoredTextBox1.Dock = DockStyle.Fill;
            fastColoredTextBox1.AutoCompleteBrackets = true;

        }
        private void CloseTabButton_Click(object sender, EventArgs e)
        {
            Button closeButton = (Button)sender;
            int index = (int)closeButton.Tag;
            tabPage.TabPages.RemoveAt(index);
        }
        // method, to open file
        private void OpenDlg()
        {
            // create new open file dialog
            OpenFileDialog openFileDialog = new OpenFileDialog();
            openFileDialog.Filter = "ST File|*.ST|Any File|*.*";
            // if after showing dialog, clicked ok
            if (openFileDialog.ShowDialog() == DialogResult.OK)
            {
                // open file
                StreamReader sr = new StreamReader(openFileDialog.FileName);
                // place file text to text box
                //fastColoredTextBox1.Text = sr.ReadToEnd();
                // close file
                sr.Close();
                // text of this window = path of currently opened file
                this.Text = openFileDialog.FileName;
            }
        }


        private void openToolStripMenuItem_Click(object sender, EventArgs e)
        {
            OpenDlg();
        }
        
        private void saveToolStripMenuItem_Click(object sender, EventArgs e)
        {
            TabPage SelectedTab = tabPage.SelectedTab;
            var selectedFastColoredTextBox = SelectedTab.Controls.OfType<FastColoredTextBox>().FirstOrDefault();

            if (SelectedTab.Tag != null)
            {
                File.WriteAllText(SelectedTab.Tag.ToString(), selectedFastColoredTextBox.Text);
            }
            else
            {
                SaveDlg();
            }
                
            
 

        }

        private void saveAllToolStripMenuItem_Click(object sender, EventArgs e)
        {
            for (int i = 0; i < tabPage.TabCount; i++)
            {
                TabPage SelectedTab = tabPage.TabPages[i];
                FastColoredTextBox selectedFastColoredTextBox = SelectedTab.Controls.OfType<FastColoredTextBox>().FirstOrDefault();

                if (SelectedTab.Tag != null)
                {
                    File.WriteAllText(SelectedTab.Tag.ToString(), selectedFastColoredTextBox.Text);
                }
                else
                {
                    SaveDlg();
                }
            }
        }

        private void exitToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Application.Exit();
        }

        private void copyToolStripMenuItem_Click(object sender, EventArgs e)
        {
            fastColoredTextBox1.Copy();
        }

        private void pasteToolStripMenuItem_Click(object sender, EventArgs e)
        {
            //fastColoredTextBox1.Paste();
        }

        private void backgroundColorToolStripMenuItem_Click(object sender, EventArgs e)
        {
            // new color choosing dialog
            ColorDialog cd = new ColorDialog();
            // if after showing dialog, user clicked ok
            if (cd.ShowDialog() == DialogResult.OK)
            {
                //fastColoredTextBox1.BackColor = cd.Color;
            }
        }

        private void textColorToolStripMenuItem_Click(object sender, EventArgs e)
        {
            // new color choosing dialog
            ColorDialog cd = new ColorDialog();
            // if after showing dialog, user clicked ok
            if (cd.ShowDialog() == DialogResult.OK)
            {
                ///fastColoredTextBox1.ForeColor = cd.Color;
            }
        }

        private void fontToolStripMenuItem_Click(object sender, EventArgs e)
        {
            // new font choosing dialog
            FontDialog fd = new FontDialog();
            // if after showing dialog, user clicked ok
            if (fd.ShowDialog() == DialogResult.OK)
            {
                // set background color to text box
                //fastColoredTextBox1.Font = fd.Font;
            }
        }

        private void cutToolStripMenuItem_Click(object sender, EventArgs e)
        {
           // /fastColoredTextBox1.Cut();
        }

        private void undoToolStripMenuItem_Click(object sender, EventArgs e)
        {
           /// fastColoredTextBox1.Undo();
        }
        /*
        private void redoToolStripMenuItem_Click(object sender, EventArgs e)
        {
            ////fastColoredTextBox1.Redo();
        }   */

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
        

        STTokenizer stTokenizer = new STTokenizer();
        private void fastColoredTextBox1_TextChanged(object sender, TextChangedEventArgs e)
        {
            FastColoredTextBox textBox = sender as FastColoredTextBox;
            //e.ChangedRange.FromLine , e.ChangedRange.ToLine
            if (textBox != null)
            {
                string tekst = textBox.Text;
                Range range = (sender as FastColoredTextBox).VisibleRange;
                //clear style of changed range
                range.ClearStyle(ttIdentifierStyle, ttImmConstantStyle,ttKeywordStyle, ttInvalidStyle, ttOperatorStyle, ttDelimiterStyle, ttCommentStyle, ttUnknownStyle, ttDirectiveStyle, ttWhiteSpaceStyle, ttVarLocDescStyle, ttILLabelStyle, ttVCBlockStyle);
                Range r = new Range(textBox);
                char[] textChars = range.Text.ToCharArray();

                TokenList TokenList = stTokenizer.TokenizeSTStream(textChars, 1);
                foreach (var token in TokenList.Lista)
                {
                    Range tokenRange = fastColoredTextBox1.GetRange(token.Pozycja,(token.Pozycja + token.Tekst.Length));
                        
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
        }




    private void wordWrapToolStripMenuItem_Click(object sender, EventArgs e)
        {
            foreach (TabPage tabPage in tabPage.TabPages)
            {
                var fastColoredTextBox = tabPage.Controls.OfType<FastColoredTextBox>().FirstOrDefault();
                if (fastColoredTextBox != null)
                {
                    if (fastColoredTextBox.WordWrap == false)
                    {
                        fastColoredTextBox.WordWrap = true;
                    }
                    else
                    {
                        fastColoredTextBox.WordWrap = false;
                    }
                }
            }            
        }



        private void fastColoredTextBox1_Load(object sender, EventArgs e)
        {

        }

        private void Form1_Load(object sender, EventArgs e)
        {
            Size mysize = new System.Drawing.Size(20, 20);
            Bitmap bt = new Bitmap(Properties.Resources.close); 
            Bitmap btm = new Bitmap(bt, mysize);
            closeImageAct = btm;

            Bitmap bt2 = new Bitmap(Properties.Resources.closeBlack);
            Bitmap btm2 = new Bitmap(bt2, mysize);
            closeImage = btm2;
            tabPage.Padding = new Point(30);
        }

        private void openFolderToolStripMenuItem_Click(object sender, EventArgs e)
        {

            FolderBrowserDialog folderBrowserDialog1 = new FolderBrowserDialog();
            Cursor.Current = Cursors.WaitCursor;
            tree.Nodes.Clear();
            if (folderBrowserDialog1.ShowDialog() == DialogResult.OK)
            {
                foreach (var item in Directory.GetDirectories(folderBrowserDialog1.SelectedPath))
                {
                    DirectoryInfo directoryInfo = new DirectoryInfo(item);
                    var node = tree.Nodes.Add(directoryInfo.Name, directoryInfo.Name, 0);
                    node.Tag = directoryInfo;
                }

                foreach (var item in Directory.GetFiles(folderBrowserDialog1.SelectedPath))
                {
                    FileInfo fileInfo = new FileInfo(item);
                    var node = tree.Nodes.Add(fileInfo.Name, fileInfo.Name, 1);
                    node.Tag = fileInfo;
                }
                Cursor.Current = Cursors.Default;
            }
               
        }

        private void tree_AfterSelect(object sender, TreeViewEventArgs e)
        {

        }

        private void tree_NodeMouseDoubleClick(object sender, TreeNodeMouseClickEventArgs e)
        {
            if ((e.Node.Tag == null))
            {
                // return
            }
            else if (e.Node.Tag.GetType() == typeof(DirectoryInfo))
            {
                // open folder
                e.Node.Nodes.Clear();
                foreach (var item in Directory.GetDirectories(((DirectoryInfo)e.Node.Tag).FullName))
                {
                    DirectoryInfo directoryInfo = new DirectoryInfo(item);
                    var node = e.Node.Nodes.Add(directoryInfo.Name, directoryInfo.Name, 0, 0);
                    node.Tag = directoryInfo;
                }
                foreach (var item in Directory.GetFiles(((DirectoryInfo)e.Node.Tag).FullName))
                {
                    FileInfo fileInfo = new FileInfo(item);
                    var node = e.Node.Nodes.Add(fileInfo.Name, fileInfo.Name, 0, 0);
                    node.Tag = fileInfo;
                }
                e.Node.Expand();
            }
            else
            {
                // Tworzenie nowej zakładki
                TabPage newTabPage = new TabPage();

                // Tworzenie kontrolki FastColoredTextBox z plikiem
                FastColoredTextBox fastColoredTextBox1 = new FastColoredTextBox();
                fastColoredTextBox1.OpenFile(((FileInfo)e.Node.Tag).FullName);
                
                // Ustawienie właściwości Tag na ścieżkę do pliku
                fastColoredTextBox1.Tag = ((FileInfo)e.Node.Tag).FullName;
                newTabPage.Tag = ((FileInfo)e.Node.Tag).FullName;

                // Dodanie kontrolki FastColoredTextBox do nowej zakładki
                newTabPage.Controls.Add(fastColoredTextBox1);
                AutocompleteMenuNS.AutocompleteMenu acm = new AutocompleteMenuNS.AutocompleteMenu();
                acm.SetAutocompleteMenu(fastColoredTextBox1, acm);
                acm.Items = this.autocompleteMenu1.Items;
                acm.Colors = this.autocompleteMenu1.Colors;
                acm.Font = this.autocompleteMenu1.Font;

                // Ustawienie tekstu zakładki na nazwę pliku
                newTabPage.Text = Path.GetFileName(((FileInfo)e.Node.Tag).FullName);

                // Dodanie nowej zakładki do kontroli TabControl
                tabPage.TabPages.Add(newTabPage);

                // Ustawienie nowej zakładki jako aktualnie wybranej
                tabPage.SelectedTab = newTabPage;

                //obsługa zdarzenia TextChanged
                fastColoredTextBox1.TextChanged += new System.EventHandler<FastColoredTextBoxNS.TextChangedEventArgs>(fastColoredTextBox1_TextChanged);
                fastColoredTextBox1.Dock = DockStyle.Fill;
                fastColoredTextBox1.AutoCompleteBrackets = true;
            }
        }


        private void tabControl1_SelectedIndexChanged(object sender, EventArgs e)
        {
            // Pobranie aktualnie wybranej zakładki
            var selectedTab = tabPage.SelectedTab;
            if (selectedTab != null && selectedTab.Controls.Count > 0)
            {
                var fastColoredTextBox = selectedTab.Controls[0] as FastColoredTextBox;
                if (fastColoredTextBox != null)
                {
                    var filePath = fastColoredTextBox.Tag as string;
                    if (filePath != null)
                    {/*
                        // Pobranie kontrolki FastColoredTextBox z wybranej zakładki
                        FastColoredTextBox fastColoredTextBox1 = selectedTab.Controls.OfType<FastColoredTextBox>().FirstOrDefault();
                        fastColoredTextBox1.Text = File.ReadAllText(filePath);  */
                    }
                }
            }
        }
        
    private void tabPage_DrawItem(object sender, DrawItemEventArgs e)
    {

            Rectangle rect = tabPage.GetTabRect(e.Index);
            Rectangle imageRec = new Rectangle(rect.Right - closeImage.Width,
                rect.Top + (rect.Height - closeImage.Height) / 2,
                closeImage.Width, closeImage.Height);
            // size rect
            rect.Size = new Size(rect.Width + 20, 38);

            Font f;
            Brush br = Brushes.Black;
            StringFormat strF = new StringFormat(StringFormat.GenericDefault);
            if (tabPage.SelectedTab == tabPage.TabPages[e.Index])
            {
                e.Graphics.DrawImage(closeImageAct, imageRec);
                f = new Font("Arial", 10, FontStyle.Bold);
                e.Graphics.DrawString(tabPage.TabPages[e.Index].Text, f, br, rect, strF);
            }
            else
            {
                e.Graphics.DrawImage(closeImage, imageRec);
                f = new Font("Arial", 9, FontStyle.Regular);
                e.Graphics.DrawString(tabPage.TabPages[e.Index].Text, f, br, rect, strF);
            }
        }

        /*
        private void fastColoredTextBox1_AutoIndentNeeded(object sender, AutoIndentEventArgs e)
        {
            if (Regex.IsMatch(e.LineText, @"(\b(?i)(FUNCTION))"))
            {
                e.ShiftNextLines = e.TabLength;
                return;
            }
            if (Regex.IsMatch(e.LineText, @"(\b(?i)(END_FUNCTION))"))
            {
                e.Shift = -e.TabLength;
                e.ShiftNextLines = -e.TabLength;
                return;
            }
        
        }
        */

        private void tabPage_MouseClick(object sender, MouseEventArgs e)
    {
            for (int i = 0; i < tabPage.TabCount; i++)
            {
                Rectangle rect = tabPage.GetTabRect(i);
                Rectangle imageRec = new Rectangle(rect.Right - closeImage.Width,
                    rect.Top + (rect.Height - closeImage.Height) / 2,
                    closeImage.Width, closeImage.Height);

                if (imageRec.Contains(e.Location))
                    tabPage.TabPages.Remove(tabPage.SelectedTab);
            }
        }
   
    
    
    }
}