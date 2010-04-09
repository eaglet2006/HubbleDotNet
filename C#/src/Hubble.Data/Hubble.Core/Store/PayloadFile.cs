/*
 * Licensed to the Apache Software Foundation (ASF) under one or more
 * contributor license agreements.  See the NOTICE file distributed with
 * this work for additional information regarding copyright ownership.
 * The ASF licenses this file to You under the Apache License, Version 2.0
 * (the "License"); you may not use this file except in compliance with
 * the License.  You may obtain a copy of the License at
 * 
 * http://www.apache.org/licenses/LICENSE-2.0
 * 
 * Unless required by applicable law or agreed to in writing, software
 * distributed under the License is distributed on an "AS IS" BASIS,
 * WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
 * See the License for the specific language governing permissions and
 * limitations under the License.
 */

using System;
using System.Collections.Generic;
using System.Text;
using Hubble.Framework.Threading;
using Hubble.Core.Data;

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
            Update = 3,
        }

        class PayloadEntity : IComparable<PayloadEntity>
        {
            public int DocId;
            public PayloadProvider PayloadProvider;
            public Hubble.Core.Data.Payload Payload;

            public PayloadEntity(int docId, Hubble.Core.Data.Payload payload, PayloadProvider payloadProvider)
            {
                DocId = docId;
                Payload = payload;
                PayloadProvider = payloadProvider;
            }

            #region IComparable<PayloadEntity> Members

            public int CompareTo(PayloadEntity other)
            {
                if (other == null)
                {
                    return 1;
                }

                if (this.Payload.FileIndex > other.Payload.FileIndex)
                {
                    return 1;
                }
                else if (this.Payload.FileIndex < other.Payload.FileIndex)
                {
                    return -1;
                }
                else
                {
                    return 0;
                }
            }

            #endregion
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
        int _StoreLength = 0;
        int _DocumentsCount = 0;
        int _LastStoredId = -1;

        /// <summary>
        /// The last docid that is stored 
        /// </summary>
        internal int LastStoredId
        {
            get
            {
                return _LastStoredId;
            }
        }

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
                case Event.Update:
                    UpdateToFile((List<PayloadEntity>)data);
                    break;
            }

            return null;
        }

        private void UpdateToFile(List<PayloadEntity> peList)
        {
            if (_StoreLength <= 0)
            {
                //No data
                return;
            }

            if (peList.Count <= 0)
            {
                return;
            }

            peList.Sort();

            int payloadLen = peList[0].Payload.Data.Length;
            byte[] data = new byte[payloadLen * 4];

            using (System.IO.FileStream fs = new System.IO.FileStream(_FileName, System.IO.FileMode.Open, System.IO.FileAccess.Write))
            {
                foreach (PayloadEntity pe in peList)
                {
                    if (pe.Payload.FileIndex < 0)
                    {
                        throw new Data.DataException("Payload fileIndex < 0!");
                    }

                    fs.Seek(pe.Payload.FileIndex * _StoreLength + HeadLength + sizeof(int), System.IO.SeekOrigin.Begin);

                    pe.Payload.CopyTo(data);
                    fs.Write(data, 0, data.Length);
                }
            }
        }

        private void CheckPayloadEntities()
        {
            int lastDocId = this.LastStoredId;

            for (int i = 0; i < _PayloadEntities.Count; i++)
            {
                if (_PayloadEntities[i].DocId <= lastDocId)
                {
                    throw new DataException(string.Format("DocId:{0} less then or equal with the last docid:{1}",
                        _PayloadEntities[i].DocId, lastDocId));
                }

                lastDocId = _PayloadEntities[i].DocId;
            }
        }

        private void SaveToFile()
        {
            try
            {
                if (_PayloadEntities.Count > 0)
                {
                    CheckPayloadEntities(); //Check docid is serialization.

                    int payloadLen = _PayloadEntities[0].Payload.Data.Length;
                    byte[] data = new byte[payloadLen * 4];

                    if (_StoreLength <= 0)
                    {
                        _StoreLength = data.Length + sizeof(int);
                    }

                    if (data.Length == 0)
                    {
                        _DocumentsCount += _PayloadEntities.Count;

                        using (System.IO.FileStream fs = new System.IO.FileStream(_FileName, System.IO.FileMode.Open, System.IO.FileAccess.Write))
                        {
                            //Seek to end of head
                            fs.Seek(HeadLength, System.IO.SeekOrigin.Begin);
                            
                            //Write last doc id
                            _LastStoredId = (int)_PayloadEntities[_PayloadEntities.Count - 1].DocId;
                            byte[] buf = BitConverter.GetBytes(_LastStoredId);
                            fs.Write(buf, 0, buf.Length);
                            
                            //Write documents count
                            buf = BitConverter.GetBytes((int)_DocumentsCount);
                            fs.Write(buf, 0, buf.Length);
                        }

                        _PayloadEntities.Clear();

                        return;
                    }


                    using (System.IO.FileStream fs = new System.IO.FileStream(_FileName, System.IO.FileMode.Append, System.IO.FileAccess.Write))
                    {
                        fs.Seek(0, System.IO.SeekOrigin.End);

                        int fileIndex = (int)((fs.Length - HeadLength) / _StoreLength);

                        foreach (PayloadEntity pe in _PayloadEntities)
                        {
                            byte[] buf = BitConverter.GetBytes((int)pe.DocId);
                            _LastStoredId = pe.DocId;
                            fs.Write(buf, 0, buf.Length);

                            pe.Payload.CopyTo(data);

                            fs.Write(data, 0, data.Length);

                            pe.PayloadProvider.SetFileIndex(pe.DocId, fileIndex);
                            //pe.Payload.FileIndex = fileIndex;
                            fileIndex++;
                        }

                        _DocumentsCount += _PayloadEntities.Count;

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

        internal int DocumentsCount
        {
            get
            {
                return _DocumentsCount;
            }
        }

        internal PayloadProvider Open(Field docIdReplaceField, List<Data.Field> fields, int payloadLength, out int lastDocId)
        {
            lastDocId = -1;
            List<Data.Field> tmpFields = new List<Hubble.Core.Data.Field>();
            PayloadProvider docPayload = new PayloadProvider(docIdReplaceField);

            long truncateLength = 0;

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

            if (_StoreLength <= 0)
            {
                _StoreLength = payloadLength * 4 + sizeof(int);
            }

            using (System.IO.FileStream fs = new System.IO.FileStream(_FileName, System.IO.FileMode.Open, System.IO.FileAccess.Read))
            {
                object obj;
                Hubble.Framework.Serialization.BinSerialization.DeserializeBinary(fs, out obj);

                PayloadFileHead fileHead = (PayloadFileHead)obj;

                if (fileHead.Version[1] < 8 || (fileHead.Version[1] == 8 && fileHead.Version[2] == 0 && fileHead.Version[3] < 4))
                {
                    throw new Data.DataException("Index file version is less then V0.8.0.4, you have to rebuild the index");
                }

                for (int i = 0; i < head.FieldMD5.Length; i++)
                {
                    if (head.FieldMD5[i] != fileHead.FieldMD5[i])
                    {
                        throw new Data.DataException(string.Format("Payload file name: {0} does not match with the table",
                            _FileName));
                    }
                }

                fs.Seek(HeadLength, System.IO.SeekOrigin.Begin);
                bool breakForProctect = false; 

                while (fs.Position < fs.Length)
                {
                    if (payloadLength == 0 && InsertProtect.InsertProtectInfo != null)
                    {
                        //Exit exception last time
                        lastDocId = InsertProtect.InsertProtectInfo.LastDocId;
                        _DocumentsCount = InsertProtect.InsertProtectInfo.DocumentsCount;
                        break;
                    }


                    int fileIndex = (int)((fs.Position - HeadLength) / _StoreLength);

                    byte[] buf = new byte[sizeof(int)];

                    fs.Read(buf, 0, buf.Length);

                    int docidInFile = (int)BitConverter.ToInt32(buf, 0);
                    if (docidInFile < lastDocId)
                    {
                        throw new Data.DataException(string.Format("docid = {0} < last docid ={1}", docidInFile, lastDocId));
                    }

                    lastDocId = docidInFile;

                    if (InsertProtect.InsertProtectInfo != null)
                    {
                        if (lastDocId == InsertProtect.InsertProtectInfo.LastDocId)
                        {
                            breakForProctect = true;
                        }

                        //Exit exception last time
                        if (lastDocId > InsertProtect.InsertProtectInfo.LastDocId)
                        {
                            lastDocId = InsertProtect.InsertProtectInfo.LastDocId;
                            truncateLength = fs.Position - sizeof(int);
                            break;
                        }
                    }

                    if (payloadLength == 0)
                    {
                        buf = new byte[sizeof(int)];

                        fs.Read(buf, 0, buf.Length);

                        _DocumentsCount = (int)BitConverter.ToInt32(buf, 0);

                        return docPayload;
                    }

                    byte[] byteData = new byte[payloadLength * 4];
                    fs.Read(byteData, 0, byteData.Length);

                    Data.Payload payload = new Hubble.Core.Data.Payload(payloadLength);

                    payload.CopyFrom(byteData);

                    payload.FileIndex = fileIndex;

                    if (payloadLength > 0)
                    {
                        docPayload.Add(lastDocId, payload);
                        _DocumentsCount++;
                    }

                    if (breakForProctect)
                    {
                        truncateLength = fs.Position;
                        break;
                    }
                }

                GC.Collect();

            }

            if (InsertProtect.InsertProtectInfo != null)
            {
                //Exit exception last time

                if (InsertProtect.InsertProtectInfo.LastDocId < 0)
                {
                    //No Data
                    lastDocId = -1;

                    using (System.IO.FileStream fs = new System.IO.FileStream(_FileName, System.IO.FileMode.Open, System.IO.FileAccess.Write))
                    {
                        fs.SetLength(HeadLength);
                    }
                }
                else
                {
                    //Have data
                    if (payloadLength == 0)
                    {
                        using (System.IO.FileStream fs = new System.IO.FileStream(_FileName, System.IO.FileMode.Open, System.IO.FileAccess.Write))
                        {
                            //Seek to end of head
                            fs.Seek(HeadLength, System.IO.SeekOrigin.Begin);

                            //Write last doc id
                            byte[] buf = BitConverter.GetBytes(InsertProtect.InsertProtectInfo.LastDocId);
                            fs.Write(buf, 0, buf.Length);

                            //Write documents count
                            buf = BitConverter.GetBytes(InsertProtect.InsertProtectInfo.DocumentsCount);
                            fs.Write(buf, 0, buf.Length);
                        }

                    }
                    else
                    {
                        using (System.IO.FileStream fs = new System.IO.FileStream(_FileName, System.IO.FileMode.Open, System.IO.FileAccess.Write))
                        {
                            if (truncateLength > 0 && truncateLength < fs.Length)
                            {
                                fs.SetLength(truncateLength);
                            }
                        }

                    }
                }
            }

            return docPayload;

        }

        internal void Create(List<Data.Field> fields)
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

        internal void Add(int docId, Hubble.Core.Data.Payload payLoad, PayloadProvider payloadProvider)
        {
            ASendMessage((int)Event.Add, new PayloadEntity(docId, payLoad, payloadProvider));
        }

        internal void Collect()
        {
            SSendMessage((int)Event.Collect, null, 300 * 1000);
        }

        internal void Update(IList<int> docIds, IList<Data.Payload> payloads, PayloadProvider payloadProvider)
        {
            List<PayloadEntity> peList = new List<PayloadEntity>();

            for (int i = 0; i < docIds.Count; i++)
            {
                peList.Add(new PayloadEntity(docIds[i], payloads[i], payloadProvider));
            }

            SSendMessage((int)Event.Update, peList, int.MaxValue);
        }
    }
}
