using System;
using System.Collections.Generic;
using System.Text;
using Hubble.Framework.DataStructure;

namespace Hubble.Core.SFQL.LexicalAnalysis
{
    public class LexicalException : Exception
    {
        private char _CurrentChar;

        public char CurrentChar
        {
            get
            {
                return _CurrentChar;
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


        public LexicalException(string message)
            : base(message)
        {

        }

        public LexicalException(string message, DFAException e, int row, int col)
            : base(message, e)
        {
            _State = e.State;
            _CurrentChar = (char)e.Action;
            _Row = row;
            _Col = col;
        }

        public override string ToString()
        {
            return string.Format("{0} at ({1}, {2}) CurrentChar={3} ", this.Message, Row, Col, CurrentChar);
        }
    }
}
