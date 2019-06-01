using System.Collections.Generic;

namespace KeepassToBitwardenConverter.Entities.Bitwarden
{
    public class ItemLogin
    {
        public List<ItemLoginUri> Uris;
        public string Username;
        public string Password;
        public string Totp;
    }
}