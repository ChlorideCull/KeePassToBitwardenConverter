using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Xml.Linq;
using KeepassToBitwardenConverter.Entities.Bitwarden;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;

namespace KeepassToBitwardenConverter
{
    class Program
    {
        // Fields with these names won't be carried over.
        private static string[] _blacklistedFields = {
            "KeePassXC-Browser Settings",
            "KeePassHttp Settings"
        };

        // Entries with these titles won't be carried over.
        private static string[] _blacklistedEntries =
        {
            "KeePassXC-Browser Settings",
            "KeePassHttp Settings"
        };
        
        // Folders with these names will be put in the root instead.
        private static string[] _blacklistedFolders =
        {
            "KeePassHttp Passwords",
            "KeePassXC-Browser Passwords"
        };
        
        private static void Main(string[] args)
        {
            var inputPath = args[0];
            var outputPath = args[1];
            var fileRoot = XDocument.Load(inputPath);
            Console.WriteLine("Loaded XML.");

            var outputObject = new BitwardenRoot();
            var groupsToProcess = new Queue<KeyValuePair<string, XElement>>();
            if (fileRoot.Root == null)
                throw new Exception("Root element missing");
            groupsToProcess.Enqueue(new KeyValuePair<string, XElement>("", fileRoot.Root.Descendants("Group").First()));

            while (groupsToProcess.TryDequeue(out var valuePair))
            {
                var groupKey = valuePair.Key;
                var element = valuePair.Value;
                
                string folderId = null;
                if (groupKey != "")
                {
                    folderId = Guid.NewGuid().ToString();
                    var newFolder = new Folder
                    {
                        Id = folderId,
                        Name = groupKey
                    };
                    outputObject.Folders.Add(newFolder);
                }

                var childElements = element.Elements("Group");
                foreach (var childElement in childElements)
                {
                    var childGroupName = childElement.Element("Name")?.Value;
                    if (childGroupName == null)
                        throw new Exception("Found group with no name, malformed file");
                    if (_blacklistedFolders.Contains(childGroupName))
                        childGroupName = "";
                    
                    var newGroupKey = $"{groupKey}/{childGroupName}";
                    if (groupKey == "")
                        newGroupKey = childGroupName;
                    Console.WriteLine($"Found group '{newGroupKey}'");
                    groupsToProcess.Enqueue(new KeyValuePair<string, XElement>(newGroupKey, childElement));
                }

                var childEntries = element.Elements("Entry");
                foreach (var entry in childEntries)
                {
                    var fields = entry.Elements("String").ToArray();
                    var title = fields.Where(x => x.Element("Key")?.Value == "Title").Select(x => x.Element("Value")?.Value)
                        .First();
                    if (_blacklistedFields.Contains(title))
                    {
                        continue;
                    }
                    var notes = fields.Where(x => x.Element("Key")?.Value == "Notes").Select(x => x.Element("Value")?.Value)
                        .First();
                    var username = fields.Where(x => x.Element("Key")?.Value == "UserName").Select(x => x.Element("Value")?.Value)
                        .First();
                    var password = fields.Where(x => x.Element("Key")?.Value == "Password").Select(x => x.Element("Value")?.Value)
                        .First();
                    var url = fields.Where(x => x.Element("Key")?.Value == "URL").Select(x => x.Element("Value")?.Value)
                        .First();

                    var newEntry = new Item
                    {
                        Id = Guid.NewGuid().ToString(),
                        Name = title,
                        Type = ItemType.Login,
                        FolderId = folderId,
                        Notes = notes,
                        Login = new ItemLogin
                        {
                            Username = username,
                            Password = password,
                            Totp = null,
                            Uris = new List<ItemLoginUri> {new ItemLoginUri {Match = null, Uri = url}}
                        },
                        CollectionIds = null,
                        Favorite = false,
                        Fields = fields.Where(x =>
                        {
                            var key = x.Element("Key")?.Value;
                            return key != "Title" && key != "Notes" && key != "UserName" && key != "Password" &&
                                   key != "URL" && !_blacklistedFields.Contains(key);
                        }).Select(x => new ItemField
                        {
                            Name = x.Element("Key")?.Value,
                            Type = x.Element("Value")?.Attribute("ProtectInMemory")?.Value == "True" ? ItemFieldType.HiddenText : ItemFieldType.Text,
                            Value = x.Element("Value")?.Value
                        }).ToList()
                    };
                    outputObject.Items.Add(newEntry);
                }
            }

            Console.WriteLine("Serializing output file...");
            var contractResolver = new DefaultContractResolver
            {
                NamingStrategy = new CamelCaseNamingStrategy()
            };
            var output = JsonConvert.SerializeObject(outputObject, new JsonSerializerSettings
            {
                ContractResolver = contractResolver,
                Formatting = Formatting.Indented
            });
            Console.WriteLine("Writing output file...");
            using (var file = File.OpenWrite(outputPath))
            {
                file.Write(Encoding.UTF8.GetBytes(output));
            }
            Console.WriteLine("Done!");
        }
    }
}