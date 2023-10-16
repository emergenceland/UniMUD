import { mudConfig, resolveTableId } from "@latticexyz/world/register";

export default mudConfig({
  tables: {
    Counter: {
      keySchema: {},
      valueSchema: "uint32",
    },
    Test: "bool",
  },
  modules: [
    {
      name: "UniqueEntityModule",
      root: true,
    },
    {
      name: "KeysWithValueModule",
      root: true,
      args: [resolveTableId("Counter")],
    },
  ],
});
