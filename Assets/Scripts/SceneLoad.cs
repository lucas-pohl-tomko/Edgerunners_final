using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneLoad : MonoBehaviour
{
	[SerializeField] string levelName;

	// Use this for initialization
	void Start()
	{
		
	}
	
	// Update is called once per frame
	void Update()
	{
		
	}

	public void LoadScene()
	{
		SceneManager.LoadScene(levelName);
        StartCoroutine(UnloadUnusedAssetsAfterLoad());
    }

    private IEnumerator UnloadUnusedAssetsAfterLoad()
    {
        yield return null; // Wait one frame for the new scene to fully initialize
        Resources.UnloadUnusedAssets();
    }

    public void Exit()
	{
		Application.Quit();
	}
}
