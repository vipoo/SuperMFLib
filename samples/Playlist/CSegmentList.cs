/****************************************************************************
While the underlying libraries are covered by LGPL, this sample is released
as public domain.  It is distributed in the hope that it will be useful, but
WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY
or FITNESS FOR A PARTICULAR PURPOSE.
*****************************************************************************/

using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using System.Runtime.InteropServices;

using MediaFoundation.Misc;

namespace Playlist
{
    class SegmentInfo
    {
        public SegmentInfo pNextSegment;
        public int SegmentID;
        public string SegmentName;
        public long hnsSegmentDuration;
        public long TopologyID;

        public SegmentInfo(int SegID, long hnsSegDuration, long TopoId, string szSegmentName)
        {
            SegmentID = SegID;
            hnsSegmentDuration = hnsSegDuration;
            TopologyID = TopoId;
            SegmentName = szSegmentName;

            pNextSegment = null;
        }
    }

    class CSegmentList : COMBase
    {
        #region Members

        private SortedList m_List;

        #endregion

        //Constructor
        public CSegmentList()
        {
            m_List = new SortedList();
        }

        public int GetCount()
        {
            return m_List.Count;
        }

        ////////////////////////////////////////////////////////////////////////////////////////
        //  Name: CSegmentList::AddNewSegmentEntry (Public)
        //  Description:
        //      Add a segment node.
        //  Parameters:
        //      SegId: [in] Segment identifier
        //      hnsSegDuration: [in] Segment Duration
        //      TopoId: [in] Topology identifier
        //      szSegmentName: [in] Segment Name
        /////////////////////////////////////////////////////////////////////////////////////////

        public void AddNewSegmentEntry(
                                int SegId,
                                long hnsSegDuration,
                                long TopoId,
                                string szSegmentName)
        {
            SegmentInfo pNewSegment = new SegmentInfo(SegId, hnsSegDuration, TopoId, szSegmentName);

            m_List.Add(SegId, pNewSegment);
        }

        ////////////////////////////////////////////////////////////////////////////////////////
        //  Name: CSegmentList::DeleteSegmentEntry (Public)
        //  Description:
        //      Delete a segment node based on segment identifier.
        //  Parameters:
        //      SegId: [in] Segment identifier
        /////////////////////////////////////////////////////////////////////////////////////////
        public void DeleteSegmentEntry(int SegId)
        {
            m_List.Remove(SegId);
        }

        ////////////////////////////////////////////////////////////////////////////////////////
        //  Name: CSegmentList::GetSegmentEntryInfo (Public)
        //  Description:
        //      Gets segment name and duration based on segment identifier.
        //  Parameters:
        //      SegID: [in] Segment identifier
        //      phnsSegDuration: [out] Segment Duration
        //      szSegURL: [out] Segment name
        //      dwSize: [in] size of szSegURL
        /////////////////////////////////////////////////////////////////////////////////////////

        public void GetSegmentEntryInfo(
                                int SegID,
                                out long phnsSegDuration,
                                out string szSegName)
        {
            int iIndex = m_List.IndexOfKey(SegID);
            SegmentInfo current = (SegmentInfo)m_List.GetByIndex(iIndex);

            phnsSegDuration = current.hnsSegmentDuration;
            szSegName = current.SegmentName;
        }

        ////////////////////////////////////////////////////////////////////////////////////////
        //  Name: CSegmentList::GetSegmentIDByTopoID (Public)
        //  Description:
        //      Gets the segment identifier based on the topology identifier.
        //  Parameter:
        //      TopologyID: [in] Topology identifier
        //      SegmentID: [out] Receives the segment identifier
        /////////////////////////////////////////////////////////////////////////////////////////
        public void GetSegmentIDByTopoID(
                                long TopologyID,
                                out int SegmentID)
        {
            int iCnt = m_List.Count;

            for (int x = 0; x < iCnt; x++)
            {
                SegmentInfo current = (SegmentInfo)m_List.GetByIndex(x);

                if (TopologyID == current.TopologyID)
                {
                    SegmentID = current.SegmentID;
                    return;
                }
            }

            //SegmentID not found
            throw new COMException("SegmentID not found", E_Fail);
        }

        ////////////////////////////////////////////////////////////////////////////////////////
        //  Name: CSegmentList::GetLastSegmentId (Public)
        //  Description:
        //      Gets the segment identifier for the last node.
        //  Parameter:
        //      pSegmentID: [out] Receives the segment identifier
        /////////////////////////////////////////////////////////////////////////////////////////
        public void GetLastSegmentId(out int pSegmentId)
        {
            pSegmentId = (int)m_List.GetKey(m_List.Count - 1);
        }
    }
}
