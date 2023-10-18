using System;
using mud.IStore.ContractDefinition;

namespace mud
{
    public abstract class DecodedEventUnion
    {
        public abstract void Match(Action<StoreSetRecordEventDTO> onSetRecord,
            Action<StoreSpliceStaticDataEventDTO> onSpliceStatic,
            Action<StoreSpliceDynamicDataEventDTO> onSpliceDynamic,
            Action<StoreDeleteRecordEventDTO> onDeleteRecord);
    }

    public class DecodedSetRecord : DecodedEventUnion
    {
        public StoreSetRecordEventDTO Value { get; }
        public DecodedSetRecord(StoreSetRecordEventDTO value) => Value = value;

        public override void Match(
            Action<StoreSetRecordEventDTO> onSetRecord,
            Action<StoreSpliceStaticDataEventDTO> onSpliceStatic,
            Action<StoreSpliceDynamicDataEventDTO> onSpliceDynamic,
            Action<StoreDeleteRecordEventDTO> onDeleteRecord
        )
        {
            onSetRecord(Value);
        }
    }

    public class DecodedSpliceStatic : DecodedEventUnion
    {
        public StoreSpliceStaticDataEventDTO Value { get; }
        public DecodedSpliceStatic(StoreSpliceStaticDataEventDTO value) => Value = value;

        public override void Match(
            Action<StoreSetRecordEventDTO> onSetRecord,
            Action<StoreSpliceStaticDataEventDTO> onSpliceStatic,
            Action<StoreSpliceDynamicDataEventDTO> onSpliceDynamic,
            Action<StoreDeleteRecordEventDTO> onDeleteRecord
        )
        {
            onSpliceStatic(Value);
        }
    }

    public class DecodedSpliceDynamic : DecodedEventUnion
    {
        public StoreSpliceDynamicDataEventDTO Value { get; }
        public DecodedSpliceDynamic(StoreSpliceDynamicDataEventDTO value) => Value = value;

        public override void Match(
            Action<StoreSetRecordEventDTO> onSetRecord,
            Action<StoreSpliceStaticDataEventDTO> onSpliceStatic,
            Action<StoreSpliceDynamicDataEventDTO> onSpliceDynamic,
            Action<StoreDeleteRecordEventDTO> onDeleteRecord
        )
        {
            onSpliceDynamic(Value);
        }
    }

    public class DecodedDeleteRecord : DecodedEventUnion
    {
        public StoreDeleteRecordEventDTO Value { get; }
        public DecodedDeleteRecord(StoreDeleteRecordEventDTO value) => Value = value;

        public override void Match(
            Action<StoreSetRecordEventDTO> onSetRecord,
            Action<StoreSpliceStaticDataEventDTO> onSpliceStatic,
            Action<StoreSpliceDynamicDataEventDTO> onSpliceDynamic,
            Action<StoreDeleteRecordEventDTO> onDeleteRecord
        )
        {
            onDeleteRecord(Value);
        }
    }
}
