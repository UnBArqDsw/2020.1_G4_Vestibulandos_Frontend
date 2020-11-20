using System;
using System.Linq;

namespace Network.Security
{
    public class SecureBuffer
    {
        private ByteStream m_bsBuf = null;
        private ushort m_nSPIndex;

        private SecurityAssociation m_SA = null;

        public byte[] GetData
        {
            get => m_bsBuf.Buffer;
            protected set => m_bsBuf.Buffer = value;
        }

        private int m_nSize;
        public int GetSize => m_nSize;

        // Constructor
        public SecureBuffer(ushort nSPI, SecurityAssociation sa)
        {
            m_nSPIndex = nSPI;
            m_nSize = 0;
            m_SA = sa;
        }

        // Constructor
        public SecureBuffer(SecureBuffer buf, SecurityAssociation sa)
        {
            m_nSPIndex = buf.m_nSPIndex;
            m_bsBuf = buf.m_bsBuf;
            m_nSize = 0;
            m_SA = sa;
        }

        // Constructor
        public SecureBuffer(ushort nSPI, byte[] pRecvData, ulong nBytes, SecurityAssociation sa)
        {
            m_bsBuf = new ByteStream(pRecvData);
            m_nSPIndex = nSPI;
            m_nSize = 0;
            m_SA = sa;
        }

        public bool Create(ByteStream payload)
        {
            m_bsBuf = new ByteStream(GetMaxSecureSize(payload));

            // Add Data: SPI
            m_bsBuf.Append(BitConverter.GetBytes(m_nSPIndex), m_nSize, SecurityAssociation.PACKET_SPI_HEADER);
            m_nSize += SecurityAssociation.PACKET_SPI_HEADER;

            // Add data: sequence number
            m_bsBuf.Append(BitConverter.GetBytes(GetSA().GetSequenceNum()), m_nSize, SecurityAssociation.PACKET_SEQUENCE_NUMBER_HEADER);
            m_nSize += SecurityAssociation.PACKET_SEQUENCE_NUMBER_HEADER;

            // IV is randomly generated every time.
            ByteStream iv = GenerateIV();
            m_bsBuf.Append(iv.Buffer, m_nSize, SecurityAssociation.IV_SIZE);
            m_nSize += SecurityAssociation.IV_SIZE;

            // Encrypt it with the generated IV. (encrypts to padding and padding size)
            ByteStream crypt = GenerateCrypt(payload, iv);
            if (crypt.Length <= 0)
                return false;

            // Add data: payload, padding, padding size
            m_bsBuf.Append(crypt.Buffer, m_nSize, crypt.Length);
            m_nSize += crypt.Length;

            // Generate ICV with input data so far
            ByteStream icv = GenerateICV(m_bsBuf, m_nSize);
            if (icv.Length <= 0)
                return false;

            // Add data: ICV
            m_bsBuf.Append(icv.Buffer, m_nSize, icv.Length);
            m_nSize += icv.Length;

            // Increase the sequence number of SecurityAssociation. (Sequence number of the next packet to send.)
            GetSA().IncrSequenceNum();

            // Created with success.
            return true;
        }

        /// <summary>
        /// Validate a secure KByteStream by examining the authentication data
        /// </summary>
        /// <returns>Return 'true' if is authenticated and 'false' if not.</returns>
        public bool IsAuthentic()
        {
            // Obtain the SPI from the data and verify that it is the SPI currently in the SADB.
            ushort nSPIndex = 0;
            if (!IsValidSPIndex(ref nSPIndex))
                return false;

            // Check packet size
            if (!IsValidSize())
                return false;

            // Validate the Integrity Check Value (ICV)
            if (!IsValidICV())
                return false;

            // Check sequence number
            uint nSequenceNum = 0;
            if (!IsValidSequenceNum(ref nSequenceNum))
                return false;

            // It's authentic.
            return true;
        }

        /// <summary>
        /// Extract the payload from a secure KByteStream. Returns false if the payload
        /// is invalid or cannot be retrieved. Assumes the KByteStream has already been
        /// authenticated.
        /// </summary>
        /// <param name="payload"></param>
        /// <returns></returns>
        public bool GetPayload(ref ByteStream payload)
        {
            // Check if packet is authentic.
            if (!IsAuthentic())
                return false;

            // Obtain IV from the data. IV is located after SPI and SeqNum.
            ulong nPos = SecurityAssociation.PACKET_SEQUENCE_NUMBER_HEADER + SecurityAssociation.PACKET_SPI_HEADER;
            ulong nSize = SecurityAssociation.IV_SIZE;
            if (nPos + nSize > (ulong)m_bsBuf.Length)
                return false;

            ByteStream iv = new ByteStream(m_bsBuf.SubStr((int)nPos, (int)nSize));

            // Fill in the remaining portion of the IV with zero bytes
            // Initialize the remaining digits after IV.
            // Obtained IV is already the path of IV_SIZE.

            // Extracts a portion to be decoded from the data. IV, and the ICV at the end is to be subtracted.
            nPos = SecurityAssociation.PACKET_SPI_HEADER + SecurityAssociation.PACKET_SEQUENCE_NUMBER_HEADER + nSize;
            if (nPos + SecurityAssociation.ICV_SIZE > (ulong)m_bsBuf.Length)
                return false;

            nSize = (ulong)m_bsBuf.Length - nPos - SecurityAssociation.ICV_SIZE;
            ByteStream crypt = new ByteStream(m_bsBuf.SubStr((int)nPos, (int)nSize));

            // Decryption.
            //payload.Clear();
            payload = new ByteStream(GetSA().Decryption(crypt.Buffer, iv.Buffer));
            if (payload.Empty)
                return false;

            // Obtain the padding length and verify that the value of padding is correct.
            ulong nPadBytes = 0;
            if (!IsValidPadding(payload, ref nPadBytes))
                return false;

            // Remove padding and padding size bytes.
            if ((ulong)payload.Length < nPadBytes + sizeof(byte))
                return false;

            payload.Resize(payload.Length - (long)nPadBytes - sizeof(byte));

            return true;
        }

        /// <summary>
        /// Marks a given KByteStream as authenticated and accepted. Automatically adjusts
        /// the replay window, so that IsAuthentic() will no longer validate the
        /// KByteStream correctly. Call this function only after calling IsAuthentic() and
        /// GetPayload(), but prior to calling IsAuthentic() on the next packet.
        /// </summary>
        void SetAccepted()
        {
            uint nSequenceNum = 0;
            bool bSuccess = IsValidSequenceNum(ref nSequenceNum);

            System.Diagnostics.Debug.Assert(bSuccess);

            // Update the replay window.
            GetSA().UpdateReplayWindow(nSequenceNum);
        }

        private ByteStream GenerateIV()
        {
            ByteStream bsIV_ = new ByteStream(SecurityAssociation.IV_SIZE);

            var random = new System.Random();
            byte ivByte = (byte)random.Next('A', 'h');

            for (int i = 0; i < SecurityAssociation.IV_SIZE; i++)
                bsIV_.Buffer[i] = ivByte;

            return bsIV_;
        }

        private ByteStream GenerateCrypt(ByteStream payload, ByteStream iv)
        {
            ByteStream crypt = new ByteStream(payload);

            // Append padding, if any
            ByteStream pad = new ByteStream(GeneratePadding(payload));
            crypt.Append(pad.Buffer);

            // Append pad length
            crypt.Append(BitConverter.GetBytes(pad.Length), sizeof(byte));

            return new ByteStream(GetSA().Encryption(crypt.Buffer, iv.Buffer));
        }

        /// <summary>
        /// Generate the Integrity Check Value of the given ByteStream.
        /// </summary>
        /// <param name="auth"></param>
        /// <param name="size"></param>
        /// <returns></returns>
        private ByteStream GenerateICV(ByteStream auth, int size)
        {
            return new ByteStream(GetSA().GenerateICV(auth.Buffer, size));
        }

        /// <summary>
        /// Generate the padding bytes based on the given payload
        /// </summary>
        /// <param name="payload"></param>
        /// <returns></returns>
        private byte[] GeneratePadding(ByteStream payload)
        {
            // The size of the payload, the padding and the pad length (1 byte)
            // must be evenly divisible by nBlockBytes
            const ulong nBlockBytes = SecurityAssociation.BLOCK_SIZE;

            // Once you get the number of bytes that should be padded by default.
            ulong nPadBytes = nBlockBytes - (((ulong)payload.Length + sizeof(byte)) % nBlockBytes);

            // Add some random padding to hide the true size of the payload
            byte nRand = 0;
            ulong nRandBlocks = 0;

            // If the maximum number of extra blocks is 5, the value of [0,5] must be mod to 6, which is 5 + 1.
            // If the extra padding size is 0, rand is% 1, so the value is always 0.
            nRandBlocks = nRand % (SecurityAssociation.MAX_EXTRA_PAD_BLOCKS + sizeof(byte));
            nPadBytes += nBlockBytes * nRandBlocks;

            // Create the padding buffer.
            byte[] pad = new byte[nPadBytes];

            // RFC 2406 says padding bytes are initialized with a series of 
            // one-byte integer values
            for (ulong i = 1; i <= nPadBytes; ++i)
                pad[i - 1] = (byte)i;

            return pad;
        }

        /// <summary>
        /// Determine the max size of the secure ByteStream based on the given payload.
        /// </summary>
        /// <param name="payload"></param>
        /// <returns></returns>
        private ulong GetMaxSecureSize(ByteStream payload)
        {
            ulong size = 0;

            size += SecurityAssociation.PACKET_SPI_HEADER;              // SPI.
            size += SecurityAssociation.PACKET_SEQUENCE_NUMBER_HEADER;  // Sequence.
            size += SecurityAssociation.IV_SIZE;                        // IV length.
            size += (ulong)payload.Length;                              // Payload length.
            size += GetMaxPadSize(payload);                             // Padding.
            size += sizeof(byte);                                       // Pad length.
            size += SecurityAssociation.ICV_SIZE;                       // ICV length.

            return size;
        }

        private ulong GetMaxPadSize(ByteStream payload)
        {
            // Obtain the block size.
            const ulong blockBytes = SecurityAssociation.BLOCK_SIZE;

            // Once you get the number of bytes that should be padded by default.
            ulong padBytes = blockBytes - (((ulong)payload.Length + sizeof(byte)) % blockBytes);

            // It adds extra padding can obtain additional blocks.
            padBytes += SecurityAssociation.MAX_EXTRA_PAD_BLOCKS * blockBytes;

            // Debug.
            System.Diagnostics.Debug.Assert(((ulong)payload.Length + sizeof(byte) + padBytes) % blockBytes == 0);
            System.Diagnostics.Debug.Assert(payload.Length < byte.MaxValue);

            return padBytes;
        }

        private bool IsValidSize()
        {
            // Assume no padding for quick check
            if (m_bsBuf.Length < SecurityAssociation.PACKET_SPI_HEADER +                // SPI
                                SecurityAssociation.PACKET_SEQUENCE_NUMBER_HEADER +     // sequence number
                                SecurityAssociation.IV_SIZE +                           // IV
                                sizeof(byte) +                                          // pad size
                                SecurityAssociation.ICV_SIZE)                           // ICV
                return false;

            // ByteStream meets minimum requires; other checks performed later
            return true;
        }

        /// <summary>
        /// Check if packet received has same SPI from current Client.
        /// </summary>
        /// <param name="nSPIndex"></param>
        /// <returns></returns>
        public bool IsValidSPIndex(ref ushort nSPIndex)
        {
            // Checks whether the SPI of the data stored in the buffer is valid.

            // The size of the data is smaller than the SPI size.
            if (m_bsBuf.Length < SecurityAssociation.PACKET_SPI_HEADER)
                return false;

            // Extract the SPI from the data. It is the first one.
            nSPIndex = BitConverter.ToUInt16(m_bsBuf.SubStr(0, SecurityAssociation.PACKET_SPI_HEADER), 0);

            // PS: The SPIndex registered between the server and the client can be different.

            return (m_nSPIndex == nSPIndex);
        }

        /// <summary>
        /// Check if packet received has valid sequence number.
        /// </summary>
        /// <param name="nSequenceNum"></param>
        /// <returns></returns>
        private bool IsValidSequenceNum(ref uint nSequenceNum)
        {
            // Read the Sequence Number from the data. It is right behind the SPI.
            nSequenceNum = BitConverter.ToUInt32(m_bsBuf.SubStr(SecurityAssociation.PACKET_SPI_HEADER, SecurityAssociation.PACKET_SEQUENCE_NUMBER_HEADER), 0);

            // Check if sequence is valid.
            return GetSA().IsValidSequenceNum(nSequenceNum);
        }

        /// <summary>
        /// Determines if the ICV is valid.
        /// </summary>
        /// <returns></returns>
        private bool IsValidICV()
        {
            // Read the ICV at the end of the data.
            ulong nSize = SecurityAssociation.ICV_SIZE;
            ulong nPos = (ulong)m_bsBuf.Length - nSize;
            ByteStream icv = new ByteStream(m_bsBuf.SubStr((int)nPos, (int)nSize));

            // Reads data except for the ICV value (data to be indexed in ICV calculation).
            nSize = nPos;
            ByteStream auth = new ByteStream(m_bsBuf.SubStr(0, (int)nSize));

            // Calculate the ICV.
            ByteStream icvCompare = GenerateICV(auth, auth.Length);

            // Compare the calculated icv and received icv values.
            if (!icv.Buffer.SequenceEqual(icvCompare.Buffer))
                return false;

            return true;
        }

        bool IsValidPadding(ByteStream payload, ref ulong nPadBytes)
        {
            // Receives decoded payload as input (actual data + padding + padding size).

            // At a minimum, padding size should be included.
            if (payload.Length == 0)
                return false;

            // Get padding size. It is located in the last byte.
            nPadBytes = payload.SubStr(payload.Length - sizeof(byte), sizeof(byte)).First();

            // Data must be at least as large as the padding size and larger than the size indicated by the size of the bytes.
            if (nPadBytes + sizeof(byte) > (ulong)payload.Length)
                return false;

            // Gets the padded part of the data.
            ulong nPos = ((ulong)payload.Length - nPadBytes - sizeof(byte));
            byte[] pad = payload.SubStr((int)nPos, (int)nPadBytes);

            // Verify that the padding bytes are correct. It must be a contiguous one-byte integer starting at 1.
            for (ulong i = 1; i <= nPadBytes; ++i)
            {
                if (pad[i - 1] != (byte)i)
                    return false;
            }

            // Padding is good.
            return true;
        }

        public SecurityAssociation GetSA()
        {
            return m_SA;
            //return Security.GetSADB().GetSA(m_nSPIndex);
        }
    }
}
