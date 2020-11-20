using System;
using System.IO;

namespace Network.Transport.Tcp
{
    public class StreamBuffer
    {
        /// <summary>
        /// Array of the Buffer.
        /// </summary>
        private byte[] m_arrBuffer;

        /// <summary>
        /// Position of the Buffer.
        /// </summary>
        private int m_nPosition;

        /// <summary>
        /// Length of the Buffer.
        /// </summary>
        private int m_nLength;

        //---------------------------------------------------------------------------------------------------
        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="nSize"></param>
        public StreamBuffer(int nSize = 0)
        {
            this.m_arrBuffer = new byte[nSize];
        }

        //---------------------------------------------------------------------------------------------------
        public StreamBuffer(byte[] arrBuffer)
        {
            m_arrBuffer = arrBuffer;
            m_nLength = arrBuffer.Length;
        }

        //---------------------------------------------------------------------------------------------------
        public byte[] ToArray()
        {
            byte[] arrBuffer = new byte[m_nLength];
            Buffer.BlockCopy(m_arrBuffer, 0, arrBuffer, 0, m_nLength);
            return arrBuffer;
        }

        //---------------------------------------------------------------------------------------------------
        public byte[] ToArrayFromPos()
        {
            int nOffset = m_nLength - m_nPosition;

            if (nOffset <= 0)
            {
                return new byte[0];
            }

            byte[] arrBuffer = new byte[nOffset];
            Buffer.BlockCopy(m_arrBuffer, m_nPosition, arrBuffer, 0, nOffset);
            return arrBuffer;
        }

        //---------------------------------------------------------------------------------------------------
        public void Compact()
        {
            long nOffset = Length - Position;

            if (nOffset > 0)
            {
                Buffer.BlockCopy(m_arrBuffer, Position, m_arrBuffer, 0, (int)nOffset);
            }

            Position = 0;
            SetLength(nOffset);
        }

        //---------------------------------------------------------------------------------------------------
        public byte[] GetBuffer()
        {
            return m_arrBuffer;
        }

        //---------------------------------------------------------------------------------------------------
        public byte[] GetBufferAndAdvance(int nLength, out int nOffset)
        {
            nOffset = Position;

            Position += nLength;

            return m_arrBuffer;
        }

        //---------------------------------------------------------------------------------------------------
        public int Length => m_nLength;

        //---------------------------------------------------------------------------------------------------
        public int Position
        {
            get => m_nPosition;
            set
            {
                m_nPosition = value;

                if (m_nLength < m_nPosition)
                {
                    m_nLength = m_nPosition;
                    CheckSize(m_nLength);
                }
            }
        }

        //---------------------------------------------------------------------------------------------------
        public void Flush()
        {
        }

        //---------------------------------------------------------------------------------------------------
        public long Seek(long nOffset, SeekOrigin seek)
        {
            int nPosition = 0;

            switch (seek)
            {
                case SeekOrigin.Begin:
                    nPosition = (int)nOffset;
                    break;
                case SeekOrigin.Current:
                    nPosition = m_nPosition + (int)nOffset;
                    break;
                case SeekOrigin.End:
                    nPosition = m_nLength + (int)nOffset;
                    break;
                default:
                    throw new ArgumentException("Invalid seek origin");
            }

            if (nPosition < 0)
            {
                throw new ArgumentException("Seek before begin");
            }

            if (nPosition > m_nLength)
            {
                throw new ArgumentException("Seek after end");
            }

            m_nPosition = nPosition;
            return m_nPosition;
        }

        //---------------------------------------------------------------------------------------------------
        public void SetLength(long nValue)
        {
            m_nLength = (int)nValue;

            CheckSize(m_nLength);

            if (m_nPosition > m_nLength)
            {
                m_nPosition = m_nLength;
            }
        }

        //---------------------------------------------------------------------------------------------------
        public void SetCapacityMinimum(int nNeededSize)
        {
            this.CheckSize(nNeededSize);
        }

        //---------------------------------------------------------------------------------------------------
        public int Read(byte[] arrBuffer, int nOffset, int nCount)
        {
            int nDiff = m_nLength - m_nPosition;
            if (nDiff <= 0)
                return 0;

            if (nCount > nDiff)
                nCount = nDiff;

            Buffer.BlockCopy(m_arrBuffer, m_nPosition, arrBuffer, nOffset, nCount);
            m_nPosition += nCount;

            return nCount;
        }

        //---------------------------------------------------------------------------------------------------
        public void Write(byte[] arrbuffer, int nSrcOffset, int nCount)
        {
            int nOffset = m_nPosition + nCount;
            CheckSize(nOffset);

            if (nOffset > m_nLength)
            {
                m_nLength = nOffset;
            }

            Buffer.BlockCopy(arrbuffer, nSrcOffset, m_arrBuffer, m_nPosition, nCount);
            m_nPosition = nOffset;
        }

        //---------------------------------------------------------------------------------------------------
        public byte ReadByte()
        {
            if (m_nPosition >= m_nLength)
            {
                throw new EndOfStreamException(string.Concat("SteamBuffer.ReadByte() failed. pos:", m_nPosition, "len:", m_nLength));
            }

            return m_arrBuffer[m_nPosition + 1];
        }

        //---------------------------------------------------------------------------------------------------
        public void WriteByte(byte v0)
        {
            if (m_nPosition >= m_nLength)
            {
                m_nLength = m_nPosition + 1;
                CheckSize(m_nLength);
            }

            m_arrBuffer[m_nPosition + 1] = v0;
        }

        //---------------------------------------------------------------------------------------------------
        public void WriteBytes(byte v0, byte v1)
        {
            int nOffset = m_nPosition + 2;
            if (m_nLength < nOffset)
            {
                m_nLength = nOffset;
                CheckSize(m_nLength);
            }

            m_arrBuffer[m_nPosition + 1] = v0;
            m_arrBuffer[m_nPosition + 2] = v1;
        }

        //---------------------------------------------------------------------------------------------------
        private bool CheckSize(int nSize)
        {
            if (nSize <= m_arrBuffer.Length) return false;

            int nBufferLen = m_arrBuffer.Length;
            if (nBufferLen == 0)
                nBufferLen = 1;

            while (nSize > nBufferLen)
            {
                nBufferLen *= 2;
            }

            byte[] arrDst = new byte[nBufferLen];
            Buffer.BlockCopy(m_arrBuffer, 0, arrDst, 0, m_arrBuffer.Length);
            m_arrBuffer = arrDst;

            return true;
        }
    }
}