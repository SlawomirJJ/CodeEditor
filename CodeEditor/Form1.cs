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
        public Style commentStyle = new TextStyle(Brushes.Green, null, FontStyle.Bold);
        public Style KeyWordStyle = new TextStyle(Brushes.Blue, null, FontStyle.Bold);
        public Style StringStyle = new TextStyle(Brushes.Red, null, FontStyle.Bold);
        //public Style PurpleStyle = new TextStyle(Brushes.Purple, null, FontStyle.Bold);
        public Style NumberStyle = new TextStyle(Brushes.Olive, null, FontStyle.Regular);
        public Style OperatorStyle = new TextStyle(Brushes.MidnightBlue, null, FontStyle.Regular);
        public Style DataTypeStyle = new TextStyle(Brushes.SaddleBrown, null, FontStyle.Regular);


        private void fastColoredTextBox1_TextChanged(object sender, TextChangedEventArgs e)
        {
            FastColoredTextBox textBox = sender as FastColoredTextBox;
            if (textBox != null)
            {
                Range range = (sender as FastColoredTextBox).VisibleRange;
                //clear style of changed range
                range.ClearStyle(commentStyle, KeyWordStyle, StringStyle, NumberStyle, OperatorStyle, DataTypeStyle);

                //comments
                range.SetStyle(commentStyle, @"//.*$", RegexOptions.Multiline);
                range.SetStyle(commentStyle, @"(/\*.*?\*/)|(/\*.*)", RegexOptions.Singleline);
                range.SetStyle(commentStyle, @"(\(\*.*?\*\))|(\(\*.*)", RegexOptions.Singleline);

                // strings
                range.SetStyle(StringStyle, "(\'.*?\')|(\'.*)", RegexOptions.Singleline);
                range.SetStyle(StringStyle, "(\".*?\")|(\".*)", RegexOptions.Singleline);

                // key words
                range.SetStyle(KeyWordStyle, @"\b((?i)((PROGRAM)|(END_PROGRAM)|(VAR)|(VAR_INPUT)|(VAR_OUTPUT)|(VAR_IN_OUT)|(VAR_EXTERNAL)|(VAR_GLOBAL)|(VAR_ACCESS)|(VAR_TEMP)|(VAR_CONFIG)|(END_VAR)|(RETAIN)|(NON_RETAIN)|(PROTECTED)|(PUBLIC)|(PRIVATE)|(INTERNAL)|(CONSTANT)|(IF)|(ELSIF)|(THEN)|(ELSE)|(END_IF)|(CASE)|(OF)|(END_CASE)|(FOR)|(TO)|(BY)|(DO)|(END_FOR)|(EXIT)|(RETURN)|(WHILE)|(END_WHILE)|(REPEAT)|(UNTIL)|(END_REPEAT)|(TYPE)|(END_TYPE)|(ARRAY)|(STRUCT)|(END_STRUCT)|(OVERLAP)|(AT)|(REF_TO)|(REF)|(FUNCTION)|(END_FUNCTION)|(FUNCTION_BLOCK)|(END_FUNCTION_BLOCK)|(CLASS)|(END_CLASS)|(FINAL)|(METHOD)|(END_METHOD)|(EXTENDS)|(OVERRIDE)|(ABSTRACT)|(THIS)|(SUPER)|(INTERFACE)|(END_INTERFACE)|(IMPLEMENTS)|(READ_WRITE)|(READ_ONLY)|(NAMESPACE)|(END_NAMESPACE)))\b", RegexOptions.Singleline);

                ///         DATA TYPES      /// 
                range.SetStyle(DataTypeStyle, @"\b((?i)((SINT)|(INT)|(DINT)|(LINT)|(USINT)|(UINT)|(UDINT)|(LDINT)|(ULINT)|(REAL)|(LREAL)|(TIME)|(DATE)|(TIME_OF_DAY)|(TOD)|(LTIME_OF_DAY)|(LTOD)|(DATE_AND_TIME)|(DT)|(LDATE_AND_TIME)|(LDT)||(STRING)|(BOOL)|(R_EDGE)|(F_EDGE)|(BYTE)|(WORD)|(DWORD)|(LWORD)|(LTIME)|(LDATE)|(WSTRING)|(CHAR)|(WCHAR)))\b", RegexOptions.Singleline);

                // bool
                range.SetStyle(NumberStyle, @"\b((?i)(TRUE|FALSE))\b", RegexOptions.Singleline);
                // numbers
                range.SetStyle(NumberStyle, @"\b(\d+(\.\d+)?)\b", RegexOptions.Singleline);
                range.SetStyle(NumberStyle, @"(\-(\d+))", RegexOptions.Singleline);

                // TIME
                // duration of time
                range.SetStyle(NumberStyle, @"(?i)((T|TIME)#\d+(d|h|s|(ms)|m)((\d+)(d|h|(ms)|s|m)){0,4})");
                // calendar date
                range.SetStyle(NumberStyle, @"(?i)((T|TIME)#(\d+)-(\d\d)-(\d\d)-(\d\d)?)");
                // time of day
                range.SetStyle(NumberStyle, @"(?i)(TDD|TIME_OF_DAY)#(\d\d):(\d\d):(\d\d)(.(\d\d))?");
                //date and time of day
                range.SetStyle(NumberStyle, @"(?i)(DT|DATE_AND_TIME)#(\d+)-(\d\d)-(\d\d)-((\d\d):(\d\d):(\d\d)(.(\d+))?)");

                // data types style
                //range.SetStyle(DataTypeStyle, @"\b((?i)(SINT|INT|DINT|LINT|USINT|UINT|LDINT|ULINT|REAL|LREAL|TIME|DATE|TIME_OF_DAY|DATE_AND_TIME|STRING|BOOL|BYTE|WORD|DWORD|LWORD))\b", RegexOptions.Singleline);

                // operators and special characters
                range.SetStyle(OperatorStyle, @"((?i)(\(|\)|(NOT)|\*|(\*\*)|\/|(MOD)|\+|\=|\-|<|>|(<=)|(>=)|(<>)|&|(AND)|(XOR)|(OR)|(:=)|;|:|\.|,|\[|\]|\{|\}|#|\^|%))", RegexOptions.Singleline);

                // Code Folding
                //fastColoredTextBox1.CollapseBlock (fastColoredTextBox1.Selection.Start.iLine,
                //fastColoredTextBox1.Selection.End.iLine);

                //clear folding markers of changed range
                //e.ChangedRange.ClearFoldingMarkers();
                //set folding markers


                var currentSelection = fastColoredTextBox1.Selection.Clone();
                currentSelection.Normalize();


                        const string startMarker = "FUNCTION";
                        const string endMarker = "END_FUNCTION";

                        fastColoredTextBox1[currentSelection.Start.iLine].FoldingStartMarker = startMarker;
                        fastColoredTextBox1[currentSelection.End.iLine].FoldingEndMarker = endMarker;
                        fastColoredTextBox1.Invalidate();



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