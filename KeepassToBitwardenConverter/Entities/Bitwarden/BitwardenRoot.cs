using System.Collections.Generic;

namespace KeepassToBitwardenConverter.Entities.Bitwarden
{
    public class BitwardenRoot
    {
        public List<Folder> Folders = new List<Folder>();
        public List<Item> Items = new List<Item>();
    }
}