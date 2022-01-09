using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;
[System.Serializable]
public class WeaponToTake
{
    public string name;
    public GameObject prefab;
}
[System.Serializable]
public class GrenadeIcon
{
    public string name;
    public Sprite icon;
}
public class WeaponManager : MonoBehaviour
{
    public WeaponToTake[] allWeapons;
    public GrenadeIcon[] grenades;
    public static WeaponManager Instance;
    public float size;

    public GameObject defaultHitEnemyParticles;
    public GameObject defaultHitWallParticles;


    public Volume flashbangVolume;
    private void Awake()
    {
        if (Instance == null)
            Instance = this;
    }

    public GameObject GetWeaponByName(string name)
    {
        foreach (WeaponToTake item in allWeapons)
        {
            if (item.name == name)
                return item.prefab;
        }
        return null;
    }

    public void AddFlashBangEffect(float targetWeight, float holdTime)
    {
        StopAllCoroutines();
        StartCoroutine(flashbangDamp(targetWeight, holdTime));
    }
    IEnumerator flashbangDamp(float targetWeight, float holdTime)
    {
        targetWeight = Mathf.Clamp01(targetWeight);
        float t = 0;
        float startWeight = flashbangVolume.weight;
        while (t < 1)
        {
            t += Time.deltaTime * 10;
            t = Mathf.Clamp01(t);
            flashbangVolume.weight = Mathf.Lerp(startWeight, targetWeight, t);
            yield return new WaitForEndOfFrame();
        }
        yield return new WaitForSeconds(0.5f);

        t = 0;
        startWeight = flashbangVolume.weight;
        while (t < 1)
        {
            t += Time.deltaTime / holdTime;
            t = Mathf.Clamp01(t);
            flashbangVolume.weight = Mathf.Lerp(startWeight, 0, t);
            yield return new WaitForEndOfFrame();
        }
    }
    public Sprite GetGrenadeIcon(string name)
    {
        foreach(GrenadeIcon grenadeIcon in grenades)
        {
            if (name == grenadeIcon.name)
                return grenadeIcon.icon;
        }

        return null;
    }

}
