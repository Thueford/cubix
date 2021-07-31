using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InputHandler
{
    public static bool enableSpace = true;
    public static bool enableNumbers = true;
    public static bool enableMovement = true;
    public static bool enableMouse = true;
    private static int last = -1;

    public static Vector3Int ReadColorInput(Vector3Int old)
    {
        if (!enableNumbers || GameState.unlockedColors == Vector3Int.zero) return old;

        if (Input.GetKeyDown(KeyCode.Alpha1) && GameState.unlockedColors.x == 1)
        {
            old.x = 1 - old.x;
            if (old.x == 1) old = Vector3Int.right;
        }
        if (Input.GetKeyDown(KeyCode.Alpha2) && GameState.unlockedColors.y == 1)
        {
            old.y = 1 - old.y;
            if (old.y == 1) old = Vector3Int.up;
        }
        if (Input.GetKeyDown(KeyCode.Alpha3) && GameState.unlockedColors.z == 1)
        {
            old.z = 1 - old.z;
            if (old.z == 1) old = Vector3Int.forward;
        }
        return old;
    }

    public static Vector3 ReadDirInput()
    {
        if (!enableMovement || ReadCtl()) return Vector3.zero;
        Vector3 dir = Vector3.zero;
        if (Input.GetKey(KeyCode.DownArrow) || Input.GetKey(KeyCode.S)) dir.z -= 1;
        if (Input.GetKey(KeyCode.RightArrow) || Input.GetKey(KeyCode.D)) dir.x += 1;
        if (Input.GetKey(KeyCode.UpArrow) || Input.GetKey(KeyCode.W)) dir.z += 1;
        if (Input.GetKey(KeyCode.LeftArrow) || Input.GetKey(KeyCode.A)) dir.x -= 1;
        return dir;
    }

    public static bool ReadShootInput() => Input.GetMouseButton(0) && enableMouse;
    public static bool ReadSpaceInput() => Input.GetKeyDown(KeyCode.Space) && enableSpace;
    public static bool ReadPauseInput() => Input.GetKeyDown(KeyCode.Escape);

    public static void ResetLast() => last = -1;
    public static int GetLast() => last;
    private static bool SetLast(KeyCode k) => (last = (int) k) >= 0;

    public static bool ReadKeyDown(KeyCode k) => Input.GetKeyDown(k) && SetLast(k);
    public static bool ReadCombo(KeyCode prim, KeyCode sec, KeyCode tert) => Input.GetKey(prim) && ReadCombo(sec, tert);
    public static bool ReadCombo(KeyCode prim, KeyCode sec) => Input.GetKey(prim) && Input.GetKeyDown(sec);
    public static bool ReadCtlCombo(KeyCode sec) => ReadCtl() && Input.GetKeyDown(sec);
    public static bool ReadCtlCombo(KeyCode sec, KeyCode tert) => ReadCtl() && ReadCombo(sec, tert);
    public static bool ReadCtl() => Input.GetKey(KeyCode.X);
}
