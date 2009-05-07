using System;
using System.Collections.Generic;
using System.Text;
using Hubble.Framework.Threading;

namespace Hubble.Core.Store
{
    [Serializable]
    public class PayloadFileHead
    {
        int[] _Version;

        byte[] _FieldsMD5;

        public int[] Version
        {
            get
            {
                return _Version;
            }

            set
            {
                _Version = value;
            }
        }

        public byte[] FieldMD5
        {
            get
            {
                return _FieldsMD5;
            }

            set
            {
                _FieldsMD5 = value;
            }
        }

        public void Build(List<Data.Field> fields)
        {
            System.Security.Cryptography.MD5CryptoServiceProvider _MD5 =
                new System.Security.Cryptography.MD5CryptoServiceProvider();

            StringBuilder str = new StringBuilder();

            foreach (Data.Field field in fields)
            {
                str.AppendLine(field.Name.ToLower());
                str.AppendLine(field.TabIndex.ToString());
                str.AppendLine(field.DataType.ToString());
                str.AppendLine(field.DataLength.ToString());
                str.AppendLine(field.IndexType.ToString());
            }

            string key = str.ToString();

            byte[] b = new byte[key.Length * 2];

            for (int i = 0; i < key.Length; i++)
            {
                char c = key[i];
                b[2 * i] = (byte)(c % 256);
                b[2 * i + 1] = (byte)(c / 256);
            }

            _FieldsMD5 = _MD5.ComputeHash(b);
            _Version = Hubble.Framework.Reflection.Assembly.GetVersionValue(
                Hubble.Framework.Reflection.Assembly.GetCallingAssemblyVersion());
        }

    }

    class PayloadFile : MessageQueue
    {
        const int HeadLength = 4 * 1024;

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

        class TabCompare : IComparer<Data.Field>
        {
            #region IComparer<Field> Members

            public int Compare(Hubble.Core.Data.Field x, Hubble.Core.Data.Field y)
            {
                return x.TabIndex.CompareTo(y.TabIndex);
            }

            #endregion
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

                        int fileIndex = (int)((fs.Length - HeadLength) / payloadLen); 

                        foreach (PayloadEntity pe in _PayloadEntities)
                        {
                            byte[] buf = BitConverter.GetBytes(pe.DocId);
                            fs.Write(buf, 0, buf.Length);

                            pe.Payload.CopyTo(data);

                            fs.Write(data, 0, data.Length);

                            pe.Payload.FileIndex = fileIndex;
                            fileIndex++;
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
        }

        public Dictionary<long, Data.Payload> Open(List<Data.Field> fields, int payloadLength, out long lastDocId)
        {
            lastDocId = 0;
            List<Data.Field> tmpFields = new List<Hubble.Core.Data.Field>();
            Dictionary<long, Data.Payload> docPayload = new Dictionary<long, Hubble.Core.Data.Payload>();

            foreach (Data.Field field in fields)
            {
                if (field.IndexType != Hubble.Core.Data.Field.Index.None)
                {
                    tmpFields.Add(field);
                }
            }

            tmpFields.Sort(new TabCompare());

            PayloadFileHead head = new PayloadFileHead();

            head.Build(tmpFields);

            using (System.IO.FileStream fs = new System.IO.FileStream(_FileName, System.IO.FileMode.Open, System.IO.FileAccess.Read))
            {
                object obj;
                Hubble.Framework.Serialization.BinSerialization.DeserializeBinary(fs, out obj);

                PayloadFileHead fileHead = (PayloadFileHead)obj;

                for (int i = 0; i < head.FieldMD5.Length; i++)
                {
                    if (head.FieldMD5[i] != fileHead.FieldMD5[i])
                    {
                        throw new Data.DataException(string.Format("Payload file name: {0} does not match with the table",
                            _FileName));
                    }
                }

                fs.Seek(HeadLength, System.IO.SeekOrigin.Begin);

                while (fs.Position < fs.Length)
                {
                    int fileIndex = (int)((fs.Length - HeadLength) / payloadLength); 

                    byte[] buf = new byte[sizeof(long)];

                    fs.Read(buf, 0, buf.Length);

                    lastDocId = BitConverter.ToInt64(buf, 0);

                    byte[] byteData = new byte[payloadLength * 4];
                    fs.Read(byteData, 0, byteData.Length);

                    Data.Payload payload = new Hubble.Core.Data.Payload(payloadLength);
                    
                    payload.CopyFrom(byteData);

                    payload.FileIndex = fileIndex;

                    docPayload.Add(lastDocId, payload);
                }

                return docPayload;
            }
        }

        public void Create(List<Data.Field> fields)
        {
            List<Data.Field> tmpFields = new List<Hubble.Core.Data.Field>();

            foreach (Data.Field field in fields)
            {
                if (field.IndexType != Hubble.Core.Data.Field.Index.None)
                {
                    tmpFields.Add(field);
                }
            }

            tmpFields.Sort(new TabCompare());

            PayloadFileHead head = new PayloadFileHead();

            head.Build(tmpFields);

            if (System.IO.File.Exists(_FileName))
            {
                System.IO.File.Delete(_FileName);
            }

            using (System.IO.FileStream fs = new System.IO.FileStream(_FileName, System.IO.FileMode.CreateNew, System.IO.FileAccess.ReadWrite))
            {
                Hubble.Framework.Serialization.BinSerialization.SerializeBinary(head, fs);
                fs.SetLength(HeadLength);
            }
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
