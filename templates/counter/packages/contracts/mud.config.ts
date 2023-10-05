import { mudConfig, resolveTableId } from "@latticexyz/world/register";

export default mudConfig({
  overrideSystems: {
    IncrementSystem: {
      name: "increment",
      openAccess: true,
    },
  },

  //create enum types here
  enums: {
    StateType: ["Normal", "Happy", "Sad", "Count"],
  },

  tables: {
    //used in the counter example scene
    Counter: {
      valueSchema: {
        value: "uint32",
      },
      storeArgument: true,
    },

    //used in test scenes
    Tester: "int32",
    State: "StateType",
    //universal ball counter
    Balls: {
      keySchema: {},
      valueSchema: { count: "int32" },
    },

    Position: {
      valueSchema: {
        x: "int32",
        y: "int32",
        hasWaited: "bool",
      },
    },

    AllTypes: {
      dataStruct: false,
      valueSchema: {
        boolTest: "bool",
        int32Test: "int32",
        uint32Test: "uint32",
        // bigIntTest: "int256",
        bigUintTest: "uint256",
        enumTest: "StateType",
        entityTest: "bytes32",
        addressTest: "address",
        // staticArrayTest: "int256[2]",
        // dynamicArrayTest: "uint256[]",
        // emptyArrayTest: "bool[]",
      },
    },
  },
  modules: [
    {
      name: "KeysWithValueModule",
      root: true,
      args: [resolveTableId("Counter")],
    },
  ],
});
