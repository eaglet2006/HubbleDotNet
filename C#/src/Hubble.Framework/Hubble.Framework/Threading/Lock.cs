using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;

namespace Hubble.Framework.Threading
{
    /// <summary>
    /// Share or Mutex lock
    /// </summary>
    public class Lock
    {
        public enum Mode
        {
            Share = 0,
            Mutex = 1,
        }

        enum State
        {
            Share = 0,
            Mutex = 1,
        }

        State _State = State.Share;

        int _ShareCounter = 0;

        public void Enter(Mode mode)
        {
            bool waitShareCounterZero = false;
            bool waitForShareState = false;
        Loop:
            lock (this)
            {
                switch (mode)
                {
                    case Mode.Share:
                        switch (_State)
                        {
                            case State.Share:
                                _ShareCounter++;
                                return;
                            case State.Mutex:
                                waitForShareState = true;
                                break;
                        }
                        break;

                    case Mode.Mutex:
                        switch (_State)
                        {
                            case State.Share:
                                waitShareCounterZero = true;
                                _State = State.Mutex;
                                break;
                            case State.Mutex:
                                waitForShareState = true;
                                break;
                        }
                        break;
                }
            }

            if (waitShareCounterZero)
            {
                int counter;
                int times = 0;
                do
                {
                    lock (this)
                    {
                        counter = _ShareCounter;
                    }

                    if (counter > 0)
                    {
                        if (times++ < 10)
                        {
                            Thread.Sleep(0);
                        }
                        else
                        {
                            Thread.Sleep(1);
                        }
                    }

                } while (counter > 0);
            }
            else if (waitForShareState)
            {
                int times = 0;
                State state;

                do
                {
                    lock (this)
                    {
                        state = _State;
                    }

                    if (state != State.Share)
                    {
                        if (times++ < 10)
                        {
                            Thread.Sleep(0);
                        }
                        else
                        {
                            Thread.Sleep(1);
                        }
                    }
                } while (state != State.Share);
                waitShareCounterZero = false;
                waitForShareState = false;
                goto Loop;
            }
        }

        public void Leave()
        {
            lock (this)
            {
                if (_ShareCounter > 0)
                {
                    _ShareCounter--;
                    return;
                }

                if (_State == State.Mutex)
                {
                    _State = State.Share;
                }
            }
        }
    }
}
