using System.Collections.Generic;

namespace KeepassToBitwardenConverter.Entities.Bitwarden
{
    public class Item
    {
        public string Id;
        public string OrganizationId;
        public string FolderId;
        public ItemType Type;
        public string Name;
        public string Notes;
        public bool Favorite;
        public List<ItemField> Fields;
        public ItemLogin Login;
        public string CollectionIds;
    }
}