using System.Collections.Generic;
using System.Dynamic;
using mud.Client;
using mud.Network;
using NUnit.Framework;
using v2;
using Common = mud.Client.Common;

namespace Tests.Runtime
{
    public class DatastoreTests
    {
        private RxDatastore _ds;
        private Dictionary<string, RxTable> _components;


        [SetUp]
        public void BeforeEach()
        {
            _ds = new RxDatastore();

            RxTable positionTable = _ds.CreateTable("", "Position", new Dictionary<string, SchemaAbiTypes.SchemaType>
            {
                { "x", SchemaAbiTypes.SchemaType.UINT32 },
                { "y", SchemaAbiTypes.SchemaType.UINT32 }
            });
            
            RxTable healthTable = _ds.CreateTable("", "Health", new Dictionary<string, SchemaAbiTypes.SchemaType>
            {
                { "value", SchemaAbiTypes.SchemaType.UINT32 }
            });
            
            _ds.RegisterTable(positionTable);
            _ds.RegisterTable(healthTable);
            
            _components = new Dictionary<string, RxTable>
            {
                ["Position"] = positionTable,
                ["Health"] = healthTable,
            };
        }

        [Test]
        public void SetValue()
        {
            var position = Utils.CreateProperty(("x", 1), ("y", 2));
            var health = Utils.CreateProperty(("value", 100));
            
            _ds.Set(_components["Position"], "a", position);
            _ds.Set(_components["Position"], "b", position);
            _ds.Set(_components["Health"], "a", health);

            Assert.AreEqual(2, _ds.store.Count); // should have 2 RxTables
            Assert.AreEqual(2,
                _ds.store[_components["Position"].Id].Values.Count); // should have 2 records in Position table

            Assert.AreEqual(100, _ds.GetValue(_components["Health"], "a")?.value["value"]); // should have 100 health 
        }

        [Test]
        public void UpdateValue()
        {
            var position = Utils.CreateProperty(("x", 1), ("y", 2));

            _ds.Set(_components["Position"], "a", position);

            // make sure the value is set
            Assert.AreEqual(1, _ds.store[_components["Position"].Id].Values.Count);
            Assert.AreEqual(1, _ds.GetValue(_components["Position"], "a")?.value["x"]);
            Assert.AreEqual(2, _ds.GetValue(_components["Position"], "a")?.value["y"]);

            // update the value
            var position2 = Utils.CreateProperty(("x", 3), ("y", 4));
            _ds.Update(_components["Position"], "a", position2, position);

            // use GetValue
            Assert.AreEqual(3, _ds.GetValue(_components["Position"], "a")?.value["x"]);
            Assert.AreEqual(4, _ds.GetValue(_components["Position"], "a")?.value["y"]);
        }

        [Test]
        public void DeleteValue()
        {
            var position = Utils.CreateProperty(("x", 1), ("y", 2));

            _ds.Set(_components["Position"], "a", position);
            Assert.AreEqual(1, _ds.store[_components["Position"].Id].Values.Count);

            _ds.Delete(_components["Position"], "a");
            Assert.IsNull(_ds.GetValue(_components["Position"], "a"));
        }
    }
}
