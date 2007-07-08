/****************************************************************************
While the underlying libraries are covered by LGPL, this sample is released 
as public domain.  It is distributed in the hope that it will be useful, but 
WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY 
or FITNESS FOR A PARTICULAR PURPOSE.  
*****************************************************************************/

using System;
using System.Diagnostics;
using System.Runtime.InteropServices;

using MediaFoundation;
using MediaFoundation.Misc;
using MediaFoundation.Utils;

namespace WavSourceFilter
{
    [StructLayout(LayoutKind.Sequential, Pack=2)]
    public class RIFFCHUNK
    {
        public FourCC fcc;
        public int cb;
    }

    [StructLayout(LayoutKind.Sequential, Pack=2)]
    public class RIFFLIST
    {
        public FourCC fcc;
        public int cb;
        public FourCC fccListType;
    }

    [StructLayout(LayoutKind.Sequential, Pack=2)]
    public class CRiffChunk : RIFFCHUNK
    {
        public CRiffChunk()
        {
            fcc = new FourCC(0);
            cb = 0;
        }

        public CRiffChunk(RIFFCHUNK c)
        {
            fcc = c.fcc;
            cb = c.cb;
        }
        public FourCC FourCC() { return fcc; }
        public int DataSize() { return cb; }
        public bool IsList() { return fcc == new FourCC("LIST"); }
    }

    public class CRiffParser : COMBase, IDisposable
    {
        #region Member Variables

        IMFByteStream m_pStream;

        FourCC m_fccID;		// FOURCC of the current container ('RIFF' or 'LIST').
        FourCC m_fccType;		// FOURCC of the RIFF file type or LIST type.

        long m_llContainerOffset;    // Start of the container, as an offset into the stream.
        int m_dwContainerSize;		// Size of the container including the RIFF header.
        long m_llCurrentChunkOffset;	// Start of the current RIFF chunk, as an offset into the stream.

        CRiffChunk m_chunk;		        // Current RIFF chunk.

        int m_dwBytesRemaining;     // How many bytes are left in this chunk?

        #endregion

        /// <summary>
        /// CRiffParser constructor
        /// </summary>
        /// <param name="pStream">Stream to read from RIFF file</param>
        /// <param name="id">FOURCC of the RIFF container. Should be 'RIFF' or 'LIST'.</param>
        /// <param name="cbStartOfContainer">Start of the container, as an offset into the stream.</param>
        public CRiffParser(IMFByteStream pStream, FourCC id, long cbStartOfContainer)
        {
            m_chunk = new CRiffChunk();
            m_fccID = id;
            m_llContainerOffset = cbStartOfContainer;

            if (pStream == null)
            {
                throw new COMException("invalid IMFByteStream", E_Pointer);
            }
            else
            {
                m_pStream = pStream;

                ReadRiffHeader();
            }
        }

        ~CRiffParser()
        {
            if (m_pStream != null)
            {
                try
                {
                    Marshal.ReleaseComObject(m_pStream);
                }
                catch { }
                m_pStream = null;
            }
        }

        #region Public Methods

        public FourCC RiffID() { return m_fccID; }
        public FourCC RiffType() { return m_fccType; }

        public CRiffChunk Chunk() { return m_chunk; }
        public int BytesRemainingInChunk() { return m_dwBytesRemaining; }

        //-------------------------------------------------------------------
        // Name: MoveToNextChunk
        // Description:
        // Advance to the start of the next chunk and read the chunk header.
        //-------------------------------------------------------------------

        public void MoveToNextChunk()
        {
            // chunk offset is always bigger than container offset,
            // and both are always non-negative.
            Debug.Assert(m_llCurrentChunkOffset > m_llContainerOffset);
            Debug.Assert(m_llCurrentChunkOffset >= 0);
            Debug.Assert(m_llContainerOffset >= 0);

            // Update current chunk offset to the start of the next chunk
            m_llCurrentChunkOffset = m_llCurrentChunkOffset + ChunkActualSize();

            // Are we at the end?
            if ((m_llCurrentChunkOffset - m_llContainerOffset) >= m_dwContainerSize)
            {
                throw new COMException("At end of chunk", E_Fail);
            }

            // Current chunk offset + size of current chunk
            if (long.MaxValue - m_llCurrentChunkOffset <= ChunkActualSize())
            {
                throw new COMException("Chunk size error", E_InvalidArgument);
            }

            // Seek to the start of the chunk.
            m_pStream.SetCurrentPosition(m_llCurrentChunkOffset);

            // Read the header.
            ReadChunkHeader();

            // This chunk cannot be any larger than (container size - (chunk offset - container offset) )
            long maxChunkSize = (long)m_dwContainerSize - (m_llCurrentChunkOffset - m_llContainerOffset);

            if (maxChunkSize < ChunkActualSize())
            {
                throw new COMException("Bad chunk size", E_InvalidArgument);
            }

            m_dwBytesRemaining = m_chunk.DataSize();
        }

        //-------------------------------------------------------------------
        // Name: MoveToChunkOffset
        // Description:
        // Move the file pointer to a byte offset from the start of the
        // current chunk.
        //-------------------------------------------------------------------

        public void MoveToChunkOffset(int dwOffset)
        {
            if (dwOffset > m_chunk.DataSize())
            {
                throw new COMException("End of chunk", E_InvalidArgument);
            }

            m_pStream.SetCurrentPosition(m_llCurrentChunkOffset + dwOffset + Marshal.SizeOf(typeof(RIFFCHUNK)));
            m_dwBytesRemaining = m_chunk.DataSize() - dwOffset;
        }

        //-------------------------------------------------------------------
        // Name: ReadDataFromChunk
        // Description:
        // Read data from the current chunk. (Starts at the current file ptr.)
        //-------------------------------------------------------------------

        public void ReadDataFromChunk(IntPtr pData, int dwLengthInBytes)
        {
            if (dwLengthInBytes > m_dwBytesRemaining)
            {
                throw new COMException("End of chunk", E_InvalidArgument);
            }

            int cbRead = 0;
            m_pStream.Read(pData, dwLengthInBytes, out cbRead);

            m_dwBytesRemaining -= cbRead;
        }

        #endregion

        #region Private methods

        private long ChunkActualSize() { return Marshal.SizeOf(typeof(RIFFCHUNK)) + RIFFROUND(m_chunk.cb); }

        static private int RIFFROUND(int cb)
        {
            return ((cb) + ((cb) & 1));
        }

        //-------------------------------------------------------------------
        // Name: EnumerateChunksInList
        // Description: Return a parser for a LIST.
        //-------------------------------------------------------------------

        private void EnumerateChunksInList(out CRiffParser ppParser)
        {
            ppParser = null;

            if (!m_chunk.IsList())
            {
                throw new COMException("not in list", E_Fail);
            }

            ppParser = new CRiffParser(m_pStream, new FourCC("LIST"), m_llCurrentChunkOffset);
        }

        //-------------------------------------------------------------------
        // Name: MoveToChunkOffset
        // Description:
        // Move the file pointer to the start of the current chunk.
        //-------------------------------------------------------------------

        private void MoveToStartOfChunk()
        {
            MoveToChunkOffset(0);
        }

        //-------------------------------------------------------------------
        // Name: ReadRiffHeader
        // Description:
        // Read the container header section. (The 'RIFF' or 'LIST' header.)
        //
        // This method verifies the header is well-formed and caches the
        // container's FOURCC type.
        //-------------------------------------------------------------------

        private void ReadRiffHeader()
        {
            int iRiffSize = Marshal.SizeOf(typeof(RIFFLIST));

            // Riff chunks must be WORD aligned
            if (!Utils.IsAligned(m_llContainerOffset, 2))
            {
                throw new COMException("bad alignment", E_InvalidArgument);
            }

            // Offset must be positive.
            if (m_llContainerOffset < 0)
            {
                throw new COMException("negative offset", E_InvalidArgument);
            }

            // Offset + the size of header must not overflow.
            if (long.MaxValue - m_llContainerOffset <= iRiffSize)
            {
                throw new COMException("overflow chunk", E_InvalidArgument);
            }

            RIFFLIST header = new RIFFLIST();
            int cbRead = 0;

            // Seek to the start of the container.
            m_pStream.SetCurrentPosition(m_llContainerOffset);

            // Read the header.
            IntPtr ip = Marshal.AllocCoTaskMem(iRiffSize);
            try
            {
                m_pStream.Read(ip, iRiffSize, out cbRead);

                // Make sure we read the number of bytes we expected.
                if (cbRead == iRiffSize)
                {
                    Marshal.PtrToStructure(ip, header);
                }
                else
                {
                    throw new COMException("read riff failure", E_InvalidArgument);
                }
            }
            finally
            {
                Marshal.FreeCoTaskMem(ip);
            }

            // Make sure the header ID matches what the caller expected.
            if (header.fcc != m_fccID)
            {
                throw new COMException("bad header id", E_InvalidArgument);
            }

            // The size given in the RIFF header does not include the 8-byte header.
            // However, our m_llContainerOffset is the offset from the start of the
            // header. Therefore our container size = listed size + size of header.

            m_dwContainerSize = header.cb + Marshal.SizeOf(typeof(RIFFCHUNK));
            m_fccType = header.fccListType;

            // Start of the first chunk = start of container + size of container header
            m_llCurrentChunkOffset = m_llContainerOffset + iRiffSize;

            ReadChunkHeader();
        }

        //-------------------------------------------------------------------
        // Name: ReadChunkHeader
        // Description:
        // Reads the chunk header. Caller must ensure that the current file
        // pointer is located at the start of the chunk header.
        //-------------------------------------------------------------------

        private void ReadChunkHeader()
        {
            int iRiffChunkSize = Marshal.SizeOf(typeof(RIFFCHUNK));

            // Offset + the size of header must not overflow.
            if (long.MaxValue - m_llCurrentChunkOffset <= iRiffChunkSize)
            {
                throw new COMException("overflow chunk", E_InvalidArgument);
            }

            int cbRead;
            IntPtr ip = Marshal.AllocCoTaskMem(iRiffChunkSize);

            try
            {
                m_pStream.Read(ip, iRiffChunkSize, out cbRead);
                // Make sure we got the number of bytes we expected.
                if (cbRead == iRiffChunkSize)
                {
                    Marshal.PtrToStructure(ip, m_chunk);
                }
                else
                {
                    throw new COMException("read failure on chunk header", E_InvalidArgument);
                }
            }
            finally
            {
                Marshal.FreeCoTaskMem(ip);
            }

            m_dwBytesRemaining = m_chunk.DataSize();
        }

        #endregion

        #region IDisposable Members

        public void Dispose()
        {
            if (m_pStream != null)
            {
                Marshal.ReleaseComObject(m_pStream);
                m_pStream = null;
            }
            GC.SuppressFinalize(this);
        }

        #endregion
    }
}
