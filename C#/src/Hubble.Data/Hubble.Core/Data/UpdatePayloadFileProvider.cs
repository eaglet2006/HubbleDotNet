using System;
using System.Collections.Generic;
using System.Text;

namespace Hubble.Core.Data
{
    class UpdatePayloadFileProvider
    {
        internal List<int> UpdateIds = new List<int>();
        internal List<Payload> UpdatePayloads = new List<Payload>();
        Dictionary<int, int> _DocidIndex = new Dictionary<int, int>();

        internal void Sort()
        {
            UpdateIds.Sort();

            List<Payload> updatePayloads = new List<Payload>();

            foreach (int docid in UpdateIds)
            {
                updatePayloads.Add(UpdatePayloads[_DocidIndex[docid]]);
            }

            UpdatePayloads = updatePayloads; 
        }

        internal void Add(List<int> updateIds, List<Payload> updatePayloads)
        {
            for(int i = 0; i < updateIds.Count; i++)
            {
                int docid = updateIds[i];

                Payload payload = updatePayloads[i];

                int index;
                if (_DocidIndex.TryGetValue(docid, out index))
                {
                    UpdatePayloads[index] = payload;
                }
                else
                {
                    UpdateIds.Add(docid);
                    UpdatePayloads.Add(payload);
                    _DocidIndex.Add(docid, UpdateIds.Count - 1);
                }
            }
        }
    }
}
