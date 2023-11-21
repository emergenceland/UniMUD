// SPDX-License-Identifier: MIT
pragma solidity >=0.8.0;
import { IWorld } from "../codegen/world/IWorld.sol";
import { System } from "@latticexyz/world/src/System.sol";
import { Helpers } from "../Helpers.sol";

import { Damage, Position, PositionTableId, Player, PositionData, Health } from "../codegen/index.sol";

contract AttackSystem is System {
  function attack(int32 x, int32 y) public {
    bytes32 player = Helpers.addressToEntityKey(address(_msgSender()));
    IWorld world = IWorld(_world());

    PositionData[] memory neighbors = mooreNeighborhood(PositionData(x, y));

    for (uint i = 0; i < neighbors.length; i++) {
      PositionData memory neighbor = neighbors[i];
      bytes32[] memory atPosition = Helpers.getKeysWithPosition(world, neighbor.x, neighbor.y);
      if (atPosition.length == 1) {
        attackTarget(player, atPosition);
      }
    }
  }

  function attackTarget(bytes32 player, bytes32[] memory atPosition) internal {
    bytes32 defender = atPosition[0];

    require(Player.get(defender), "target is not a player");
    require(Health.get(defender) > 0, "target is dead");

    int32 playerDamage = Damage.get(player);
    int32 defenderHealth = Health.get(defender);
    int32 newHealth = defenderHealth - playerDamage;
    if (newHealth <= 0) {
      Health.deleteRecord(defender);
      Position.deleteRecord(defender);
      Player.deleteRecord(defender);
    } else {
      Health.set(defender, newHealth);
    }
  }

  function mooreNeighborhood(PositionData memory center) internal pure returns (PositionData[] memory) {
    PositionData[] memory neighbors = new PositionData[](9);
    uint256 index = 0;

    for (int32 x = -1; x <= 1; x++) {
      for (int32 y = -1; y <= 1; y++) {
        neighbors[index] = PositionData(center.x + x, center.y + y);
        index++;
      }
    }

    return neighbors;
  }
}
