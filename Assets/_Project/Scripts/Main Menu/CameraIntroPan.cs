using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

public class CameraIntroPan : MonoBehaviour
{
    public float startOffsetY = 1.5f;
    public float duration = 5f;
    public GameObject menuUI;
    public string nextScene = "Intro";

    IEnumerator Start()
    {
        Vector3 end = transform.position;
        Vector3 start = end + new Vector3(0f, startOffsetY, 0f);
        transform.position = start;
        if (menuUI != null) menuUI.SetActive(true);

        yield return new WaitForSeconds(0.5f);
        while (!Input.anyKeyDown) yield return null;

        if (menuUI != null) menuUI.SetActive(false);

        for (float t = 0f; t < duration; t += Time.deltaTime)
        {
            transform.position = Vector3.Lerp(start, end, Mathf.SmoothStep(0f, 1f, t / duration));
            yield return null;
        }
        transform.position = end;
        SceneManager.LoadScene(nextScene);
    }
}