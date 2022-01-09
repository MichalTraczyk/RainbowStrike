using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
public class SpawnManager : MonoBehaviour
{
    public static SpawnManager Instance;
    private PhotonView PV;
    public Transform[] redTeamSpawnpoints;
    public Transform[] blueeamSpawnpoints;

    int ctIndex;
    int ttIndex;
    private void Awake()
    {
        PV = GetComponent<PhotonView>();
        if (Instance == null)
            Instance = this;
        else
            Destroy(this.gameObject);
    }

    /*
    public Vector3 GetSpawnpoint(Team t,int index)
    {
        //PV.RPC("RPC_RemoveSpawnpointFromList", RpcTarget.All, t);
        if (t == GameManager.Instance.currentTerroTeam)
        {
            Debug.Log("returning: " + redTeamSpawnpoints[index] + "from index: " + index + "on position " + redTeamSpawnpoints[index].position);
            return redTeamSpawnpoints[ttIndex].position;
        }
        else
        {
            Debug.Log("returning: " + blueeamSpawnpoints[index] + "from index: " + index + "on position " + blueeamSpawnpoints[index].position);
            return blueeamSpawnpoints[ctIndex].position;
        }
    }*/
    [PunRPC]
    void RPC_RemoveSpawnpointFromList(Team t)
    {
        if (t == GameManager.Instance.currentTerroTeam)
        {
            ttIndex++;
        }
        else
        {
            ctIndex++;
        }
    }
    public int GetSpawnIndex(Team t)
    {
        int ret = 0;
        if(t==Team.Red)
        {
            ret= ttIndex;
        }
        else
        {
            ret= ctIndex;
        }

        PV.RPC("RPC_RemoveSpawnpointFromList", RpcTarget.All, t);
        return ret;
    }
}
