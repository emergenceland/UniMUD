using System.Collections.Generic;
using System.Numerics;

namespace mud.Network
{
    using Property = Dictionary<string, object>;

    public class Types
    {
        public struct TxMetadata
        {
            public string To { get; set; }
            public byte[] Data { get; set; }
            public int Value { get; set; }
        }

        public struct EntityOptions
        {
            public string Id { get; set; }
            public string? IdSuffix { get; set; }
        }

        public enum NetworkEvents
        {
            SystemCall,
            NetworkComponentUpdate
        }

        public class NetworkEvent
        {
            public NetworkTableUpdate TableUpdate { get; set; }
            // TODO: System call
        }

        public class NetworkTableUpdate
        {
            public string Component { get; set; } // component key
            public NetworkEvents Type { get; set; }
            public Property? Value { get; set; }
            public Property? PartialValue { get; set; }
            public Property? InitialValue { get; set; }
            public EntityID Entity { get; set; }
            public bool LastEventInTx { get; set; }
            public string TxHash { get; set; }
            public TxMetadata TxMetadata { get; set; }
            public BigInteger BlockNumber { get; set; }
            public BigInteger? LogIndex { get; set; }
            public bool? Ephemeral { get; set; }
        }

        public struct EntityID
        {
            public string Value;
        }
    }
}
