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
        public void RxQuerySimple()
        {
            var position = Utils.CreateProperty(("x", 1), ("y", 2));
            var position2 = Utils.CreateProperty(("x", 1), ("y", 1));
            var h1 = Utils.CreateProperty(("value", 100));
            var h2 = Utils.CreateProperty(("value", 50));
            
            _ds.Set(Position, "a", position);
            _ds.Set(Position, "b", position);

            var fullHealth = new Query().In(Position);

            var set = new List<Record>();
            var removed = new List<Record>();
            var disposer = _ds.RxQuery(fullHealth, false).Subscribe(((List<Record> set, List<Record> removed) updates )=>
            {
                set.AddRange(updates.set);
                removed.AddRange(updates.removed);
                Debug.Log("REACTION: " + JsonConvert.SerializeObject(updates));
            });
            
            // change position of entity A 
            _ds.Update(Position, "a", position2);
            // change position of entity B
            _ds.Update(Position, "b", position2); // should not update because it doesn't have full health

            Debug.Log("Length: " + set.Count);
            Debug.Log("Last: " + JsonConvert.SerializeObject(set));
            
            // make sure we only have 2 updates
            Assert.AreEqual(2, set.Count);
            
            // delete 1 position record 
            _ds.Delete(Position, "a");
            
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
            
            _ds.Set(Position, "a", position);
            _ds.Set(Position, "b", position);
            _ds.Set(Health, "a", h1); // A has full health, we should only get updates for A
            _ds.Set(Health, "b", h2); // B should not trigger updates

            var fullHealth = new Query().Select(Health).In(Position).In(Health, new[] { Condition.Has("value", 100) });

            var set = new List<Record>();
            var removed = new List<Record>();
            // every time something with full health changes either health of position, we get a reaction (health value)
            var disposer = _ds.RxQuery(fullHealth, false).Subscribe(((List<Record> set, List<Record> removed) updates )=>
            {
                set.AddRange(updates.set);
                removed.AddRange(updates.removed);
                Debug.Log("REACTION: " + JsonConvert.SerializeObject(updates));
            });
            
            // change position of entity A 
            _ds.Update(Position, "a", position2);
            // change position of entity B
            _ds.Update(Position, "b", position2); // should not update because it doesn't have full health
            // change position of entity A back to original
            _ds.Update(Position, "a", position); // should update

            Debug.Log("Length: " + set.Count);
            Debug.Log("Last: " + JsonConvert.SerializeObject(set));
            
            // make sure we only have 2 updates
            Assert.AreEqual(2, set.Count);
            
            // delete 1 health record 
            _ds.Delete(Health, "a");
            
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
            
            _ds.Set(Position, "a", position);
            _ds.Set(Position, "b", position);
            _ds.Set(Health, "a", h1); // A has full health, we should only get updates for A
            _ds.Set(Health, "b", h2); // B should not trigger updates

            var fullHealth = new Query().Select(Health).In(Position).In(Health);

            var set = new List<Record>();
            var removed = new List<Record>();
            // every time something with full health changes either health of position, we get a reaction (health value)
            var disposer = _ds.RxQuery(fullHealth, false).Subscribe(((List<Record> set, List<Record> removed) updates )=>
            {
                set.AddRange(updates.set);
                removed.AddRange(updates.removed);
                Debug.Log("REACTION: " + JsonConvert.SerializeObject(updates));
            });
            
            // change position of entity A 
            _ds.Update(Position, "a", position2);
            // change position of entity B
            _ds.Update(Position, "b", position2); 
            // change position of entity A back to original
            _ds.Update(Position, "a", position); // should update

            Debug.Log("Length: " + set.Count);
            Debug.Log("Last: " + JsonConvert.SerializeObject(set));
            
            // make sure we only have 3 updates
            Assert.AreEqual(3, set.Count);
            
            // delete 1 health record 
            _ds.Delete(Health, "a");
            
            Assert.AreEqual(1, removed.Count);
            disposer.Dispose();
        }


    }
    
    
}
