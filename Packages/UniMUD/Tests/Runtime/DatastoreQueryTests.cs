using System;
using System.Collections.Generic;
using mud.Client;
using mud.Network;
using mud.Network.schemas;
using Newtonsoft.Json;
using NUnit.Framework;
using Debug = UnityEngine.Debug;

namespace Tests.Runtime
{
    public class DatastoreQueryTests
    {
        private Datastore _ds;

        private TableId Position;
        private TableId Health;
        private TableId Player;
        private TableId CarriedBy;

        [SetUp]
        public void BeforeEach()
        {
            _ds = new Datastore();
            Position = new TableId("", "Position");
            Health = new TableId("", "Health");
            Player = new TableId("", "Player");
            CarriedBy = new TableId("", "CarriedBy");

            _ds.RegisterTable(Position);
            _ds.RegisterTable(Health);
            _ds.RegisterTable(Player);
            _ds.RegisterTable(CarriedBy);
        }

        [Test]
        public void QueryAnd()
        {
            var position = Utils.CreateProperty(("x", 1), ("y", 2));
            var health = Utils.CreateProperty(("value", 100));

            _ds.Set(Position, "a", position);
            _ds.Set(Position, "b", position);
            _ds.Set(Health, "a", health);

            var withPositionAndHealth = new Query().In(Position).In(Health);
            using var q2 = _ds.RunQuery(withPositionAndHealth).GetEnumerator();
            Assert.IsTrue(q2.MoveNext());

            var firstResult = q2.Current;
            Assert.IsNotNull(firstResult);
            Debug.Log(JsonConvert.SerializeObject(firstResult));
            Assert.AreEqual("a", firstResult.key);
            Assert.IsTrue(q2.MoveNext());
        }

        [Test]
        public void QueryNot()
        {
            var position = Utils.CreateProperty(("x", 1), ("y", 2));
            var health = Utils.CreateProperty(("value", 100));
            var player = Utils.CreateProperty(("value", true));

            _ds.Set(Position, "a", position);
            _ds.Set(Position, "b", position);

            _ds.Set(Player, "a", player);

            _ds.Set(Health, "a", health);
            _ds.Set(Health, "b", health);

            var positionNoPlayers = new Query().In(Position).Not(Player);
            using var q2 = _ds.RunQuery(positionNoPlayers).GetEnumerator();
            Assert.IsTrue(q2.MoveNext());

            var firstResult = q2.Current;
            Assert.IsNotNull(firstResult);
            Debug.Log(JsonConvert.SerializeObject(firstResult));
            Assert.AreEqual("b", firstResult.key);
            Assert.IsFalse(q2.MoveNext());
        }

        [Test]
        // Get all records of entities that have Component X and Component Y, return only rows from Component X
        public void QuerySelect()
        {
            var position = Utils.CreateProperty(("x", 1), ("y", 2));
            var h1 = Utils.CreateProperty(("value", 100));

            _ds.Set(Position, "a", position);
            _ds.Set(Position, "b", position);
            _ds.Set(Health, "a", h1);

            var hasHealth = new Query().Select(Health).In(Position).In(Health);
            using var q2 = _ds.RunQuery(hasHealth).GetEnumerator();
            Assert.IsTrue(q2.MoveNext());

            var firstResult = q2.Current;
            Assert.IsNotNull(firstResult);
            Debug.Log(JsonConvert.SerializeObject(firstResult));
            Assert.AreEqual("TableId<:Health>", firstResult.table);
            Assert.IsFalse(q2.MoveNext());
        }

        [Test]
        public void QueryAttribute()
        {
            var position = Utils.CreateProperty(("x", 1), ("y", 2));
            var h1 = Utils.CreateProperty(("value", 100));
            var h2 = Utils.CreateProperty(("value", 50));

            _ds.Set(Position, "a", position);
            _ds.Set(Position, "b", position);
            _ds.Set(Health, "a", h1);
            _ds.Set(Health, "b", h2);

            var fullHealth = new Query().Select(Health).In(Position).In(Health, new[] { Condition.Has("value", 100) });
            using var q2 = _ds.RunQuery(fullHealth).GetEnumerator();
            Assert.IsTrue(q2.MoveNext());

            var firstResult = q2.Current;
            Assert.IsNotNull(firstResult);
            Debug.Log(JsonConvert.SerializeObject(firstResult));
            Assert.AreEqual("a", firstResult.key);
            Assert.IsFalse(q2.MoveNext());
        }

        [Test]
        public void RxQuery()
        {
            var position = Utils.CreateProperty(("x", 1), ("y", 2));
            var h1 = Utils.CreateProperty(("value", 100));
            var h2 = Utils.CreateProperty(("value", 50));

            var fullHealth = new Query().Select(Health).In(Position).In(Health, new[] { Condition.Has("value", 100) });

            var set = new List<Record>();
            var removed = new List<Record>();
            var disposer = _ds.RxQuery(fullHealth).Subscribe(((List<Record> set, List<Record> removed) updates )=>
            {
                set.AddRange(set);
                removed.AddRange(removed);
                Debug.Log("REACTION: " + JsonConvert.SerializeObject(updates));
            });

            _ds.Set(Position, "a", position);
            _ds.Set(Position, "b", position);
            _ds.Set(Health, "a", h1);
            _ds.Set(Health, "b", h2);

            Assert.IsNotEmpty(set);
            Debug.Log("Length: " + set.Count);
            Debug.Log("Last: " + JsonConvert.SerializeObject(set));
            
            // make sure we only have 2 records
            Assert.AreEqual(2, set.Count);
            
            // delete 1 health record 
            _ds.Delete(Health, "a");
            
            Assert.AreEqual(1, removed.Count);
            disposer.Dispose();
        }
    }
}
