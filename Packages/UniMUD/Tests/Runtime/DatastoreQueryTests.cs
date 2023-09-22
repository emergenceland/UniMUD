using System.Collections.Generic;
using mud.Client;
using mud.Network;
using NUnit.Framework;
using UniRx;
using UnityEngine;
using v2;

namespace Tests.Runtime
{
    public class DatastoreQueryTests
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

            RxTable playerTable = _ds.CreateTable("", "Player", new Dictionary<string, SchemaAbiTypes.SchemaType>
            {
                { "value", SchemaAbiTypes.SchemaType.BOOL }
            });

            _ds.RegisterTable(positionTable);
            _ds.RegisterTable(healthTable);
            _ds.RegisterTable(playerTable);

            _components = new Dictionary<string, RxTable>
            {
                ["Position"] = positionTable,
                ["Health"] = healthTable,
                ["Player"] = playerTable,
            };
        }

        [Test]
        public void QueryAnd()
        {
            var position = Utils.CreateProperty(("x", 1), ("y", 2));
            var health = Utils.CreateProperty(("value", 100));

            _ds.Set(_components["Position"], "a", position);
            _ds.Set(_components["Position"], "b", position);
            _ds.Set(_components["Health"], "a", health);

            var withPositionAndHealth = new Query().In(_components["Position"]).In(_components["Health"]);
            using var q2 = _ds.RunQuery(withPositionAndHealth).GetEnumerator();
            Assert.IsTrue(q2.MoveNext());

            var firstResult = q2.Current;
            Assert.IsNotNull(firstResult);
            Assert.AreEqual("a", firstResult.key);
            Assert.IsTrue(q2.MoveNext());
        }

        [Test]
        public void QueryNot()
        {
            var position = Utils.CreateProperty(("x", 1), ("y", 2));
            var health = Utils.CreateProperty(("value", 100));
            var player = Utils.CreateProperty(("value", true));

            _ds.Set(_components["Position"], "a", position);
            _ds.Set(_components["Position"], "b", position);

            _ds.Set(_components["Player"], "a", player);

            _ds.Set(_components["Health"], "a", health);
            _ds.Set(_components["Health"], "b", health);

            var positionNoPlayers = new Query().In(_components["Position"]).Not(_components["Player"]);
            using var q2 = _ds.RunQuery(positionNoPlayers).GetEnumerator();
            Assert.IsTrue(q2.MoveNext());

            var firstResult = q2.Current;
            Assert.IsNotNull(firstResult);
            Assert.AreEqual("b", firstResult.key);
            Assert.IsFalse(q2.MoveNext());
        }

        [Test]
        // Get all records of entities that have Component X and Component Y, return only rows from Component X
        public void QuerySelect()
        {
            var position = Utils.CreateProperty(("x", 1), ("y", 2));
            var h1 = Utils.CreateProperty(("value", 100));

            _ds.Set(_components["Position"], "a", position);
            _ds.Set(_components["Position"], "b", position);
            _ds.Set(_components["Health"], "a", h1);

            var hasHealth = new Query().Select(_components["Health"].Id).In(_components["Position"])
                .In(_components["Health"]);
            using var q2 = _ds.RunQuery(hasHealth).GetEnumerator();
            Assert.IsTrue(q2.MoveNext());

            var firstResult = q2.Current;
            Assert.IsNotNull(firstResult);
            Assert.AreEqual(_components["Health"].Id, firstResult.tableId);
            Assert.IsFalse(q2.MoveNext());
        }

        [Test]
        public void QueryAttribute()
        {
            var position = Utils.CreateProperty(("x", 1), ("y", 2));
            var h1 = Utils.CreateProperty(("value", 100));
            var h2 = Utils.CreateProperty(("value", 50));

            _ds.Set(_components["Position"], "a", position);
            _ds.Set(_components["Position"], "b", position);
            _ds.Set(_components["Health"], "a", h1);
            _ds.Set(_components["Health"], "b", h2);

            var fullHealth = new Query().Select(_components["Health"].Id).In(_components["Position"])
                .In(_components["Health"], new[] { Condition.Has("value", 100) });
            using var q2 = _ds.RunQuery(fullHealth).GetEnumerator();
            Assert.IsTrue(q2.MoveNext());

            var firstResult = q2.Current;
            Assert.IsNotNull(firstResult);
            Assert.AreEqual("a", firstResult.key);
            Assert.IsFalse(q2.MoveNext());
        }

        [Test]
        public void RxQuerySimple()
        {
            var position = Utils.CreateProperty(("x", 1), ("y", 2));
            var position2 = Utils.CreateProperty(("x", 1), ("y", 1));

            _ds.Set(_components["Position"], "a", position);
            _ds.Set(_components["Position"], "b", position);

            var hasPosition = new Query().In(_components["Position"]);

            var set = new List<RxRecord>();
            var removed = new List<RxRecord>();
            var disposer = _ds.RxQuery(hasPosition, false).Subscribe(
                updates =>
                {
                    set.AddRange(updates.SetRecords);
                    removed.AddRange(updates.RemovedRecords);
                });

            // change position of entity A 
            Debug.Log("Changing position of entity a...");
            _ds.Update(_components["Position"], "a", position2);
            // change position of entity B
            Debug.Log("Changing position of entity b...");
            _ds.Update(_components["Position"], "b", position2); 

            // make sure we only have 2 updates
            Assert.AreEqual(2, set.Count);

            // delete 1 position record 
            _ds.Delete(_components["Position"], "a");

            Assert.AreEqual(1, removed.Count);
            disposer.Dispose();
        }

        [Test]
        public void RxQueryWithCondition()
        {
            var position = Utils.CreateProperty(("x", 1), ("y", 2));
            var position2 = Utils.CreateProperty(("x", 1), ("y", 1));
            var h1 = Utils.CreateProperty(("value", 100));
            var h2 = Utils.CreateProperty(("value", 50));

            _ds.Set(_components["Position"], "a", position);
            _ds.Set(_components["Position"], "b", position);
            _ds.Set(_components["Health"], "a", h1); // A has full health, we should only get updates for A
            _ds.Set(_components["Health"], "b", h2); // B should not trigger updates

            var fullHealth = new Query().Select(_components["Health"].Id).In(_components["Position"])
                .In(_components["Health"], new[] { Condition.Has("value", 100) });

            var set = new List<RxRecord>();
            var removed = new List<RxRecord>();
            // every time something with full health changes either health of position, we get a reaction (health value)
            var disposer = _ds.RxQuery(fullHealth, false).Subscribe(
                (updates =>
                {
                    set.AddRange(updates.SetRecords);
                    removed.AddRange(updates.RemovedRecords);
                }));

            // change position of entity A 
            _ds.Update(_components["Position"], "a", position2);
            // change position of entity B
            _ds.Update(_components["Position"], "b",
                position2); // should not update because it doesn't have full health
            // change position of entity A back to original
            _ds.Update(_components["Position"], "a", position); // should update

            // make sure we only have 2 updates
            Assert.AreEqual(2, set.Count);

            // delete 1 health record 
            _ds.Delete(_components["Health"], "a");

            Assert.AreEqual(1, removed.Count);
            disposer.Dispose();
        }

        [Test]
        public void RxQueryNoCondition()
        {
            var position = Utils.CreateProperty(("x", 1), ("y", 2));
            var position2 = Utils.CreateProperty(("x", 1), ("y", 1));
            var h1 = Utils.CreateProperty(("value", 100));
            var h2 = Utils.CreateProperty(("value", 50));

            _ds.Set(_components["Position"], "a", position);
            _ds.Set(_components["Position"], "b", position);
            _ds.Set(_components["Health"], "a", h1); // A has full health, we should only get updates for A
            _ds.Set(_components["Health"], "b", h2); // B should not trigger updates

            var fullHealth = new Query().Select(_components["Health"].Id).In(_components["Position"])
                .In(_components["Health"]);

            var set = new List<RxRecord>();
            var removed = new List<RxRecord>();
            // every time something with full health changes either health of position, we get a reaction (health value)
            var disposer = _ds.RxQuery(fullHealth, false).Subscribe(update =>
            {
                set.AddRange(update.SetRecords);
                removed.AddRange(update.RemovedRecords);
            });

            // change position of entity A 
            _ds.Update(_components["Position"], "a", position2);
            // change position of entity B
            _ds.Update(_components["Position"], "b", position2);
            // change position of entity A back to original
            _ds.Update(_components["Position"], "a", position); // should update

            // make sure we only have 3 updates
            Assert.AreEqual(3, set.Count);

            // delete 1 health record 
            _ds.Delete(_components["Health"], "a");

            Assert.AreEqual(1, removed.Count);
            disposer.Dispose();
        }
    }
}
