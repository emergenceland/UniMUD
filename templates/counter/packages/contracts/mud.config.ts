import { mudConfig, resolveTableId } from "@latticexyz/world/register";

export default mudConfig({
  systems: {},

  tables: {
    Counter: {
      keySchema: {},
      valueSchema: "uint32",
    },
    Position: {
      valueSchema: {
        x: "int32",
        y: "int32",
      },
    },
    MyTableWithTwoKeys: {
      valueSchema: {
        value1: "uint32",
        value2: "uint32",
      },
      keySchema: {
        key1: "bytes32",
        key2: "bytes32",
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
