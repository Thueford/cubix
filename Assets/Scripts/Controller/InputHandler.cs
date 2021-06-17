using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InputHandler : MonoBehaviour
{
    public static bool enableSpace = true;
    public static bool enableNumbers = true;
    public static bool enableMovement = true;
    public static bool enableMouse = true;

    private static Vector3Int lastActivatedColor = new Vector3Int(0, 0, 0);

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {

    }

    public static void ReadSpaceInput()
    {
        if (Input.GetKeyDown(KeyCode.Space) && enableSpace)
        {
            Ressource.activateColors();
        }
    }

    public static Vector3Int ReadColorInput(Vector3Int rgb)
    {
        if (!enableNumbers || GameState.unlockedColors == Vector3Int.zero) return rgb;
        Vector3Int oldrgb = rgb;

        if (Input.GetKeyDown(KeyCode.Alpha1) && GameState.unlockedColors.x == 1)
        {
            rgb.x = 1 - rgb.x;
            if (rgb.x == 1) rgb = Vector3Int.right;
        }
        if (Input.GetKeyDown(KeyCode.Alpha2) && GameState.unlockedColors.y == 1)
        {
            rgb.y = 1 - rgb.y;
            if (rgb.y == 1) rgb = Vector3Int.up;
        }
        if (Input.GetKeyDown(KeyCode.Alpha3) && GameState.unlockedColors.z == 1)
        {
            rgb.z = 1 - rgb.z;
            if (rgb.z == 1) rgb = Vector3Int.forward;
        }
        if (rgb != oldrgb)
        {
            Player.self.bs.updateProperties(rgb);
        }
        return rgb;
    }

    public static Vector3 ReadDirInput()
    {
        if (!enableMovement) return Vector3.zero;
        Vector3 dir = Vector3.zero;
        if (Input.GetKey(KeyCode.DownArrow) || Input.GetKey(KeyCode.S)) dir.z -= 1;
        if (Input.GetKey(KeyCode.RightArrow) || Input.GetKey(KeyCode.D)) dir.x += 1;
        if (Input.GetKey(KeyCode.UpArrow) || Input.GetKey(KeyCode.W)) dir.z += 1;
        if (Input.GetKey(KeyCode.LeftArrow) || Input.GetKey(KeyCode.A)) dir.x -= 1;
        return dir;
    }

    public static  void ReadShootInput()
    {
        if (Input.GetMouseButton(0) && enableMouse)
        {
            Player.self.bs.tryShot();
        }
    }
}
