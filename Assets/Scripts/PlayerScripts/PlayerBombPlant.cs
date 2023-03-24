using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using System.IO;

public class PlayerBombPlant : MonoBehaviour
{
    float startTimeToPlant;
    float remainingTimeToPlant;
    ///public NetworkVariableBool hasBomb = new NetworkVariableBool(new NetworkVariableSettings { WritePermission = NetworkVariablePermission.Everyone }, false);
    public bool hasBomb { get; private set; }
    //public GameObject toTakeBomb;
    PlayerInteractHandle interact;
    PlayerUI ui;
    bool planting;
    bool isOnBombsite;
    PhotonView PV;


    public void Start()
    {
        PV = GetComponent<PhotonView>();
        ui = GetComponent<PlayerUI>();
        startTimeToPlant = GameManager.Instance.timeToPlant;
        remainingTimeToPlant = startTimeToPlant;
        interact = GetComponent<PlayerInteractHandle>();
        GetComponent<PlayerUI>().OnBombisteEnterExit(isOnBombsite);
        //PickupBomb();
    }
    void UpdateUIOnPickup( bool nev)
    {
        GetComponent<PlayerUI>().OnBombPickup(nev);
    }
    public void PickupBomb()
    {
        hasBomb = true;
        UpdateUIOnPickup(hasBomb);
    }
    public void EnterBombsite()
    {
        isOnBombsite = true;
        GetComponent<PlayerUI>().OnBombisteEnterExit(isOnBombsite);
    }
    public void ExitBombsite()
    {
        isOnBombsite = false;
        GetComponent<PlayerUI>().OnBombisteEnterExit(isOnBombsite);
    }
    public void DropBomb()
    {
        if (hasBomb)
        {
            string path = Path.Combine("PhotonPrefabs", "BombToTake");
            PhotonNetwork.Instantiate(path, transform.position + Vector3.up + transform.forward, Quaternion.identity);
            //GameObject obj = Instantiate(toTakeBomb, transform.position + Vector3.up + transform.forward, Quaternion.identity);
            hasBomb = false;
            UpdateUIOnPickup(hasBomb);
        }
    }
    public void NoBombHereOwo()
    {
        hasBomb = false;
        UpdateUIOnPickup(hasBomb);
    }

    private void Update()
    {
        if (!PV.IsMine)
            return;
        if (Input.GetKeyDown(KeyCode.G))
        {
            DropBomb();
        }
        if (interact.currInteract == null && isOnBombsite)
        {
            if (Input.GetKeyDown(KeyCode.Alpha4))
            {
                if (hasBomb)
                {
                    ui.StartPlanting(startTimeToPlant);
                    StartPlanting();
                }
            }
            if (Input.GetKeyUp(KeyCode.Alpha4))
            {
                if (planting)
                {
                    StopPlanting();
                    ui.StopPlanting();
                }
            }
        }

        if (planting)
        {
            remainingTimeToPlant -= Time.deltaTime;
            ui.SetPlantingTime(remainingTimeToPlant);
            if (remainingTimeToPlant <= 0)
            {
                ui.StopPlanting();
                PlantBomb();
                StopPlanting();
                planting = false;
            }
        }



    }
    void PlantBomb()
    {
        string path = Path.Combine("PhotonPrefabs","Bomb");
        PhotonNetwork.Instantiate(path, transform.position, Quaternion.identity);
        hasBomb = false;
        UpdateUIOnPickup(hasBomb);
    }

    void ChangePlayerState(bool s)
    {
        GetComponent<PlayerWeaponSwap>().enabled = s;
        GetComponent<Animator>().SetBool("Planting", !s);
    }

    void StartPlanting()
    {
        GetComponent<PlayerMove>().DisablePlayer();
        planting = true;

        ChangePlayerState(false);
    }

    void StopPlanting()
    {
        GetComponent<PlayerMove>().EnablePlayer();
        ChangePlayerState(true);

        remainingTimeToPlant = startTimeToPlant;
        planting = false;
    }
}
