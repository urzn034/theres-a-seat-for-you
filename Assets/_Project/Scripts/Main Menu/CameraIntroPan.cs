using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using TMPro;

public class CameraIntroPan : MonoBehaviour
{
    public float startOffsetY = 1.5f;
    public float duration = 5f;
    public GameObject menuUI;
    public string nextScene = "Intro";

    [Header("Judul Chapter")]
    public TMP_FontAsset titleFont;
    public string chapterLabel = "CHAPTER 1";
    public string chapterTitle = "A Town Named Willowmere";
    public float titleFadeTime = 0.8f;
    public float titleHoldTime = 2f;

    IEnumerator Start()
    {
        Vector3 end = transform.position;
        Vector3 start = end + new Vector3(0f, startOffsetY, 0f);
        transform.position = start;
        if (menuUI != null) menuUI.SetActive(true);

        yield return new WaitForSeconds(0.5f);
        while (!Input.anyKeyDown) yield return null;

        if (menuUI != null) menuUI.SetActive(false);

        if (MainMenu.hasSession && !string.IsNullOrEmpty(MainMenu.sessionScene))
        {
            SceneManager.LoadScene(MainMenu.sessionScene);
            yield break;
        }

 
        var co = new GameObject("TitleCanvas");
        var canvas = co.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = 1000;
        var sc = co.AddComponent<CanvasScaler>();
        sc.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        sc.referenceResolution = new Vector2(1920, 1080);

        var bgObj = new GameObject("Dim");
        bgObj.transform.SetParent(co.transform, false);
        var bg = bgObj.AddComponent<Image>();
        bg.color = new Color(0, 0, 0, 0);
        bg.raycastTarget = false;
        var br = bg.rectTransform;
        br.anchorMin = Vector2.zero; br.anchorMax = Vector2.one; br.sizeDelta = Vector2.zero;

        var tObj = new GameObject("Title");
        tObj.transform.SetParent(co.transform, false);
        var tr = tObj.AddComponent<RectTransform>();
        tr.anchorMin = Vector2.zero; tr.anchorMax = Vector2.one; tr.sizeDelta = Vector2.zero;
        var t = tObj.AddComponent<TextMeshProUGUI>();
        if (titleFont != null) t.font = titleFont;
        t.alignment = TextAlignmentOptions.Center;
        t.fontSize = 56;
        t.text = chapterLabel + "\n<size=170%>" + chapterTitle + "</size>";
        t.raycastTarget = false;
        Color txt = new Color(1f, 0.95f, 0.85f, 1f);

        for (float x = 0f; x < titleFadeTime; x += Time.unscaledDeltaTime)
        {
            float a = x / titleFadeTime;
            bg.color = new Color(0, 0, 0, 0.55f * a);
            t.color = new Color(txt.r, txt.g, txt.b, a);
            yield return null;
        }
        bg.color = new Color(0, 0, 0, 0.55f);
        t.color = txt;

        yield return new WaitForSecondsRealtime(titleHoldTime);

        for (float x = 0f; x < titleFadeTime; x += Time.unscaledDeltaTime)
        {
            float a = 1f - x / titleFadeTime;
            bg.color = new Color(0, 0, 0, 0.55f * a);
            t.color = new Color(txt.r, txt.g, txt.b, a);
            yield return null;
        }
        Destroy(co);

        // kamera bandel
        for (float x = 0f; x < duration; x += Time.deltaTime)
        {
            transform.position = Vector3.Lerp(start, end, Mathf.SmoothStep(0f, 1f, x / duration));
            yield return null;
        }
        transform.position = end;
        SceneManager.LoadScene(nextScene);
    }
}