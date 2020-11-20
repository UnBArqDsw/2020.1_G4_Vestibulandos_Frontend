namespace Network.Security
{
    public static class Security
    {
        private static SADatabase _SADB;

        public static void InitSecurity()
        {
            // Init Security Assocciation Database.
            if (_SADB == null)
                _SADB = new SADatabase();
        }

        public static void ReleaseSecurity()
        {
            _SADB.Clear();
            _SADB = null;
        }

        public static SADatabase GetSADB()
        {
            return _SADB ?? (_SADB = new SADatabase());
        }
    }
}
