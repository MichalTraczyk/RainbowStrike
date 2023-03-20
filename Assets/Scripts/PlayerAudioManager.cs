using Photon.Pun;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
public enum SoundType
{
    Gun,
    Other,
    Footsteps
}
public class PlayerAudioManager : MonoBehaviour
{
    public static PlayerAudioManager Instance;
    public FootstepsArea currFootstepsArea;

    [SerializeField] AudioSource oneshotSource;

    float t;

    public float movePause = 0.15f;
    public float runPause = 0.1f;
    
    PlayerMove move;
    PhotonView PV;
    private void Awake()
    {
        move = GetComponent<PlayerMove>();
        PV = GetComponent<PhotonView>();
        if(PV.IsMine)
        {
            if (Instance == null)
                Instance = this;
        }
    }

    public void FireSound(AudioClip clip)
    {
        
        oneshotSource.pitch = Random.Range(0.9f, 1.1f);
        oneshotSource.PlayOneShot(clip, GlobalSoundManager.Instance.gunVolume);
    }
    public void PlayOtherSound(AudioClip clip,SoundType type = SoundType.Other)
    {
        float volume;
        switch (type)
        {
            case SoundType.Gun:
                volume = GlobalSoundManager.Instance.gunVolume;
                break;
            case SoundType.Other:
                volume = GlobalSoundManager.Instance.soundFXVolume;
                break;
            case SoundType.Footsteps:
                volume = GlobalSoundManager.Instance.footstepsVolume;
                break;
            default:
                volume = 0.5f;
                break;
        }

        Debug.Log(oneshotSource.gameObject.name);
        oneshotSource.pitch = 1;
        oneshotSource.PlayOneShot(clip,volume);
    }

    private void Update()
    {
        if (!move.isWalking())
            return;

        t += Time.deltaTime;

        if(move.currentMoveState == MoveState.Running && t> runPause)
        {
            PV.RPC("RPC_PlayRunstep", RpcTarget.All);
            t = 0;
        }
        else if (move.currentMoveState == MoveState.Walking && t > movePause)
        {
            PV.RPC("RPC_PlayFootstep", RpcTarget.All);
            t = 0;
        }
    }

    [PunRPC]
    void RPC_PlayFootstep()
    {
        int r = Random.Range(0, currFootstepsArea.walkingClips.Length);
        PlayOtherSound(currFootstepsArea.walkingClips[r],SoundType.Footsteps);
    }
    [PunRPC]
    void RPC_PlayRunstep()
    {
        int r = Random.Range(0, currFootstepsArea.runningClips.Length);
        PlayOtherSound(currFootstepsArea.runningClips[r], SoundType.Footsteps);
    }

}
