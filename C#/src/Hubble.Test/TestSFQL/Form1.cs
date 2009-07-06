using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using System.Diagnostics;

using Hubble.Core.SFQL.LexicalAnalysis;
using Hubble.Framework.DataStructure;

namespace TestSFQL
{
    public partial class Form1 : Form
    {
        int wordNumber = 0;

        public Form1()
        {
            InitializeComponent();
            Lexical.Initialize();
        }

        private void AddToken(StringBuilder outputText, Lexical.Token token)
        {
            wordNumber++;

            outputText.AppendFormat("Word {0}: Type={1} Row={2} Col={3} Text={4}", wordNumber,
                token.SyntaxType.ToString(), token.Row, token.Col, token.Text);
            outputText.AppendLine();
        }

        private void LexicalAnalyse()
        {
            wordNumber = 0;
            string text = textBoxSFQL.Text;

            Lexical lexical = new Lexical(text);

            StringBuilder outputText = new StringBuilder();

            try
            {
                DFAResult dfaResult;

                for (int i = 0; i < text.Length; i++)
                {
                    dfaResult = lexical.Input(text[i], i);

                    switch (dfaResult)
                    {
                        case DFAResult.Continue:
                            continue;
                        case DFAResult.Quit:
                            AddToken(outputText, lexical.OutputToken);
                            break;
                        case DFAResult.ElseQuit:
                            AddToken(outputText, lexical.OutputToken);
                            i--;
                            break;
                    }
                }


                dfaResult = lexical.Input(0, text.Length);

                switch (dfaResult)
                {
                    case DFAResult.Continue:
                        throw new DFAException("Lexical abort at the end of sql");
                    case DFAResult.Quit:
                        AddToken(outputText, lexical.OutputToken);
                        break;
                    case DFAResult.ElseQuit:
                        AddToken(outputText, lexical.OutputToken);
                        break;
                }

                textBoxOutput.Text = outputText.ToString();
            }
            catch (Exception e1)
            {
                textBoxOutput.Text = e1.Message;
            }
        }

        private void buttonLexical_Click(object sender, EventArgs e)
        {
            //Stopwatch sw = new Stopwatch();
            //sw.Start();

            //for (int i = 0; i < 1000; i++)
            //{
            //    LexicalAnalyse();
            //}
            //sw.Stop();
            //textBoxOutput.Text = sw.ElapsedMilliseconds.ToString();

            LexicalAnalyse();
        }
    }
}
