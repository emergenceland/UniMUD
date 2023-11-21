// SPDX-License-Identifier: MIT
pragma solidity >=0.8.21;

import { IWorld } from "./codegen/world/IWorld.sol";
import { Position, PositionTableId } from "./codegen/index.sol";
import { PackedCounter } from "@latticexyz/store/src/PackedCounter.sol";
import { getKeysWithValue } from "@latticexyz/world-modules/src/modules/keyswithvalue/getKeysWithValue.sol";

library Helpers {

  function getKeysWithPosition(IWorld world, int32 x, int32 y) internal view returns (bytes32[] memory) {
    (bytes memory staticData, PackedCounter encodedLengths, bytes memory dynamicData) = Position.encode(x, y);
    return getKeysWithValue(world, PositionTableId, staticData, encodedLengths, dynamicData);
  }

  function addressToEntityKey(address addr) internal pure returns (bytes32) {
    return bytes32(uint256(uint160(addr)));
  }
}
