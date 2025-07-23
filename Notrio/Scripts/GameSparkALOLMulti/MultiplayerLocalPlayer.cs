using System;
using System.Collections;
using System.Collections.Generic;
using Takuzu;
using UnityEngine;

public class MultiplayerLocalPlayer : MonoBehaviour
{
    internal static MultiplayerLocalPlayer CreatePlayer(string localPlayerId, Transform container , Action<string, Index2D, int, int> onPlayerPuzzleValueSet, Action<string> onPlayerPuzzleSolved)
    {
        GameObject go = new GameObject();
        go.name = "player";
        go.transform.SetParent(container);
        MultiplayerLocalPlayer localPlayer = go.AddComponent<MultiplayerLocalPlayer>();
        localPlayer.id = localPlayerId;
        localPlayer.onValueSet = onPlayerPuzzleValueSet;
        localPlayer.onPuzzleSolved = onPlayerPuzzleSolved;
        return localPlayer;
    }

    private string id;
    private Action<string, Index2D, int, int> onValueSet = null;
    private Action<string> onPuzzleSolved = null;

    private void Start() {
        //Listen to change on the board and broadcast to other player
        LogicalBoard.onCellValueSet += OnLogicalBoardValueSet;
        //Send complete msg with time stamp
        LogicalBoard.onPuzzleSolved += OnLogicalBoardSolved;
    }
    private void OnDestroy()
    {
        //Listen to change on the board and broadcast to other player
        LogicalBoard.onCellValueSet -= OnLogicalBoardValueSet;
        //Send complete msg with time stamp
        LogicalBoard.onPuzzleSolved -= OnLogicalBoardSolved;
    }

    private void OnLogicalBoardSolved()
    {
        onPuzzleSolved(id);
    }

    private void OnLogicalBoardValueSet(Index2D index, int value)
    {
        onValueSet(id, index, value, SkinManager.Instance.currentActivatedSkinIndex);
    }
}