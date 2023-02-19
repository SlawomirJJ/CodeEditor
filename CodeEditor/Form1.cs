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
            saveFileDialog.Filter = "Text File|*.txt|Any File|*.*";
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
            openFileDialog.Filter = "text File|*.txt|Any File|*.*";
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
                OpenDlg();
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
            if(cd.ShowDialog() == DialogResult.OK)
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

        

        private void Form1_Load(object sender, EventArgs e)
        {

        }

        private void fastColoredTextBox1_Load(object sender, EventArgs e)
        {

        }

        // Style
        public Style GreenStyle = new TextStyle(Brushes.Green, null, FontStyle.Bold);
        public Style KeyWordStyle = new TextStyle(Brushes.Blue, null, FontStyle.Bold);
        public Style StringStyle = new TextStyle(Brushes.Red, null, FontStyle.Bold);
        //private bool multilineCommand;

        private void fastColoredTextBox1_TextChanged(object sender, TextChangedEventArgs e)
        {
            Range range = (sender as FastColoredTextBox).VisibleRange;
            //clear style of changed range
            range.ClearStyle(GreenStyle, StringStyle);
            //comment highlighting
            //e.ChangedRange.SetStyle(GreenStyle, @"//.*$", RegexOptions.Multiline);
            //e.ChangedRange.SetStyle(GreenStyle, @"/\*.*", RegexOptions.Multiline);
            

            range.SetStyle(GreenStyle, @"//.*$", RegexOptions.Multiline);
            range.SetStyle(GreenStyle, @"(/\*.*?\*/)|(/\*.*)", RegexOptions.Singleline);
            //range.SetStyle(GreenStyle, @"(/\*.*?\*/)|(.*\*/)", RegexOptions.Singleline | RegexOptions.RightToLeft);
            
            // string highlighting
            range.SetStyle(StringStyle, "(\".*?\")|(\".*)", RegexOptions.Singleline);

            // key words
            range.SetStyle(StringStyle, "()", RegexOptions.Singleline);
        }
    }
}
