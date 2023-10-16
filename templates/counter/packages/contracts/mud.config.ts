import { mudConfig, resolveTableId } from "@latticexyz/world/register";

export default mudConfig({
  systems: {},

  enums: {
    StateType: ["None", "One", "Two", "Three"],
  },

  tables: {

    Counter: {
      keySchema: {},
      valueSchema: "uint32",
    },
  

    Tester: "int32",    
    Balls: {
      keySchema: {},
      valueSchema: {count: "int32",},
    },

    Position: {
      valueSchema: {
        x: "int32",
        y: "int32",
        hasWaited: "bool",
      },
    },

    //commented out types are TODO
    AllTypes: {
      dataStruct: false,
      valueSchema: {
        boolTest: "bool",
        int32Test: "int32",
        uint32Test: "uint32",
        // bigIntTest: "int256",
        bigUintTest: "uint256",
        // enumTest: "StateType",
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
      name: "UniqueEntityModule",
      root: true,
    },
    // {
    //   name: "KeysWithValueModule",
    //   root: true,
    //   // args: [resolveTableId("Counter")],
    // },
  ],
});
