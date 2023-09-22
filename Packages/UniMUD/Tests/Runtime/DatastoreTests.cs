// using mud.Client;
// using mud.Network;
// using mud.Network.schemas;
// using NUnit.Framework;
//
// namespace Tests.Runtime
// {
//     public class DatastoreTests
//     {
//         private Datastore _ds;
//
//         private TableId Position;
//         private TableId Health;
//
//         [SetUp]
//         public void BeforeEach()
//         {
//             _ds = new Datastore();
//             Position = new TableId("", "Position");
//             Health = new TableId("", "Health");
//
//             _ds.RegisterTable(Position);
//             _ds.RegisterTable(Health);
//         }
//
//         [Test]
//         public void SetValue()
//         {
//             var position = Utils.CreateProperty(("x", 1), ("y", 2));
//             var health = Utils.CreateProperty(("value", 100));
//
//             _ds.Set(Position, "a", position);
//             _ds.Set(Position, "b", position);
//             _ds.Set(Health, "a", health);
//
//             Assert.AreEqual(2, _ds.store.Count);
//             Assert.AreEqual(2, _ds.store[Position.ToString()].Records.Count);
//             
//             Assert.AreEqual(100, _ds.GetValue(Health, "a")?.value["value"]);
//         }
//
//         [Test]
//         public void UpdateValue()
//         {
//             var position = Utils.CreateProperty(("x", 1), ("y", 2));
//
//             _ds.Set(Position, "a", position);
//
//             // make sure the value is set
//             Assert.AreEqual(1, _ds.store[Position.ToString()].Records.Count);
//             Assert.AreEqual(1, _ds.GetValue(Position, "a")?.value["x"]);
//             Assert.AreEqual(2, _ds.GetValue(Position, "a")?.value["y"]);
//             
//             // update the value
//             var position2 = Utils.CreateProperty(("x", 3), ("y", 4));
//             _ds.Update(Position, "a", position2, position);
//             
//             // use GetValue
//             Assert.AreEqual(3, _ds.GetValue(Position, "a")?.value["x"]);
//             Assert.AreEqual(4, _ds.GetValue(Position, "a")?.value["y"]);
//         }
//
//         [Test]
//         public void DeleteValue()
//         {
//             var position = Utils.CreateProperty(("x", 1), ("y", 2));
//             
//             _ds.Set(Position, "a", position);
//             Assert.AreEqual(1, _ds.store[Position.ToString()].Records.Count);
//             
//             _ds.Delete(Position, "a");
//             Assert.IsNull(_ds.GetValue(Position, "a"));
//         }
//
//     }
// }
