using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using System.Diagnostics;

using Hubble.Core.SFQL.LexicalAnalysis;
using Hubble.Core.SFQL.SyntaxAnalysis;
using Hubble.Core.SFQL.Parse;
using Hubble.Framework.DataStructure;

namespace TestSFQL
{
    public partial class Form1 : Form
    {
        int wordNumber = 0;
        TSFQLSentence _SFQLSentence;
        List<TSFQLSentence> _SFQLSentenceList = new List<TSFQLSentence>();

        public Form1()
        {
            InitializeComponent();
            Lexical.Initialize();
        }

        private void AddToken(StringBuilder outputText, Lexical.Token token, bool testPerformance)
        {
            if (testPerformance)
            {
                return;
            }

            wordNumber++;

            outputText.AppendFormat("Word {0}: Type={1} Row={2} Col={3} Text={4}", wordNumber,
                token.SyntaxType.ToString(), token.Row, token.Col, token.Text);
            outputText.AppendLine();
        }

        private void InputLexicalToken(Lexical.Token token, bool testPerformance)
        {
            if (token.SyntaxType == SyntaxType.Space)
            {
                return;
            }

            if (_SFQLSentence == null)
            {
                _SFQLSentence = new TSFQLSentence();
            }

            DFAResult result = _SFQLSentence.Input((int)token.SyntaxType, token);

            switch (result)
            {
                case DFAResult.Quit:
                case DFAResult.ElseQuit:
                    _SFQLSentenceList.Add(_SFQLSentence);
                    _SFQLSentence = null;
                    break;
            }

        }


        private void SyntaxAnalyse(string text, bool testPerformance)
        {
            wordNumber = 0;

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
                            InputLexicalToken(lexical.OutputToken, testPerformance);
                            break;
                        case DFAResult.ElseQuit:
                            InputLexicalToken(lexical.OutputToken, testPerformance);
                            i--;
                            break;
                    }

                }


                dfaResult = lexical.Input(0, text.Length);

                switch (dfaResult)
                {
                    case DFAResult.Continue:
                        throw new Hubble.Core.SFQL.LexicalAnalysis.LexicalException("Lexical abort at the end of sql");
                    case DFAResult.Quit:
                        InputLexicalToken(lexical.OutputToken, testPerformance);
                        break;
                    case DFAResult.ElseQuit:
                        InputLexicalToken(lexical.OutputToken, testPerformance);
                        break;
                }

                InputLexicalToken(new Lexical.Token(), testPerformance);
            }
            catch (LexicalException lexicalEx)
            {
                textBoxOutput.Text = lexicalEx.ToString();
            }
            catch (SyntaxException syntaxEx)
            {
                textBoxOutput.Text = syntaxEx.ToString();
            }
            catch (Exception e1)
            {
                textBoxOutput.Text = e1.Message;
            }
        }

        private void LexicalAnalyse(string text, bool testPerformance)
        {
            wordNumber = 0;

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
                            AddToken(outputText, lexical.OutputToken, testPerformance);
                            break;
                        case DFAResult.ElseQuit:
                            AddToken(outputText, lexical.OutputToken, testPerformance);
                            i--;
                            break;
                    }
                    
                }


                dfaResult = lexical.Input(0, text.Length);

                switch (dfaResult)
                {
                    case DFAResult.Continue:
                        throw new Hubble.Core.SFQL.LexicalAnalysis.LexicalException("Lexical abort at the end of sql");
                    case DFAResult.Quit:
                        AddToken(outputText, lexical.OutputToken, testPerformance);
                        break;
                    case DFAResult.ElseQuit:
                        AddToken(outputText, lexical.OutputToken, testPerformance);
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
            string text = textBoxSFQL.Text;
            LexicalAnalyse(text, false);
        }

        private void buttonLexicalPerformance_Click(object sender, EventArgs e)
        {
            string text = textBoxSFQL.Text;
            Stopwatch sw = new Stopwatch();
            sw.Start();

            for (int i = 0; i < 10000; i++)
            {
                LexicalAnalyse(text, true);
            }
            sw.Stop();
            textBoxOutput.Text = ((double)(sw.ElapsedMilliseconds) / 10000).ToString();
        }

        private void buttonSyntax_Click(object sender, EventArgs e)
        {
            textBoxOutput.Text = "";
            _SFQLSentence = null;

            SyntaxAnalyse(textBoxSFQL.Text, false);
        }

        private void buttonParse_Click(object sender, EventArgs e)
        {
            SFQLParse parse = new SFQLParse();
            parse.Query(textBoxSFQL.Text);
        }
    }
}
