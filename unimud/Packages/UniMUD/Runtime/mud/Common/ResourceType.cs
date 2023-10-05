using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;

namespace mud
{
    public enum ResourceType
    {
        Table,
        OffchainTable,
        Namespace,
        Module,
        System
    }

    public struct ResourceID
    {
        public string Namespace;
        public string Name;
        public ResourceType Type;
    }

    public static partial class Common
    {
        private static readonly Dictionary<ResourceType, string> ResourceTypeIds = new()
        {
            // keep these in sync with worldResourceTypes.sol
            { ResourceType.Table, "tb" },
            { ResourceType.OffchainTable, "ot" },
            // keep these in sync with worldResourceTypes.sol
            { ResourceType.Namespace, "ns" },
            { ResourceType.Module, "md" },
            { ResourceType.System, "sy" }
        };

        public static string ResourceIDToHex(ResourceID resourceId)
        {
            var typeId = ResourceTypeIds[resourceId.Type];

            return ConcatHex(new[]
            {
                StringToHex(typeId, 2),
                StringToHex(resourceId.Namespace.Length >= 14 ? resourceId.Namespace[..14] : resourceId.Namespace, 14),
                StringToHex(resourceId.Name.Length >= 16 ? resourceId.Name[..16] : resourceId.Name, 16),
            });
        }

        public static Dictionary<string, ResourceType> ResourceTypeIdToType = ResourceTypeIds
            .ToDictionary(kvp => kvp.Value, kvp => kvp.Key);

        public static ResourceType? GetResourceType(string resourceTypeId)
        {
            var type = ResourceTypeIdToType[resourceTypeId];
            return type;
        }

        public static ResourceID HexToResourceId(string hex)
        {
            var resourceTypeId = Regex.Replace(HexToUTF8(SliceHex(hex, 0, 2)), @"\0 +$", "");
            var type = GetResourceType(resourceTypeId);
            var ns = Regex.Replace(HexToUTF8(SliceHex(hex, 2, 16)), @"\0 +$", "");
            var name = Regex.Replace(HexToUTF8(SliceHex(hex, 16, 32)), @"\0 +$", "");

            if (type == null) throw new InvalidCastException($"Unknown resource type: {resourceTypeId}");

            return new ResourceID
            {
                Type = type.Value,
                Namespace = FormatGetRecordResult(ns)[0],
                Name = FormatGetRecordResult(name)[0]
            };
        }
    }
}
