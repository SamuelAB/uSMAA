using UnityEngine;

[ExecuteInEditMode]
[RequireComponent(typeof(Camera))]
public class SMAA : MonoBehaviour
{
	[Range(1, 3)]
	public int RenderState = 3;
	public int Passes = 1;

	private Texture2D black;
	private Shader shader;
	private Material mat;

	private AreaTexture areaTexture;
	private SearchTexture searchTexture;

	Material material
	{
		get
		{
			if (mat == null)
			{
				mat = new Material(shader);
				mat.hideFlags = HideFlags.HideAndDontSave;
			}

			return mat;
		}
	}

	void OnEnable()
	{
		if (areaTexture == null)
			areaTexture = new AreaTexture();

		if (searchTexture == null)
			searchTexture = new SearchTexture();
	}

	void Start()
	{
		// Disable if we don't support image effects
		if (!SystemInfo.supportsImageEffects)
		{
			enabled = false;
			return;
		}

		shader = Shader.Find("Custom/SMAAshader");

		// Disable the image effect if the shader can't
		// run on the users graphics card
		if (!shader || !shader.isSupported)
			enabled = false;

		black = new Texture2D(1,1);
		black.SetPixel(0,0,new Color(0,0,0,0));
		black.Apply();
	}

	void OnDisable()
	{
		if (mat)
			DestroyImmediate(mat);
	}

	void OnRenderImage(RenderTexture source, RenderTexture destination)
	{
		Vector4 metrics = new Vector4(1 / (float)Screen.width, 1 / (float)Screen.height, Screen.width, Screen.height);

		if (RenderState == 1)
		{
			Graphics.Blit(source, destination, material, 0);
		}
		else if (RenderState == 2)
		{
			material.SetTexture("areaTex", areaTexture.alphaTex);
			material.SetTexture("luminTex", areaTexture.luminTex);
			material.SetTexture("searchTex", searchTexture.alphaTex);
			material.SetVector("SMAA_RT_METRICS", metrics);

			var rt = RenderTexture.GetTemporary(Screen.width, Screen.height, 0);

			Graphics.Blit(source, rt, material, 0);
			Graphics.Blit(rt, destination, material, 1);

			rt.Release();
		}
		else
		{
			material.SetTexture("areaTex", areaTexture.alphaTex);
			material.SetTexture("luminTex", areaTexture.luminTex);
			material.SetTexture("searchTex", searchTexture.alphaTex);
			material.SetTexture("_SrcTex", source);
			material.SetVector("SMAA_RT_METRICS", metrics);

			var rt = RenderTexture.GetTemporary(Screen.width, Screen.height, 0);
			var rt2 = RenderTexture.GetTemporary(Screen.width, Screen.height, 0);
			var rt3 = RenderTexture.GetTemporary(Screen.width, Screen.height, 0);

			Graphics.Blit(source, rt3);
			for (var i = 0; i < Passes; i++)
			{
				Graphics.Blit(black, rt);
				Graphics.Blit(black, rt2);

				Graphics.Blit(rt3, rt, material, 0);

				Graphics.Blit(rt, rt2, material, 1);
				Graphics.Blit(rt2, rt3, material, 2);
			}
			Graphics.Blit(rt3, destination);

			rt.Release();
			rt2.Release();
			rt3.Release();
		}
	}
}
