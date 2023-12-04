/* Autogenerated file. Manual edits will not be saved.*/

#nullable enable
using System;
using System.Linq;
using mud;
using UniRx;
using Property = System.Collections.Generic.Dictionary<string, object>;

namespace mudworld
{
    public class MyTableWithTwoKeysTable : MUDTable
    {
        public class MyTableWithTwoKeysTableUpdate : RecordUpdate
        {
            public uint? Value1;
            public uint? PreviousValue1;
            public uint? Value2;
            public uint? PreviousValue2;
        }

        public readonly static string ID = "MyTableWithTwoKeys";
        public static RxTable Table
        {
            get { return NetworkManager.Instance.ds.store[ID]; }
        }

        public override string GetTableId()
        {
            return ID;
        }

        public uint? Value1;
        public uint? Value2;

        public override Type TableType()
        {
            return typeof(MyTableWithTwoKeysTable);
        }

        public override Type TableUpdateType()
        {
            return typeof(MyTableWithTwoKeysTableUpdate);
        }

        public override bool Equals(object? obj)
        {
            MyTableWithTwoKeysTable other = (MyTableWithTwoKeysTable)obj;

            if (other == null)
            {
                return false;
            }
            if (Value1 != other.Value1)
            {
                return false;
            }
            if (Value2 != other.Value2)
            {
                return false;
            }
            return true;
        }

        public override void SetValues(params object[] functionParameters)
        {
            Value1 = (uint)functionParameters[0];

            Value2 = (uint)functionParameters[1];
        }

        public static IObservable<RecordUpdate> GetMyTableWithTwoKeysTableUpdates()
        {
            MyTableWithTwoKeysTable mudTable = new MyTableWithTwoKeysTable();

            return NetworkManager.Instance.sync.onUpdate
                .Where(update => update.Table.Name == ID)
                .Select(recordUpdate =>
                {
                    return mudTable.RecordUpdateToTyped(recordUpdate);
                });
        }

        public override void PropertyToTable(Property property)
        {
            Value1 = (uint)property["value1"];
            Value2 = (uint)property["value2"];
        }

        public override RecordUpdate RecordUpdateToTyped(RecordUpdate recordUpdate)
        {
            var currentValue = recordUpdate.CurrentRecordValue as Property;
            var previousValue = recordUpdate.PreviousRecordValue as Property;
            uint? currentValue1Typed = null;
            uint? previousValue1Typed = null;

            if (currentValue != null && currentValue.ContainsKey("value1"))
            {
                currentValue1Typed = (uint)currentValue["value1"];
            }

            if (previousValue != null && previousValue.ContainsKey("value1"))
            {
                previousValue1Typed = (uint)previousValue["value1"];
            }
            uint? currentValue2Typed = null;
            uint? previousValue2Typed = null;

            if (currentValue != null && currentValue.ContainsKey("value2"))
            {
                currentValue2Typed = (uint)currentValue["value2"];
            }

            if (previousValue != null && previousValue.ContainsKey("value2"))
            {
                previousValue2Typed = (uint)previousValue["value2"];
            }

            return new MyTableWithTwoKeysTableUpdate
            {
                Table = recordUpdate.Table,
                CurrentRecordValue = recordUpdate.CurrentRecordValue,
                PreviousRecordValue = recordUpdate.PreviousRecordValue,
                CurrentRecordKey = recordUpdate.CurrentRecordKey,
                PreviousRecordKey = recordUpdate.PreviousRecordKey,
                Type = recordUpdate.Type,
                Value1 = currentValue1Typed,
                PreviousValue1 = previousValue1Typed,
                Value2 = currentValue2Typed,
                PreviousValue2 = previousValue2Typed,
            };
        }
    }
}
