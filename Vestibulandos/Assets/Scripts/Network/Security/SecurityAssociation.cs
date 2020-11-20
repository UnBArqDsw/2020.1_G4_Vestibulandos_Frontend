using System;
using System.Linq;
using System.Security.Cryptography;
using Utils.Random;

namespace Network.Security
{
    public class SecurityAssociation
    {
        // All size unit of data is byte. ( not bit )
        public const int IV_SIZE = 8;
        public const int ICV_SIZE = 10;
        public const int BLOCK_SIZE = 8;

        /// <summary>
        /// Packet Header length.
        /// </summary>
        public const int PACKET_LENGTH_HEADER = sizeof(ushort);

        /// <summary>
        /// Packet Compress Header length.
        /// </summary>
        public const int PACKET_COMPRESS_HEADER = sizeof(bool);

        /// <summary>
        /// Packet SPI Header length.
        /// </summary>
        public const int PACKET_SPI_HEADER = sizeof(ushort);

        /// <summary>
        /// Packet Sequence Number Header length.
        /// </summary>
        public const int PACKET_SEQUENCE_NUMBER_HEADER = sizeof(uint);

        /* 
         * 1 block should be padded unconditionally. If the data length is correct in block units, 
         * fill one complete block, otherwise fill one incomplete block to fill the missing. 
         * The value specified below specifies the number of extra padding blocks to hide the data size thereafter. 
         * If the value is 0, no additional padding is done. 
         */

        public const uint MAX_EXTRA_PAD_BLOCKS = 1;

        public const int AUTH_KEY_SIZE = 8;
        public const int CRYPTO_KEY_SIZE = 8;

        // 0xC0 0xD3 0xBD 0xC3 0xB7 0xCE 0xB8 0xB8 = 임시로만드는키
        public readonly byte[] DEFAULT_AUTH_KEY = { 0xC0, 0xD3, 0xBD, 0xC3, 0xB7, 0xCE, 0xB8, 0xB8 };

        // 0xC7 0xD8 0xC4 0xBF 0xB5 0xE9 0xC0 0xFD = 해커들절대모를키
        public readonly byte[] DEFAULT_CRYPTO_KEY = { 0xC7, 0xD8, 0xC4, 0xBF, 0xB5, 0xE9, 0xC0, 0xFD };

        protected ByteStream m_bsAuthKey;
        protected ByteStream m_bsCryptoKey;

        protected uint m_nSequenceNum;
        protected uint m_nLastSequenceNum;
        public uint LastSequenceNum => m_nLastSequenceNum;

        protected uint m_nReplayWindowMask;
        public uint ReplayWindowMask => m_nReplayWindowMask;

        // Constructor's
        public SecurityAssociation()
        {
            m_nSequenceNum = 1;
            m_nLastSequenceNum = 0;
            m_nReplayWindowMask = 0;

            m_bsAuthKey = new ByteStream(DEFAULT_AUTH_KEY);
            m_bsCryptoKey = new ByteStream(DEFAULT_CRYPTO_KEY);
        }

        public void Clear()
        {
            m_nSequenceNum = 1;
            m_nLastSequenceNum = 0;
            m_nReplayWindowMask = 0;

            if (m_bsAuthKey != null)
            {
                m_bsAuthKey.Buffer = DEFAULT_AUTH_KEY;
            }

            if (m_bsCryptoKey != null)
            {
                m_bsCryptoKey.Buffer = DEFAULT_CRYPTO_KEY;
            }
        }

        public void ResetRandomizeKey()
        {
            MT19937 mt = new MT19937();

            for (var i = 0; i < AUTH_KEY_SIZE; i++)
            {
                // 1 or more. SPI 0 is already set in the constructor.
                m_bsAuthKey.Buffer[i] = Convert.ToByte(mt.RandomRange(1, byte.MaxValue));
            }

            for (var i = 0; i < CRYPTO_KEY_SIZE; i++)
            {
                // 1 or more. SPI 0 is already set in the constructor.
                m_bsCryptoKey.Buffer[i] = Convert.ToByte(mt.RandomRange(1, byte.MaxValue));
            }
        }

        public void SetAuthKey(byte[] bsKey)
        {
            m_bsAuthKey.Buffer = bsKey;
        }

        public void SetCryptoKey(byte[] bsKey)
        {
            m_bsCryptoKey.Buffer = bsKey;
        }

        public ByteStream GetAuthKey()
        {
            return m_bsAuthKey;
        }

        public ByteStream GetCryptoKey()
        {
            return m_bsCryptoKey;
        }

        public uint GetSequenceNum()
        {
            return m_nSequenceNum;
        }

        public void IncrSequenceNum()
        {
            ++m_nSequenceNum;
        }

        public bool IsValidSequenceNum(uint nSequenceNum_)
        {
            // The following algorithm is based on the Sequence Space Window
            // Code Example presented in RFC 2401.
            //
            // The "right" edge of the window represents the highest validated
            // Sequence Number received. Packets that contain Sequence Numbers lower
            // than the "left" edge of the window are rejected. Packets falling
            // within the window are checked against a list of received packets
            // within the window. Duplicates are rejected. If the received packet 
            // falls within the window or is new, or if the packet is to the right 
            // of the window, the Sequence Number is valid and the packet moves on
            // to the next verification stage.

            // Check for sequence number wrap
            if (nSequenceNum_ == 0)
                return false;

            // Nominal case: the new number is larger than the last packet
            if (nSequenceNum_ > m_nLastSequenceNum)
                return true;

            const byte CHAR_BIT = 8;
            const ulong nReplayWindowSize = PACKET_SEQUENCE_NUMBER_HEADER * CHAR_BIT;
            uint nDiff = m_nLastSequenceNum - nSequenceNum_;

            // Packet is too old or wrapped
            if (nDiff >= nReplayWindowSize)
                return false;

            // Packet is a duplicate
            uint nBit = 1;
            if ((m_nReplayWindowMask & (nBit << (int)nDiff)) > 0)
                return false;

            // Out of order, but within window
            return true;
        }

        /// Update the replay window based on the given (validated) sequence number
        public void UpdateReplayWindow(uint nSequenceNum_)
        {
            // The following algorithm is based on the Sequence Space Window
            // Code Example presented in RFC 2401.
            //
            // The "right" edge of the window represents the highest validated
            // Sequence Number received. Packets that contain Sequence Numbers lower
            // than the "left" edge of the window are rejected. Packets falling
            // within the window are checked against a list of received packets
            // within the window. Duplicates are rejected. If the received packet 
            // falls within the window or is new, or if the packet is to the right 
            // of the window, the Sequence Number is valid and the packet moves on
            // to the next verification stage.

            if (!IsValidSequenceNum(nSequenceNum_)) return;

            const byte CHAR_BIT = 8;
            ulong nReplayWindowSize = (ulong)PACKET_SEQUENCE_NUMBER_HEADER * CHAR_BIT;

            // Nominal case: the new number is larger than the last packet
            if (nSequenceNum_ > m_nLastSequenceNum)
            {
                uint nDiff = nSequenceNum_ - m_nLastSequenceNum;

                // If the packet is within the window, slide the window
                if (nDiff < nReplayWindowSize)
                {
                    m_nReplayWindowMask <<= (int)nDiff;
                    m_nReplayWindowMask |= 1;
                }
                else
                {
                    // packet way outside the window; reset the window
                    m_nReplayWindowMask = 1;
                }

                // Update the "last" sequence number
                m_nLastSequenceNum = nSequenceNum_;
            }
            else
            {
                // New number is smaller than the last packet
                uint nDiff = m_nLastSequenceNum - nSequenceNum_;

                // Mark the packet as seen
                uint nBit = 1;
                m_nReplayWindowMask |= (nBit << (int)nDiff);
            }
        }

        /// <summary>
        /// Gets the data to be encrypted and returns the encrypted data
        /// </summary>
        /// <param name="payload">Packet data to be encrypted</param>
        /// <param name="iv">Initialization vector</param>
        public byte[] Encryption(byte[] payload, byte[] iv)
        {
            using (DESCryptoServiceProvider desProvider = new DESCryptoServiceProvider())
            {
                desProvider.Mode = CipherMode.CBC;
                desProvider.Padding = PaddingMode.None;

                using (ICryptoTransform encryptor = desProvider.CreateEncryptor(m_bsCryptoKey.Buffer, iv))
                {
                    return encryptor.TransformFinalBlock(payload, 0, payload.Length);
                }
            }
        }

        /// <summary>
        /// Gets the received packet data and returns the decrypted data
        /// </summary>
        /// <param name="crypt">The packet data the way it was received</param>
        /// <param name="IV">Initialization vector</param>
        public byte[] Decryption(byte[] crypt, byte[] iv)
        {
            using (DESCryptoServiceProvider desProvider = new DESCryptoServiceProvider())
            {
                desProvider.Mode = CipherMode.CBC;
                desProvider.Padding = PaddingMode.None;

                using (ICryptoTransform decryptor = desProvider.CreateDecryptor(m_bsCryptoKey.Buffer, iv))
                {
                    return decryptor.TransformFinalBlock(crypt, 0, crypt.Length);
                }
            }
        }

        /// <summary>
        /// Generates an HMAC hash to the encrypted packet data
        /// </summary>
        /// <param name="auth"></param>
        public byte[] GenerateICV(byte[] auth, int size)
        {
            using (HMACMD5 hmac = new HMACMD5(m_bsAuthKey.Buffer))
            {
                byte[] hash = hmac.ComputeHash(auth, 0, size);
                return hash.Take(ICV_SIZE).ToArray();
            }
        }
    }
}
