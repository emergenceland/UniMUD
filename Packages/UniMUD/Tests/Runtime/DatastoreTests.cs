using System.Collections.Generic;
using mud.Client;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;
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
        var q = _ds.Query(tableQuery);

        Assert.IsNotEmpty(q);
        Assert.AreEqual(1, q[0]["x"]);
        Assert.AreEqual(2, q[0]["y"]);
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
        var result = _ds.Query(queryWhere2);
        Assert.AreEqual("Steven Spielberg", result[0]["directorName"]);
    }
}

