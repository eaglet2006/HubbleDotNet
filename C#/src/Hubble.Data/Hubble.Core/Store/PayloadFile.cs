using System;
using System.Collections.Generic;
using System.Text;
using Hubble.Framework.Threading;

namespace Hubble.Core.Store
{
    class PayloadFile : MessageQueue
    {
        enum Event
        {
            Add = 1,
            Collect = 2,
        }

        class PayloadEntity
        {
            public long DocId;

            public Hubble.Core.Data.Payload Payload;

            public PayloadEntity(long docId, Hubble.Core.Data.Payload payload)
            {
                DocId = docId;
                Payload = payload;
            }
        }

        string _FileName;
        List<PayloadEntity> _PayloadEntities = new List<PayloadEntity>();

        private object ProcessMessage(int evt, MessageFlag flag, object data)
        {
            switch ((Event)evt)
            {
                case Event.Add:
                    _PayloadEntities.Add((PayloadEntity)data);
                    break;
                case Event.Collect:
                    SaveToFile();
                    break;
            }

            return null;
        }

        private void SaveToFile()
        {
            try
            {
                if (_PayloadEntities.Count > 0)
                {
                    using (System.IO.FileStream fs = new System.IO.FileStream(_FileName, System.IO.FileMode.Append, System.IO.FileAccess.Write))
                    {
                        fs.Seek(0, System.IO.SeekOrigin.End);

                        int payloadLen = _PayloadEntities[0].Payload.Data.Length;
                        byte[] data = new byte[payloadLen * 4];


                        foreach (PayloadEntity pe in _PayloadEntities)
                        {
                            byte[] buf = BitConverter.GetBytes(pe.DocId);
                            fs.Write(buf, 0, buf.Length);

                            pe.Payload.CopyTo(data);

                            fs.Write(data, 0, data.Length);
                        }

                        _PayloadEntities.Clear();
                    }
                }
            }
            catch(Exception e)
            {
                Global.Report.WriteErrorLog("PayloadFile.SaveToFile fail!", e);
            }
        }


        public PayloadFile(string fileName)
        {
            _FileName = fileName;
            OnMessageEvent = ProcessMessage;
            this.Start();
        }

        public void Add(long docId, Hubble.Core.Data.Payload payLoad)
        {
            ASendMessage((int)Event.Add, new PayloadEntity(docId, payLoad));
        }

        public void Collect()
        {
            ASendMessage((int)Event.Collect, null);
        }
    }
}
