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



namespace CodeEditor
{
    public partial class Form1 : Form
    {
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
                sw.Write(fastColoredTextBox1.Text);
                sw.Close();
            }
        }

        private void saveAsToolStripMenuItem_Click(object sender, EventArgs e)
        {
            SaveDlg();
        }

        private void newToolStripMenuItem_Click(object sender, EventArgs e)
        {
            fastColoredTextBox1.Text = "";
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
                fastColoredTextBox1.Text = sr.ReadToEnd();
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
            try
            {
                // save file
                StreamWriter sw = new StreamWriter(this.Text);
                sw.WriteLine(fastColoredTextBox1.Text);
                sw.Close();
            }
            catch
            {

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
            fastColoredTextBox1.Paste();
        }

        private void backgroundColorToolStripMenuItem_Click(object sender, EventArgs e)
        {
            // new color choosing dialog
            ColorDialog cd = new ColorDialog();
            // if after showing dialog, user clicked ok
            if (cd.ShowDialog() == DialogResult.OK)
            {
                fastColoredTextBox1.BackColor = cd.Color;
            }
        }

        private void textColorToolStripMenuItem_Click(object sender, EventArgs e)
        {
            // new color choosing dialog
            ColorDialog cd = new ColorDialog();
            // if after showing dialog, user clicked ok
            if (cd.ShowDialog() == DialogResult.OK)
            {
                fastColoredTextBox1.ForeColor = cd.Color;
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
                fastColoredTextBox1.Font = fd.Font;
            }
        }

        private void cutToolStripMenuItem_Click(object sender, EventArgs e)
        {
            fastColoredTextBox1.Cut();
        }

        private void undoToolStripMenuItem_Click(object sender, EventArgs e)
        {
            fastColoredTextBox1.Undo();
        }

        private void redoToolStripMenuItem_Click(object sender, EventArgs e)
        {
            fastColoredTextBox1.Redo();
        }

        // Style
        public Style commentStyle = new TextStyle(Brushes.Green, null, FontStyle.Bold);
        public Style KeyWordStyle = new TextStyle(Brushes.Blue, null, FontStyle.Bold);
        public Style StringStyle = new TextStyle(Brushes.Red, null, FontStyle.Bold);
        public Style PurpleStyle = new TextStyle(Brushes.Purple, null, FontStyle.Bold);
        public Style NumberStyle = new TextStyle(Brushes.Olive, null, FontStyle.Regular);
        public Style OperatorStyle = new TextStyle(Brushes.MidnightBlue, null, FontStyle.Regular);
        public Style DataTypeStyle = new TextStyle(Brushes.SaddleBrown, null, FontStyle.Regular);

        //private bool multilineCommand;



        private void fastColoredTextBox1_TextChanged(object sender, TextChangedEventArgs e)
       {
            Range range = (sender as FastColoredTextBox).VisibleRange;
            fastColoredTextBox1.Focus(); // ??????????????????
            //clear style of changed range
            range.ClearStyle(commentStyle, StringStyle, KeyWordStyle);

            //comment highlighting
            range.SetStyle(commentStyle, @"//.*$", RegexOptions.Multiline);
            range.SetStyle(commentStyle, @"(/\*.*?\*/)|(/\*.*)", RegexOptions.Singleline);
            range.SetStyle(commentStyle, @"(\(\*.*?\*\))|(\(\*.*)", RegexOptions.Singleline);
            //range.SetStyle(GreenStyle, @"(/\*.*?\*/)|(.*\*/)", RegexOptions.Singleline | RegexOptions.RightToLeft);

            // string highlighting
            range.SetStyle(StringStyle, "(\'.*?\')|(\'.*)", RegexOptions.Singleline);

            // key words
            range.SetStyle(KeyWordStyle, @"\b((?i)((REPEAT)|(END_REPEAT)|(UNTIL)|(IF)|(ELSIF)|(ELSE)|(THEN)|(EXIT)|(END_IF)|(WHILE)|(DO)|(END_WHILE)|(FOR)|TO|(BY)|(DO)|(END_FOR)|(CASE)|(END_CASE)|(OF)|(PROGRAM)||(VAR)|(END_VAR)|(ARRAY)|(CMD_TMR)|(IN)|(PT)|(EXIT)|(TYPE)|(END_TYPE)|()))\b", RegexOptions.Singleline);


            // bool
            range.SetStyle(NumberStyle, @"\b((?i)(TRUE|FALSE))\b", RegexOptions.Singleline);
            // numbers
            range.SetStyle(NumberStyle, @"\b(\d+(\.\d+)?)\b");
            range.SetStyle(NumberStyle, @"(-\d+)");

            // TIME
            // duration of time time
            range.SetStyle(NumberStyle, @"(?i)((T|TIME)#\d+(d|h|s|(ms)|m)((\d+)(d|h|(ms)|s|m)){0,4})");
            // calendar date
            range.SetStyle(NumberStyle, @"(?i)((T|TIME)#(\d+)-(\d\d)-(\d\d)-(\d\d)?)");
            // time of day
            range.SetStyle(NumberStyle, @"(?i)(TDD|TIME_OF_DAY)#(\d\d):(\d\d):(\d\d)(.(\d\d))?");
            //date and time of day
            range.SetStyle(NumberStyle, @"(?i)(DT|DATE_AND_TIME)#(\d+)-(\d\d)-(\d\d)-((\d\d):(\d\d):(\d\d)(.(\d+))?)");

            // data types style
            range.SetStyle(DataTypeStyle, @"\b((?i)(SINT|INT|DINT|LINT|USINT|UINT|LDINT|ULINT|REAL|LREAL|TIME|DATE|TIME_OF_DAY|DATE_AND_TIME|STRING|BOOL|BYTE|WORD|DWORD|LWORD))\b", RegexOptions.Singleline);

            // operators and ; : [] () {}
            range.SetStyle(OperatorStyle, @"((?i)(\(|\)|(NOT)|\*|(\*\*)|\/|(MOD)|\+|\=|\-|<|>|(<=)|(>=)|(<>)|&|(AND)|(XOR)|(OR)|(:=)|;|:|\[|\]|\{|\}))", RegexOptions.Singleline);

            // Code Folding
            fastColoredTextBox1.CollapseBlock (fastColoredTextBox1.Selection.Start.iLine,
               fastColoredTextBox1.Selection.End.iLine);

            //clear folding markers of changed range
            e.ChangedRange.ClearFoldingMarkers();
            //set folding markers
            e.ChangedRange.SetFoldingMarkers("{", "}");
            e.ChangedRange.SetFoldingMarkers(@"#region\b", @"#endregion\b");
        }

        


        private void wordWrapToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (fastColoredTextBox1.WordWrap == false)
            {
                fastColoredTextBox1.WordWrap = true;
            }
            else
            {
                fastColoredTextBox1.WordWrap = false;
            } 

        }

        // ToolTip ????????????
        private void fastColoredTextBox1_ToolTipNeeded(object sender, ToolTipNeededEventArgs e)
        {

        }

        private void fastColoredTextBox1_Load(object sender, EventArgs e)
        {

        }

        private void Form1_Load(object sender, EventArgs e)
        {

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
                //open file
                fastColoredTextBox1.Text = File.ReadAllText(((FileInfo)e.Node.Tag).FullName);
            }

        }

        private void pictureBox1_Click(object sender, EventArgs e)
        {

        }

        private void btnMin_Click(object sender, EventArgs e)
        {
            this.WindowState = FormWindowState.Minimized;
        }

    }
    
}