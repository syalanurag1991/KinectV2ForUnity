
using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using Windows.Kinect;

public class RegisteredColorView : MonoBehaviour
{
	public GameObject ColorSourceManager;
	private ColorSourceManager _ColorManager;
	private MultiSourceManager _MultiManager;
	private Renderer objectRenderer;

	void Start()
	{
		objectRenderer = gameObject.GetComponent<Renderer>();
		if (objectRenderer != null)
			objectRenderer.material.SetTextureScale("_MainTex", new Vector2(-1, 1));
	}

	void Update()
	{
		if (ColorSourceManager == null)
		{
			return;
		}

		_ColorManager = ColorSourceManager.GetComponent<ColorSourceManager>();
		if (_ColorManager != null)
		{
			if (objectRenderer != null)
				gameObject.GetComponent<Renderer>().material.mainTexture = _ColorManager.GetColorTexture();
			else
				gameObject.GetComponent<RawImage>().texture = _ColorManager.GetColorTexture();
			return;
		}

		_MultiManager = ColorSourceManager.GetComponent<MultiSourceManager>();
		if (_MultiManager != null)
		{
			if (objectRenderer != null)
				gameObject.GetComponent<Renderer>().material.mainTexture = _MultiManager.GetColorTexture();
			else
				gameObject.GetComponent<RawImage>().texture = _MultiManager.GetColorTexture();
		}

		return;
	}
}