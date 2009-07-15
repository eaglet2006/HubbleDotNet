using System;
using System.Collections.Generic;
using System.Text;
using Hubble.Framework.DataStructure;
using Hubble.Core.SFQL.LexicalAnalysis;

namespace Hubble.Core.SFQL.SyntaxAnalysis
{
    public class SyntaxException : Exception
    {
        private SyntaxType _SyntaxType;

        public SyntaxType SyntaxType
        {
            get
            {
                return _SyntaxType;
            }
        }

        private string _Word = "";

        private string Word 
        {
            get
            {
                return _Word;
            }
        }

        private string _CurrentSyntax = "";

        private string CurrentSyntax
        {
            get
            {
                return _CurrentSyntax;
            }
        }

        private int _State;

        private int State
        {
            get
            {
                return _State;
            }
        }

        private int _Row;

        private int Row
        {
            get
            {
                return _Row;
            }
        }

        private int _Col;

        private int Col
        {
            get
            {
                return _Col;
            }
        }


        public SyntaxException(string message)
            : base(message)
        {

        }

        public SyntaxException(string message, object curSyntax, DFAException e, LexicalAnalysis.Lexical.Token token)
            : base(message, e)
        {
            _State = e.State;
            
            try
            {
                _SyntaxType = (SyntaxType)e.Action;
            }
            catch
            {

            }

            _Word = token.Text;

            _Row = token.Row;
            _Col = token.Col;

            if (curSyntax != null)
            {
                _CurrentSyntax = curSyntax.GetType().Name;
            }
        }

        public override string ToString()
        {
            return string.Format("{0} at ({1}, {2}) SyntaxType={3} Syntax={4} Word={5}", 
                this.Message, Row, Col, SyntaxType, CurrentSyntax, Word);
        }
    }
}
