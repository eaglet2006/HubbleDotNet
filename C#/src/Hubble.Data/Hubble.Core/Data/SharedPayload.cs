using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Hubble.Core.Data
{
    public partial class DBProvider
    {
        public class SharedPayload
        {
            PayloadProvider _DocPayload;
            internal SharedPayload(PayloadProvider docPayload)
            {
                _DocPayload = docPayload;
            }

            public void EnterPayloladShareLock()
            {
                _DocPayload.EnterShareLock();
            }

            public void LeavePayloadShareLock()
            {
                _DocPayload.LeaveShareLock();
            }

            public int GetPayloadRank(int docid)
            {
                UInt16 uRank;
                if (_DocPayload.InnerTryGetRank(docid, out uRank))
                {
                    return uRank;
                }
                else
                {
                    return 0;
                }
            }
        }
    }
}
