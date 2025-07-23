using System.Collections;
using System.Collections.Generic;
using UnityEngine;
[RequireComponent(typeof(Camera))]
public class RTCamera : MonoBehaviour {
    Camera m_camera;
    [HideInInspector]
    public RenderTexture rt;
	// Use this for initialization
	private void Awake() {
		m_camera = GetComponent<Camera>();
	}
	void Start () {
        rt = new RenderTexture(Screen.width, Screen.height, 24);
        m_camera.targetTexture = rt;
    }

	public Texture UpdateRTTexture(){
		rt = new RenderTexture(Screen.width, Screen.height, 24);
        m_camera.targetTexture = rt;
		m_camera.Render();
		return rt;
	}
}
