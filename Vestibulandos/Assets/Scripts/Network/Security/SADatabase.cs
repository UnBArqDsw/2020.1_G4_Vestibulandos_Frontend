using System.Collections.Generic;
using Utils.Random;

namespace Network.Security
{
    public class SADatabase
    {
        protected object m_lock = new object();
        protected Dictionary<ushort, SecurityAssociation> _mapSaList;

        public SADatabase()
        {
            lock (m_lock)
            {
                _mapSaList = new Dictionary<ushort, SecurityAssociation>
                {
                    // Insert default Security Association with SPIIndex 0.
                    { 0, new SecurityAssociation() }
                };
            }
        }

        public void Clear()
        {
            lock (m_lock)
            {
                _mapSaList.Clear();
            }
        }

        public void Insert(out ushort spiIndex, SecurityAssociation sa)
        {
            MT19937 mt = new MT19937();

            lock (m_lock)
            {
                for (; ; )
                {
                    // Generate a random number
                    spiIndex = (ushort)mt.RandomRange(1, ushort.MaxValue); // 1 or more. SPI 0 is already set in the constructor.

                    // Search for the number in the database. If it's not there, use it.
                    if (!Find(spiIndex))
                        break;
                }

                _mapSaList.Add(spiIndex, sa);
            }
        }

        public bool Insert(ushort spiIndex, SecurityAssociation sa)
        {
            lock (m_lock)
            {
                // Search for the number in the database. If it's not there, use it.
                if (!Find(spiIndex))
                    return false;

                _mapSaList.Add(spiIndex, sa);
            }

            return true;
        }

        public void Delete(ushort spiIndex)
        {
            lock (m_lock)
            {
                _mapSaList.Remove(spiIndex);
            }
        }

        public bool Find(ushort spiIndex)
        {
            lock (m_lock)
            {
                return _mapSaList.ContainsKey(spiIndex);
            }
        }

        public SecurityAssociation GetSA(ushort spiIndex)
        {
            lock (m_lock)
            {
                // If there is no SA to find, it returns a constant key set to SPI 0.
                return _mapSaList.TryGetValue(spiIndex, out SecurityAssociation sa) ? sa : _mapSaList[0];
            }
        }

        public SecurityAssociation CreateNewSA(out ushort spiIndex)
        {
            // Create new security association.
            SecurityAssociation sa = new SecurityAssociation();

            // Define new keys.
            sa.ResetRandomizeKey();

            // Insert new security association and generate new key.
            Insert(out spiIndex, sa);

            // Return security created.
            return GetSA(spiIndex);
        }

        public SecurityAssociation CreateNewSA(ushort spiIndex)
        {
            // Create new security association.
            SecurityAssociation sa = new SecurityAssociation();

            // Insert new security association and generate new key.
            Insert(spiIndex, sa);

            // Return security created.
            return GetSA(spiIndex);
        }
    }
}
