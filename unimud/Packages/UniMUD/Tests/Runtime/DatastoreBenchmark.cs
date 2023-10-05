using System;
using System.Collections.Generic;
using System.Diagnostics;
using NUnit.Framework;
using UniRx;
using mud;
using Debug = UnityEngine.Debug;

namespace Tests.Runtime
{
    public class DatastoreBenchmark
    {
        private RxDatastore _ds;
        private Dictionary<string, RxTable> _components;

        private List<string> randomWriteData;

        [SetUp]
        public void BeforeEach()
        {
            _ds = new RxDatastore();

            RxTable positionTable = _ds.CreateTable("", "Position", new Dictionary<string, SchemaAbiTypes.SchemaType>
            {
                { "x", SchemaAbiTypes.SchemaType.UINT32 },
                { "y", SchemaAbiTypes.SchemaType.UINT32 }
            });

            _ds.RegisterTable(positionTable);

            _components = new Dictionary<string, RxTable>
            {
                ["Position"] = positionTable,
            };
        }

        private long MeasureTime(Action fn)
        {
            var stopwatch = new Stopwatch();
            stopwatch.Start();
            fn();
            stopwatch.Stop();
            return stopwatch.ElapsedMilliseconds;
        }

        [SetUp]
        public void Setup()
        {
            var random = new Random();

            randomWriteData = new List<string>(10000);
            for (var i = 0; i < 10000; i++)
            {
                randomWriteData.Add(random.Next().ToString());
            }
        }

        [Test]
        public void BenchmarkReadWriteDelete()
        {
            // Perform 1k write operations
            var writeTime = MeasureTime(() =>
            {
                for (var i = 0; i < 10000; i++)
                {
                    var position = Common.CreateProperty(("x", randomWriteData[i]), ("y", randomWriteData[i]));
                    _ds.Set(_components["Position"], $"Key{i}", position);
                }
            });
            Debug.Log($"Time for 10k writes: {writeTime} ms");
            Debug.Log($"Average time per write op: {writeTime / 10000.0} ms");

            var readTime = MeasureTime(() =>
            {
                for (var i = 0; i < 10000; i++)
                {
                    _ds.GetValue(_components["Position"], $"Key{i}");
                }
            });
            Debug.Log($"Time for 10k reads: {readTime} ms");
            Debug.Log($"Average time per read op: {readTime / 10000.0} ms");

            var deleteTime = MeasureTime(() =>
            {
                for (var i = 0; i < 10000; i++)
                {
                    _ds.Delete(_components["Position"], $"Key{i}");
                }
            });
            Debug.Log($"Time for 10k deletes: {deleteTime} ms");
            Debug.Log($"Average time per delete op: {deleteTime / 10000.0} ms");
        }

        [Test]
        public void BenchmarkRxQuery()
        {
            var result = new List<RxRecord>();
            var query = new Query().In(_components["Position"]);
            var disposer =
                _ds.RxQuery(query, false).Subscribe(update => { result.AddRange(update.SetRecords); });
            var queryTime = MeasureTime(() =>
            {
                for (var i = 0; i < 10000; i++)
                {
                    var position = Common.CreateProperty(("x", randomWriteData[i]), ("y", randomWriteData[i]));
                    _ds.Update(_components["Position"], $"Key{i}", position);
                }
            });
            Debug.Log($"Time for 10k reactive queries: {queryTime} ms");
            Debug.Log($"Average time per query op: {queryTime / 10000.0} ms");
            disposer.Dispose();
        }
    }
}
