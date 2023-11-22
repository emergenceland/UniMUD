// SPDX-License-Identifier: MIT
pragma solidity >=0.8.0;
import { IWorld } from "../codegen/world/IWorld.sol";
import { System } from "@latticexyz/world/src/System.sol";
import { IStore } from "@latticexyz/store/src/IStore.sol";
import { Helpers } from "../Helpers.sol";

import { Position, PositionTableId, Health, Player, Damage } from "../codegen/index.sol";

contract MoveSystem is System {
  function move(int32 x, int32 y) public {
    // Get player key
    bytes32 player = Helpers.addressToEntityKey(address(_msgSender()));
    require(Health.get(player) > 0, "target is dead");

    // Get world
    IWorld world = IWorld(_world());

    bytes32[] memory atPosition = Helpers.getKeysWithPosition(world, x, y);
    require(atPosition.length == 0, "position occupied");

    Position.set(player, x, y);
  }

  function spawn(int32 x, int32 y) public {
    bytes32 player = Helpers.addressToEntityKey(address(_msgSender()));
    require(!Player.get(player), "already spawned");

    Player.set(player, true);
    Position.set(player, x, y);
    Health.set(player, 100);
    Damage.set(player, 40);
  }
}
