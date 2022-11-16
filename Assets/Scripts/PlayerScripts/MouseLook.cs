using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
public class MouseLook : MonoBehaviour
{
	public float minX = -60f;
	public float maxX = 60f;

	public float sensitivity;
	public Transform cam;
	//public Transform recoilT;

	float rotY = 0f;
	
	float rotYMin = 0f;
	float rotYMax = 0f;


	float rotX = 0f;

	Vector2 targetRecoil;
	public float recoilSpeed;
	float recoilTimeLeft;
	PhotonView PV;
	public GameObject[] camsToOff;
	PlayerShooting handle;
	PlayerMove PM;
	float inputX;
	float inputY;


	bool limitedRotation = false;

    private void Awake()
    {
		PV = GetComponent<PhotonView>();
		handle = GetComponent<PlayerShooting>();
		PM = GetComponent<PlayerMove>();
	}
    private void Start()
    {
        if(!PV.IsMine)
        {
			foreach(GameObject go in camsToOff)
            {
				go.SetActive(false);
            }
        }
    }
    void Update()
	{
		if (!PV.IsMine || GlobalUIManager.Instance.paused)
			return;
		RecoilLook();
	}
	void RecoilLook()
	{
		inputX = Input.GetAxisRaw("Mouse X");
		inputY = Input.GetAxisRaw("Mouse Y");

		//handle.ProceduralAnimation(inputX* PlayerSettings.Instance.mouseSensitivityX/15, inputY* PlayerSettings.Instance.mouseSensitivityY / 6);
		

		rotY += inputX * PlayerSettings.Instance.mouseSensitivityY/6;
		rotX += inputY * PlayerSettings.Instance.mouseSensitivityX / 6;

		if(limitedRotation)
        {
			rotY = Mathf.Clamp(rotY, rotYMin, rotYMax);
        }

		float targetX = rotX - targetRecoil.x;
		targetX = Mathf.Clamp(targetX, minX, maxX);

		transform.localEulerAngles = AngleLerp(transform.localEulerAngles,new Vector3(0, rotY + targetRecoil.y, 0),recoilSpeed*Time.deltaTime);

		cam.transform.localEulerAngles = AngleLerp(cam.transform.localEulerAngles,new Vector3(-targetX, 0, 0),recoilSpeed*Time.deltaTime);
	}

    private void FixedUpdate()
    {
		handle.ProceduralAnimation(inputX* PlayerSettings.Instance.mouseSensitivityX / 15, inputY * inputY * PlayerSettings.Instance.mouseSensitivityY / 15);
	}
	public void SetRepel(float startRot,int range)
    {
		rotY = startRot;
		rotYMin = startRot - range;
		rotYMax = startRot + range;
		limitedRotation = true;

	}
	public void StopRepel()
    {
		limitedRotation = false;
	}

    public void AddRecoil(float x, float y)
    {
		if(recoilTimeLeft <= 0)
			targetRecoil = Vector2.zero;

		recoilTimeLeft = 0.2f;
		targetRecoil.x -= Mathf.Abs(x);
		targetRecoil.y += y;
	}

	Vector3 AngleLerp(Vector3 StartAngle, Vector3 FinishAngle, float t)
	{
		float xLerp = Mathf.LerpAngle(StartAngle.x, FinishAngle.x, t);
		float yLerp = Mathf.LerpAngle(StartAngle.y, FinishAngle.y, t);
		float zLerp = Mathf.LerpAngle(StartAngle.z, FinishAngle.z, t);
		Vector3 Lerped = new Vector3(xLerp, yLerp, zLerp);
		return Lerped;
	}
}