# UniMUD

Low-level networking utilities for interacting with the [MUD 2.0.0-next.9](https://mud.dev) framework in [Unity](https://unity3d.com).

⚠️ If your goal is to just make a game, **you should not use UniMUD directly.**
Instead, use [MUD Template Unity](https://github.com/emergenceland/mud-template-unity).

## Getting Started

### Installation

Add UniMUD to the Unity Package Manager via git url

```
https://github.com/emergenceland/unimud.git?path=Packages/UniMUD
```

### Setup

**1. Add a NetworkManager:**

UniMUD looks for a NetworkManager instance in the scene. You should create an empty game object and attach the **NetworkManager** component (included with UniMUD) to it. The NetworkManager component has a few properties that you can must set:

- JSON RPC URL
- Websocket URL
- ChainID
- Contract Address (this can be auto-populated in a template)

As well as an optional property, `Private Key (pk)`:

**2. Generate Nethereum bindings:**

You also need Nethereum bindings for your generated World contract. Bindings can be autogenerated for you in some templates, but you can also use any of these tools: [Nethereum Code Generation](https://docs.nethereum.com/en/latest/nethereum-code-generation/)

## Example Usage

### Making transactions

```csharp
using IWorld.ContractDefinition;
using mud;

async void Move(int x, int y)
{
	// The MoveFunction type comes from your autogenerated bindings
	// NetworkManager exposes a worldSend property that you can use to send transactions.
	// It takes care of gas and nonce management for you.
	// Make sure your MonoBehaviour is set up to handle async/await.
  await NetworkManager.Instance.world.Write<MoveFunction>(x, y);
}
```

### Representing State

UniMUD caches MUD v2 events in the client for you in a "datastore." You can access the datastore via the NetworkManager instance. The datastore keeps a multilevel index of tableId -> table -> records

```csharp
class RxRecord {
  public string tableId;
  public string key;
  public Dictionary<string, object> value;
}
```

For example, records for an entity's Position might look like:

```json
[
  {
    "tableId": "Position",
    "key": "0x1234",
    "value": {
      "x": 1,
      "y": 2
    }
  },
  {
    "tableId": "Position",
    "key": "0x5678",
    "value": {
      "x": 3,
      "y": 4
    }
  }
]
```

### Fetching a value for a key

To fetch a record by key, use `GetValue` on the datastore:

```csharp
RxRecord? GetMonstersWithKey(string monsterKey) {
	RxDatastore ds = NetworkManager.Instance.ds;
	RxTable monstersTable = ds.tableNameIndex["Monsters"];

	return ds.GetValue(monstersTable, monsterKey);
}
```

### Setting a value

Use the `Set` method on the datastore:

```csharp
void SetMonsterName(string monsterKey, string name) {
  var ds = NetworkManager.Instance.ds;
	RxTable monstersTable = ds.tableNameIndex["Monsters"];

  ds.Set(monstersTable, monsterKey, new Dictionary<string, object> {
    { "name", name }
  });
}
```

### Queries

For queries that are useful in an ECS context, you can use the `Query` class to build queries.

**Get all records of entities that have Component X and Component Y**

```csharp
RxTable Health = ds.tableNameIndex["Health"];
RxTable Position ds.tableNameIndex["Position"];

var hasHealthAndPosition = new Query().In(Health).In(Position)

// -> [ { table: "Position", key: "0x1234", value: { x: 1, y: 2 } },
//      { table: "Health", key: "0x1234", value: { health: 100 } },
//      { table: "Position", key: "0x456", value: {x: 2: y: 3} }, ...]
```

**Get all records of entities that have Component X and Component Y, but not Component Z**

```csharp
var notMonsters = new Query().In(Health).In(Position).Not(Monster)
```

**Get all records of entities that have Component X and Component Y, but only return rows from Component X**

```csharp
var allHealthRows = new Query().Select(HealthTable).In(Position).In(HealthTable)
// -> [ { table: "Health", key: "0x1234", value: { health: 100 } } ]
```

**Get all monsters that have the name Chuck**

```csharp
var allMonstersNamedChuck = new Query().In(MonsterTable).In(MonsterTable, new Condition[]{Condition.Has("name", "Chuck")})
// -> [ { table: "Monsters", key: "0x1234", value: { name: "Chuck", strength: 100 } } ]
```

Make sure you actually run the query after building it, with `NetworkManager.Instance.ds.RunQuery(yourQuery)`

```csharp
using mud;

void RenderHealth() {
  var hasHealth = new Query().Select(Health).In(InitialHealth).In(Health).In(TilePosition);

  var recordsWithHealth = NetworkManager.Instance.ds.RunQuery(hasHealth); // don't forget

  foreach (var record in recordsWithHealth) {
    DrawHealthBar(record.value["healthValue"]);
    // assumes the health table has an attribute called "healthValue"
  }
}
```

### Reacting to Updates

You can do reactive queries on the datastore, with the `RxQuery(yourQuery)` method.

```csharp
using System;
using UniRx;
using mud;
using mud;
using UnityEngine;

public class Health : MonoBehaviour
{
    private IDisposable _disposable = new();

    private int m_CurrentHealth = 0;

    private void Awake()
    {
      // get the health table value from all entities that have health, initialHealth, and a position.
      RxTable Health = ds.tableNameIndex["Health"];
      // ... other tables here
      var healthValues = new Query().Select(Health).In(InitialHealth).In(Health).In(TilePosition);

      _disposable = NetworkManager.Instance.ds
      	// RxQuery returns a tuple of 2 lists: (added, removed)
        .RxQuery(healthValues).ObserveOnMainThread()
        .Subscribe(OnHealthChange);
    }

    private void OnHealthChange((List<Record> SetRecords, List<Record> RemovedRecords) update)
    {
      var setValues = update.Item1;
      var removedvalues = update.Item2;

      // Values are Dictionaries mapping string attributes to their values.
      // For example, if your mud config defines Health as:
      //  Health: {
      //    schema: {
      //      myHealthValue: "uint32",
      //      },
      //    }
      // The value will be a Dictionary<string, object> with a single key named "myHealthValue".

       foreach (var record in setValues)
       {
            var currentValue = record.value;
            if (currentValue == null) return;

            m_CurrentHealth = Convert.ToSingle(currentValue["myHealthValue"]);
            SetHealthUI();
      }

      foreach (var record in removedValues)
      {
        // if health has been removed, then the entity has died.
        var entityKey = record.key;
        KillEntity(entityKey);
      }
  }

  private void OnDestroy()
  {
      _disposable?.Dispose();
  }
}
```

### Deploying to a testnet

Change the RPC and ChainID in **NetworkManager**.
UniMUD currently does not implement a faucet service, so you must manually send funds to your address when deploying to a non-local chain (i.e. not Anvil).

## Future work

- Indexer
- Unity DOTS storage
- Caching/persistence
- Faucet

## License

MIT
