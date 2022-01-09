using UnityEngine;



[CreateAssetMenu(fileName = "FootstepArea", menuName = "ScriptableObjects/FootstepArea", order = 1)]
public class FootstepsArea : ScriptableObject
{
    public AudioClip[] walkingClips;
    public AudioClip[] runningClips;

    public AudioClip[] landingClips;
}