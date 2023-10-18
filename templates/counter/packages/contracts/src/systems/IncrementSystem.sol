// SPDX-License-Identifier: MIT
pragma solidity >=0.8.21;

import { System } from "@latticexyz/world/src/System.sol";
import { Counter, Position, PositionData, MyTableWithTwoKeys } from "../codegen/index.sol";

contract IncrementSystem is System {
  function increment() public returns (uint32) {
    uint32 counter = Counter.get();
    uint32 newValue = counter + 1;
    Counter.set(newValue);
    return newValue;
  }

   function move(int32 x, int32 y) public {
    bytes32 entityId = bytes32(uint256(uint160((_msgSender()))));

    PositionData memory position = Position.get(entityId);
    PositionData memory newPosition = PositionData(x, y);

    Position.set(entityId, newPosition);
  }
}
