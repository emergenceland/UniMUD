// SPDX-License-Identifier: MIT
pragma solidity >=0.8.0;
import { System } from "@latticexyz/world/src/System.sol";
import { IStore } from "@latticexyz/store/src/IStore.sol";
import { Counter, Tester, Position, PositionData, AllTypes, Balls } from "../codegen/index.sol";
import { StateType } from "../codegen/common.sol";
import { addressToEntityKey } from "../utility/addressToEntityKey.sol";

contract TestSystem is System {

  function startTest() public {

    bytes32 player = addressToEntityKey(address(_msgSender()));
    int256[2] memory staticTest;
    staticTest[0] = 3;
    staticTest[1] = 4;
    uint256[] memory dynamicTest = new uint256[](3);
    dynamicTest[0] = 5;
    dynamicTest[1] = 6;
    dynamicTest[2] = 7;
    bool[] memory emptyTest = new bool[](0);
    
    AllTypes.set(player, true, int32(-3), uint32(5), uint256(50), player, address(_msgSender()));

    for (uint i = 0; i < 2; i++) {
      spawnBall();
    }

    // AllTypes.set(player, true, int32(-3), uint32(5), int256(-40), uint256(50), StateType.Happy, player);
    // AllTypes.set(player, true, -3, 5, -40, 50, StateType.Happy, player, staticTest, dynamicTest, emptyTest);

  }

  
  function updateAllBalls() public {

    spawnBall();

    uint ballCount =  uint(uint32(Balls.get()));
    for(uint i = 0; i < ballCount; i++) {
      moveBall(keccak256(abi.encode(i)));
    }
  }


  function moveBall(bytes32 key) public {

    int32 y = Tester.get(key);
    PositionData memory pos = Position.get(key);

    if(pos.x == 2 && pos.hasWaited == false) {
      //test what happens if we set the same position
      Position.set(key, PositionData(pos.x, y, true));
    } else if(pos.x == 3) {
      //test record delete
      Position.deleteRecord(key);
    } else {
      //test normal update (and set when the record has been deleted)
      Position.set(key, PositionData(pos.x + 1, y, false));
    }

  }

  function spawnBall() public {

    int32 ballNumber = Balls.get();

    bytes32 key = keccak256(abi.encode(ballNumber));
    Tester.set(key, ballNumber);
    // State.set(key, StateType.Normal);
    Position.set(key, PositionData(0,ballNumber, false));

    Balls.set(ballNumber + 1);

  }

  // function setSimple(bytes32 ball) public {
  //   uint currentType = uint(State.get(ball));
  //   currentType = (currentType + 1) % uint(StateType.Count);
  //   State.set(ball, StateType(currentType));

  // }

  // function deleteSimple(bytes32 ball) public {
  //   State.deleteRecord(ball);
  // }

  // function setDelete(bytes32 ball) public {
  //   uint currentType = uint(State.get(ball));
  //   currentType = (currentType + 1) % uint(StateType.Count);
  //   State.set(ball, StateType(currentType));
  //   State.deleteRecord(ball);
  // }

  // function deleteSet(bytes32 ball) public {
  //   uint currentType = uint(State.get(ball));
  //   currentType = (currentType + 1) % uint(StateType.Count);
  //   State.deleteRecord(ball);
  //   State.set(ball, StateType(currentType));
  // }
}
