using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DestructiblePart : MonoBehaviour
{
    public void Hit(Vector3 pos, float range, float force, bool hard = false)
    {
        GetComponentInParent<Destructible>().HitWall(pos, range, force,hard);
    }
}