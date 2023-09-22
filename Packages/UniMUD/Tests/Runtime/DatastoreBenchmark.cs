// using System;
// using System.Collections.Generic;
// using System.Diagnostics;
// using mud.Client;
// using mud.Network;
// using mud.Network.schemas;
// using NUnit.Framework;
// using Debug = UnityEngine.Debug;
//
// namespace Tests.Runtime
// {
//     public class DatastoreBenchmark
//     {
//         private Datastore _ds;
//
//         private TableId Position;
//         private List<string> randomWriteData;
//
//         [SetUp]
//         public void BeforeEach()
//         {
//             _ds = new Datastore();
//             Position = new TableId("", "Position");
//
//             _ds.RegisterTable(Position);
//         }
//
//         private long MeasureTime(Action fn)
//         {
//             var stopwatch = new Stopwatch();
//             stopwatch.Start();
//             fn();
//             stopwatch.Stop();
//             return stopwatch.ElapsedMilliseconds;
//         }
//
//         [SetUp]
//         public void Setup()
//         {
//             var random = new Random();
//
//             randomWriteData = new List<string>(10000);
//             for (var i = 0; i < 10000; i++)
//             {
//                 randomWriteData.Add(random.Next().ToString());
//             }
//         }
//
//         [Test]
//         public void BenchmarkReadWriteDelete()
//         {
//             // Perform 1k write operations
//             var writeTime = MeasureTime(() =>
//             {
//                 for (var i = 0; i < 10000; i++)
//                 {
//                     var position = Utils.CreateProperty(("x", randomWriteData[i]), ("y", randomWriteData[i]));
//                     _ds.Set(Position, $"Key{i}", position);
//                 }
//             });
//             Debug.Log($"Time for 10k writes: {writeTime} ms");
//             Debug.Log($"Average time per write op: {writeTime / 10000.0} ms");
//
//             var readTime = MeasureTime(() =>
//             {
//                 for (var i = 0; i < 10000; i++)
//                 {
//                     _ds.GetValue(Position, $"Key{i}");
//                 }
//             });
//             Debug.Log($"Time for 10k reads: {readTime} ms");
//             Debug.Log($"Average time per read op: {readTime / 10000.0} ms");
//
//             var deleteTime = MeasureTime(() =>
//             {
//                 for (var i = 0; i < 10000; i++)
//                 {
//                     _ds.Delete(Position, $"Key{i}");
//                 }
//             });
//             Debug.Log($"Time for 10k deletes: {deleteTime} ms");
//             Debug.Log($"Average time per delete op: {deleteTime / 10000.0} ms");
//         }
//
//         // [Test]
//         // public void BenchmarkRxQuery()
//         // {
//         //     var result = new List<Record>();
//         //     var query = new Query().In(Position);
//         //     var disposer = _ds.RxQuery(query).Subscribe(res => result.AddRange(res.SetRecords));
//         //     var queryTime = MeasureTime(() =>
//         //     {
//         //         for (var i = 0; i < 10000; i++)
//         //         {
//         //             var position = Utils.CreateProperty(("x", randomWriteData[i]), ("y", randomWriteData[i]));
//         //             _ds.Update(Position, $"Key{i}", position);
//         //         }
//         //     });
//         //     Debug.Log($"Time for 10k reactive queries: {queryTime} ms");
//         //     Debug.Log($"Average time per query op: {queryTime / 10000.0} ms");
//         //     disposer.Dispose();
//         // }
//     }
// }
