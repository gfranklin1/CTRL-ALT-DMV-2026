using UnityEngine;

public class BodyguardSpawnPoint : MonoBehaviour
{
    [Header("Bribe Config")]
    public int   bribeCost            = 200;
    [Range(0f, 1f)]
    public float bribeSuccessChance   = 0.5f;
    public float bribeDisableDuration = 15f;
}
