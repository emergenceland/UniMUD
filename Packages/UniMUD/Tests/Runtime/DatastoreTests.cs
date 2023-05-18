using System.Collections.Generic;
using System.Diagnostics;
using mud.Client;
using NUnit.Framework;
using Debug = UnityEngine.Debug;
using Types = mud.Client.Types;

public class DatastoreTests
{
    private Datastore _ds;
    private int _key;

    private readonly Dictionary<string, Types.Type> _schema = new()
    {
        { "x", Types.Type.Number },
        { "y", Types.Type.Number }
    };

    [SetUp]
    public void Setup()
    {
        _ds = new Datastore();
        _ds.RegisterTable("TableId<:Position>", "Position", _schema);

        Assert.AreEqual("TableId<:Position>", _ds.GetTableId("Position"));
        Assert.AreEqual("Position", _ds.GetTableName("TableId<:Position>"));
    }

    [Test]
    public void DatastoreSetValue()
    {
        var position = ClientUtils.CreateProperty(("x", 1), ("y", 2));
        _ds.SetValue("TableId<:Position>", "123", position);

        var tableQuery = new Query().Find("?x", "?y")
            .Where("TableId<:Position>", "123", "x", "?x")
            .Where("TableId<:Position>", "123", "y", "?y");
        using var q = _ds.Query(tableQuery).GetEnumerator();
        Assert.IsTrue(q.MoveNext()); 

        var firstResult = q.Current;
        Assert.IsNotNull(firstResult);
        Assert.AreEqual(1, firstResult["x"]);
        Assert.AreEqual(2, firstResult["y"]);

        Assert.IsFalse(q.MoveNext()); // verify there are no more results
    }

    [Test]
    public void DatastoreRemoveKey()
    {
        var position = ClientUtils.CreateProperty(("x", 1), ("y", 2));

        _ds.SetValue("TableId<:Position>", "123", position);
        _ds.DeleteValue("TableId<:Position>", "123");
        var tableQuery = new Query().Find("?x", "?y").Where("TableId<:Position>", "123", "?x", "?y");
        var q = _ds.Query(tableQuery);
        Assert.IsEmpty(q);
    }

    [Test]
    public void DatastoreQuery()
    {
        var schema = new Dictionary<string, Types.Type>()
        {
            { "directorID", Types.Type.Number },
        };

        var dirSchema = new Dictionary<string, Types.Type>()
        {
            { "name", Types.Type.String }
        };

        _ds.RegisterTable("TableId<:Movies>", "Movies", schema);
        var movie1 = ClientUtils.CreateProperty(("directorID", 1), ("title", "Avatar"));
        var movie2 = ClientUtils.CreateProperty(("directorID", 1), ("title", "Titanic"));
        var movie3 = ClientUtils.CreateProperty(("directorID", 2), ("title", "Jaws"));

        _ds.RegisterTable("TableId<:Director>", "Director", dirSchema);

        _ds.SetValue("TableId<:Movies>", "123", movie1);
        _ds.SetValue("TableId<:Movies>", "456", movie2);
        _ds.SetValue("TableId<:Movies>", "789", movie3);

        var dir1 = ClientUtils.CreateProperty(("name", "James Cameron"));
        var dir2 = ClientUtils.CreateProperty(("name", "Steven Spielberg"));

        _ds.SetValue("TableId<:Director>", "1", dir1);
        _ds.SetValue("TableId<:Director>", "2", dir2);

        // test: find the name of the director of the movie with the title "Jaws"

        var queryWhere2 = new Query().Find("?directorName")
            .Where("TableId<:Movies>", "?movieId", "title", "Jaws")
            .Where("TableId<:Movies>", "?movieId", "directorID", "?directorId")
            .Where("TableId<:Director>", "?directorId", "name", "?directorName");
        // var result = _ds.Query(queryWhere2);
        using var result = _ds.Query(queryWhere2).GetEnumerator();
        Assert.IsTrue(result.MoveNext());
        var firstResult = result.Current;
        Assert.AreEqual("Steven Spielberg", firstResult["directorName"]);
    }
    
    [Test]
    public void Benchmark()
    {
        var random = new System.Random();
        var stopwatch = new Stopwatch();
        
        var randomWriteData = new List<string>(1000);
        for (var i = 0; i < 1000; i++)
        {
            randomWriteData.Add(random.Next().ToString());
        }
        
        var randomReadData = new List<string>(10000);
        for (var i = 0; i < 10000; i++)
        {
            randomReadData.Add(random.Next().ToString());
        }

        // Perform 1k write operations
        stopwatch.Start();
        for (var i = 0; i < 1000; i++)
        {
            var position = ClientUtils.CreateProperty(("x", randomWriteData[i]), ("y", randomWriteData[i]));
            _ds.SetValue("TableId<:Position>", $"Key{i}", position);
        }

        stopwatch.Stop();
        var writeTime = stopwatch.ElapsedMilliseconds;
        Debug.Log($"Time for 1k writes: {stopwatch.ElapsedMilliseconds} ms");
        Debug.Log($"Average time per write op: {writeTime / 1000.0} ms");

        
        stopwatch.Restart();
        for (var i = 0; i < 10000; i++)
        {
            var keyToRead = randomReadData[i];
            var tableQuery = new Query().Find("?x", "?y")
                .Where("TableId<:Position>", $"Key{keyToRead}", "x", "?x")
                .Where("TableId<:Position>", $"Key{keyToRead}", "y", "?y");
            var q = _ds.Query(tableQuery);
            // Iterate over the query results to actually execute the query
            foreach (var result in q)
            {
            }
        }
        
        stopwatch.Stop();
        var readTime = stopwatch.ElapsedMilliseconds;
        Debug.Log($"Time for 10k reads: {stopwatch.ElapsedMilliseconds} ms");
        Debug.Log($"Average time per read op: {readTime / 10000.0} ms");

        stopwatch.Restart();
        
        // 1k delete ops
        for (var i = 0; i < 1000; i++)
        {
            var keyToDelete = randomWriteData[i];
            _ds.DeleteValue("TableId<:Position>", $"Key{keyToDelete}");
        }
        stopwatch.Stop();
        var deleteTime = stopwatch.ElapsedMilliseconds;
        Debug.Log($"Time for 1k deletes: {stopwatch.ElapsedMilliseconds} ms");
        Debug.Log($"Average time per delete op: {deleteTime / 1000.0} ms");
    }
    
   
}
