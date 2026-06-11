using System.Collections.Generic;
using UnityEngine;

public static class PizzaState
{
    public static bool sauceAdded = false;
    public static bool cheeseAdded = false;

    public static int pepperoniCount = 0;
    public static List<Vector3> pepperoniLocalPositions = new List<Vector3>();
    public static List<Quaternion> pepperoniLocalRotations = new List<Quaternion>();

    public static void ResetPizza()
    {
        sauceAdded = false;
        cheeseAdded = false;
        pepperoniCount = 0;
        pepperoniLocalPositions.Clear();
        pepperoniLocalRotations.Clear();
    }
}
