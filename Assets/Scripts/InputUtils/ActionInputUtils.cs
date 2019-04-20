using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ActionInputUtils
{
    private static bool actionInputEnabled = true;

    public static void SetActionInputEnable(bool enable)
    {
        actionInputEnabled = enable;
    }

    public static bool GetAttackButton()
    {
        return actionInputEnabled? Input.GetMouseButtonDown(0) : false;
    }

    public static bool GetJumpButtonDown()
    {
        return actionInputEnabled? Input.GetButtonDown("Jump") : false;
    }

    public static bool GetJumpButtonUp()
    {
        return actionInputEnabled? Input.GetButtonUp("Jump") : false;
    }

    public static float GetHorizontalInput()
    {

        return actionInputEnabled? Input.GetAxis ("Horizontal") : 0;
    }

    public static float GetVerticalInput()
    {

        return actionInputEnabled? Input.GetAxis ("Vertical") : 0;
    }
}
