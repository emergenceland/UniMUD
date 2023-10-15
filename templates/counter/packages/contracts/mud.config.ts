import { mudConfig, resolveTableId } from "@latticexyz/world/register";

export default mudConfig({
  systems: {},

  enums: {
    ActionType: [
      "None",
      "Idle",
      "Dead",
      "Mining",
      "Shoveling",
      "Stick",
      "Fishing",
      "Walking",
      "Buy",
      "Plant",
      "Push",
      "Chop",
      "Teleport",
      "Melee",
      "Hop",
      "Spawn",
      "Bow",
    ],
    TerrainType: ["None", "Rock", "Mine", "Tree", "HeavyBoy", "HeavyHeavyBoy", "Pillar", "Road", "Hole", "Miliarium"],
    NPCType: ["None", "Player", "Soldier", "Barbarian", "Ox", "BarbarianArcher"],
    RoadState: ["None", "Shoveled", "Statumen", "Rudus", "Nucleas", "Paved", "Bones"],
    RockType: [
      "None",
      "Raw",
      "Statumen",
      "Pavimentum",
      "Rudus",
      "Nucleus",
      "Miliarium",
      "Heavy",
      "HeavyHeavy",
      "Pillar",
    ],
    MoveType: ["None", "Obstruction", "Hole", "Carry", "Push", "Trap"],
    FloraType: ["None", "Tree", "Oak", "Bramble"],
    PuzzleType: ["None", "Miliarium", "Bearer", "Count"],
    PaymentType: ["None", "Coins", "Gems", "Eth"],
  },

  tables: {
    Counter: {
      keySchema: {},
      valueSchema: "uint32",
    },
    GameConfig: {
      keySchema: {},
      valueSchema: {
        debug: "bool",
        dummyPlayers: "bool",
        roadComplete: "bool",
      },
    },

    Stats: {
      dataStruct: false,
      valueSchema: {
        startingMile: "int32",
      },
    },

    Name: {
      dataStruct: false,
      valueSchema: {
        named: "bool",
        first: "uint32",
        middle: "uint32",
        last: "uint32",
      },
    },

    //map
    MapConfig: {
      //empty keySchema creates a singleton
      keySchema: {},
      dataStruct: false,
      valueSchema: {
        playWidth: "int32",
        playHeight: "int32",
        playSpawnWidth: "int32",
      },
    },

    RoadConfig: {
      //empty keySchema creates a singleton
      keySchema: {},
      dataStruct: false,
      valueSchema: {
        width: "uint32",
        left: "int32",
        right: "int32",
      },
    },

    Bounds: {
      keySchema: {},
      dataStruct: false,
      valueSchema: {
        left: "int32",
        right: "int32",
        up: "int32",
        down: "int32",
      },
    },

    GameState: {
      keySchema: {},
      dataStruct: false,
      valueSchema: {
        miles: "int32",
        unused: "int32",
      },
    },

    Chunk: {
      dataStruct: false,
      valueSchema: {
        mile: "int32",
        spawned: "bool",
        completed: "bool",
        roads: "uint32",
        blockCompleted: "uint256",

        // // dynamic list of people who have helped build the mile
        // entities: "bytes32[]",
        //   //dynamic list of people who have helped build the mile
        // contributors: "bytes32[]",
      },
    },

    Entities: {
      dataStruct: false,
      valueSchema: {
        width: "bytes32[]",
        height: "bytes32[]",
      },
    },

    //units
    NPC: "uint32",
    Soldier: "bool",
    Barbarian: "bool",
    Archer: "uint32",
    Seeker: "uint32",
    Aggro: "uint32",

    //items
    Shovel: "bool",
    Pickaxe: "bool",
    Bones: "bool",
    Stick: "bool",
    Robe: "int32",
    Head: "int32",
    Scroll: "uint32",
    Coinage: "int32",
    Weight: "int32",

    //puzzle components try to be moved onto triggers (ie. Miliarli )
    Puzzle: { dataStruct: false, valueSchema: { puzzleType: "uint32", complete: "bool" } },
    Trigger: "bytes32",
    Miliarium: "bool",

    Position: {
      valueSchema: {
        x: "int32",
        y: "int32",
        layer: "int32",
      },
    },

    //player state
    Active: "bool",
    Player: "bool",
    Move: "uint32",
    Carrying: "bytes32",
    FishingRod: "bool",
    Boots: { valueSchema: { minMove: "int32", maxMove: "int32" } },

    //properties
    Damage: "int32",
    Health: "int32",
    Seeds: "uint32",
    Gem: "int32",
    Eth: "uint256",

    //unique objects
    Rock: "uint32",
    Boulder: "bool",
    Tree: "uint32",
    Log: "bool",
    Ox: "bool",
    Conscription: "bool",
    XP: "uint256",

    //Behaviour
    //Flee: "bool", (this will probably cause infinite loops) if a seeker chases a fleer

    Road: {
      dataStruct: false,
      valueSchema: {
        state: "uint32",
        filled: "bytes32",
        gem: "bool",
      },
    },

    Carriage: "bool",
    Row: {
      keySchema: {},
      valueSchema: {
        value: "int32",
      },
    },

    // Item: {
    //   dataStruct: false,
    //   valueSchema: {
    //     name: "string",
    //     id: "uint32",
    //     equipped: "bool",
    //   },
    // },

    Action: {
      offchainOnly: true,
      dataStruct: false,
      valueSchema: {
        action: "uint32",
        x: "int32",
        y: "int32",
        target: "bytes32",
      },
    },
  },

  modules: [
    {
      name: "UniqueEntityModule",
      root: true,
    },
    {
      name: "KeysWithValueModule",
      root: true,
      args: [resolveTableId("Position")],
    },
  ],
});
