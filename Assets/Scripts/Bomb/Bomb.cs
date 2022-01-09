using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Cinemachine;
using Photon.Pun;
using Photon.Realtime;
public class Bomb : MonoBehaviour
{
    //NetworkVariableBool isCurrlentyDefusing = new NetworkVariableBool(new NetworkVariableSettings { WritePermission = NetworkVariablePermission.Everyone }, false);
    bool isCurrlentyDefusing = false;
    float t;
    bool defused = false;
    bool exploded = false;
    // Start is called before the first frame update
    public float bombBumTime = 40;
    float startBumTime;
    public float defuseTime = 7;

    public Slider s;
    public GameObject explosionParticles;
    public CinemachineImpulseSource impulse;

    [Header("Audio")]
    [SerializeField] AudioSource source;

    [SerializeField] AudioClip TickSound;
    [SerializeField] AudioClip bumSound;
    [SerializeField] AudioClip defuseSound;
    [SerializeField] AudioClip plantSound;

    [SerializeField] float baseTickOffset;
    PhotonView PV;
    private float tickTimer;

    private void Start()
    {
        GlobalSoundManager.Instance.PlayGlobalSound(plantSound);


        startBumTime = bombBumTime;
        isCurrlentyDefusing = false;
        s.gameObject.SetActive(false);
        s.maxValue = defuseTime;
        PV = GetComponent<PhotonView>();
        Setup();

        if(PV.IsMine)
        {
            GameManager.Instance.OnBombPlanted();
        }

    }
    void Setup()
    {
        t = defuseTime;
        Team team = PlayerManager.Instance.localPlayerTeam;

        Team terro = GameManager.Instance.currentTerroTeam;

        if (team == terro)
        {
            Destroy(GetComponent<Interactable>());
        }
    }
    void Update()
    {
        if (defused)
            return;
        if (isCurrlentyDefusing)
        {
            t -= Time.deltaTime;
            s.value = t;
            if (t < 0)
                Defuse();
        }


        bombBumTime -= Time.deltaTime;

        tickTimer += Time.deltaTime;
        if (tickTimer > Mathf.Clamp(baseTickOffset * bombBumTime / startBumTime, 0.1f, 2) && bombBumTime > 0)
        {
            source.PlayOneShot(TickSound, GlobalSoundManager.Instance.soundFXVolume);
            tickTimer = 0;
        }


        if (!PhotonNetwork.IsMasterClient)
            return;
        if (bombBumTime < 0)
        {
            if (exploded)
                return;
            exploded = true;
            DamageOnExplosion();
            PV.RPC("RPC_Bum",RpcTarget.All);
        }

    }
    void Bum()
    {
        GameManager.Instance.OnBombBum();
        PhotonNetwork.Destroy(this.gameObject);
    }
    public void DestroyThis()
    {
        PV.RPC("RPC_DestroyThis", RpcTarget.All);
    }
    [PunRPC]
    void RPC_DestroyThis()
    {
        if (PV.IsMine)
        {
            PhotonNetwork.Destroy(this.gameObject);
        }
    }
    void DamageOnExplosion()
    {
        PlayerHp[] alivePlayers = FindObjectsOfType<PlayerHp>();
        foreach (PlayerHp p in alivePlayers)
        {
            float distance = Vector3.Distance(p.transform.position, transform.position);
            float damage = 150 - 5 * distance;
            damage = Mathf.Clamp(damage, 0, 200);
            //p.BombDamage(Mathf.RoundToInt(damage), transform.position);
            p.TakeDamage(Mathf.RoundToInt(damage), PV.Owner, "Bomb", transform.position);
        }
    }
    [PunRPC]
    void RPC_Bum()
    {
        Instantiate(explosionParticles, transform.position, Quaternion.identity);
        AudioSource.PlayClipAtPoint(bumSound, transform.position, GlobalSoundManager.Instance.soundFXVolume);
        impulse.GenerateImpulse();
        if(PV.IsMine)
        {
            Bum();
        }
    }

    public void AskForDefuse()
    {
        if (defused)
            return;
        if (isCurrlentyDefusing == false)
        {
            StartDefusing();
        }
    }
    public void AskForStopDefuse()
    {
        if (isCurrlentyDefusing == true)
        {
            StopDefusing();
            isCurrlentyDefusing = false;
        }
    }

    public void StartDefusing()
    {
        s.gameObject.SetActive(true);
        isCurrlentyDefusing = true;
    }
    void StopDefusing()
    {
        s.gameObject.SetActive(false);
        isCurrlentyDefusing = false;
        t = defuseTime;
    }


    void Defuse()
    {
        if (defused)
            return;
        defused = true;
        s.gameObject.SetActive(false);
        GameManager.Instance.OnBombDefused();
        PV.RPC("RPC_OnDefuse",RpcTarget.All);
    }

    [PunRPC]
    void RPC_OnDefuse()
    {
        defused = true;
        Interactable i = GetComponent<Interactable>();
        if(i!=null)
            Destroy(GetComponent<Interactable>());

        GlobalSoundManager.Instance.PlayGlobalSound(defuseSound);
        //Destroy(GetComponent<Bomb>());
    }
}
