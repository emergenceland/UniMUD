import { mudConfig, resolveTableId } from "@latticexyz/world/register";

export default mudConfig({
  systems: {},

  tables: {
    Counter: {
      keySchema: {},
      valueSchema: "uint32",
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
