using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class GameMaster : MonoBehaviour
{
    [SerializeField] GameObject[] spawnPlatforms = new GameObject[2];
    [SerializeField] LayerMask ecbLayer;
    [SerializeField] GameObject ringOutPrefab;
    [SerializeField] Collider2D[] bounds = new Collider2D[4];
    [SerializeField] GameObject shotgunPickupPrefab;
    [SerializeField] Vector2 spawnMin;
    [SerializeField] Vector2 spawnMax;

    [Header("Match / UI")]
    [SerializeField] string stageSelectSceneName = "Menu - Select Map";
    [SerializeField] GameObject winScreen;                        
    [SerializeField] Text winText;                                
    [SerializeField] float returnToStageSelectDelay = 3f; 

    GameObject currentShotgunPickup;
    AudioSource audio;
    bool matchEnded = false;

    IEnumerator Start()
    {
        audio = GetComponent<AudioSource>();
        QualitySettings.vSyncCount = 0;
        Application.targetFrameRate = 60;

        while (true)
        {
            yield return new WaitForSeconds(Random.Range(15f, 30f));

            if (matchEnded)
                yield break;

            SpawnShotgunPickup();
        }
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (matchEnded)
            return;

        if (1 << other.gameObject.layer == ecbLayer)
        {
            for (int i = 0; i < bounds.Length; i++)
            {
                if (other.IsTouching(bounds[i]))
                {
                    SpawnRingOutSplash(other.transform.position, bounds[i].transform.rotation);
                }
            }

            Avatar avatar = other.gameObject.GetComponentInParent<Avatar>();
            if (avatar == null)
                return;

            avatar.Lives -= 1;
            audio.Play();

            if (avatar.Lives <= 0)
            {
                int loserId = avatar.playerID;
                int winnerId = loserId == 0 ? 1 : 0;

                EndMatch(winnerId);
            }
            else
            {
                spawnPlatforms[avatar.playerID].SetActive(true);
                avatar.Respawn(spawnPlatforms[avatar.playerID].transform.position);
            }
        }
    }

    void SpawnRingOutSplash(Vector3 position, Quaternion rotation)
    {
        var go = Instantiate(ringOutPrefab);
        go.transform.position = position;
        go.transform.rotation = rotation;
    }

    public void SpawnShotgunPickup()
    {
        if (currentShotgunPickup != null)
            return;

        float x = Random.Range(spawnMin.x, spawnMax.x);
        float y = Random.Range(spawnMin.y, spawnMax.y);

        Vector2 spawnPos = new Vector2(x, y);

        currentShotgunPickup = Instantiate(shotgunPickupPrefab, spawnPos, Quaternion.identity);
    }

    void EndMatch(int winnerId)
    {
        matchEnded = true;

        Avatar[] avatars = FindObjectsOfType<Avatar>();
        foreach (var a in avatars)
        {
            a.enabled = false;

            var rb = a.GetComponent<Rigidbody2D>();
            if (rb != null)
            {
                rb.linearVelocity = Vector2.zero;
                rb.angularVelocity = 0f;
            }
        }

        if (winScreen != null)
            winScreen.SetActive(true);

        if (winText != null)
            winText.text = $"Jogador {winnerId + 1} Venceu!";

        StartCoroutine(ReturnToStageSelect());
    }

    IEnumerator ReturnToStageSelect()
    {
        yield return new WaitForSeconds(returnToStageSelectDelay);

        if (!string.IsNullOrEmpty(stageSelectSceneName))
        {
            SceneManager.LoadScene(stageSelectSceneName);
        }
    }
}
