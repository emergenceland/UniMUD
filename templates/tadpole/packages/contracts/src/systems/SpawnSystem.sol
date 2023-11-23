// SPDX-License-Identifier: MIT
pragma solidity >=0.8.0;

import { System } from "@latticexyz/world/src/System.sol";
import { IWorld } from "../codegen/world/IWorld.sol";
import { Player, Position, Toad, GameManager } from "../codegen/index.sol";

import { getUniqueEntity } from "@latticexyz/world-modules/src/modules/uniqueentity/getUniqueEntity.sol";
import { Utility } from "../utility/utility.sol";

contract SpawnSystem is System {
    function spawnPlayer() public {
        bytes32 entity = Utility.addressToEntityKey(address(_msgSender()));
        require(Player.get(entity) == false, "Already spawned");

        Player.set(entity, true);
    }

    function spawnToad(int32 x, int32 y, int32 z) public {
        bytes32 [] memory keys = Utility.getKeysAtPosition(IWorld(_world()), x, y, z);
        require(keys.length == 0, "Obstruction");

        bytes32 toad = getUniqueEntity();
        uint32 tadpoles = GameManager.get();

        Position.set(toad, x, y, z);
        Toad.set(toad, true);
        GameManager.set(tadpoles+1);
    }

    function deleteToad(int32 x, int32 y, int32 z) public {
        bytes32 [] memory keys = Utility.getKeysAtPosition(IWorld(_world()), x, y, z);
        require(keys.length > 0, "No toad");

        bytes32 toad = keys[0];
        uint32 tadpoles = GameManager.get();

        Position.deleteRecord(toad);
        Toad.deleteRecord(toad);
        GameManager.set(tadpoles-1);
    }
}
