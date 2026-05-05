using UnityEngine;
using UnityEngine.Rendering;
using System.Collections;

public class TimeTravelManager : MonoBehaviour
{
    public Volume normalVolume;
    public Volume journalistVolume;

    public CanvasGroup fadeCanvas;
    public GameObject overlayUI;

    public Camera cam;

    public void StartTimeTravel()
    {
        StartCoroutine(TimeTravel());
    }

    public void ReturnToPresent()
    {
        StartCoroutine(TimeTravelBack());
    }

    IEnumerator TimeTravel()
    {
        float t;

        // Fade to black
        t = 0;
        while (t < 1)
        {
            t += Time.deltaTime * 2.5f;
            fadeCanvas.alpha = t;
            yield return null;
        }

        // Zoom + slight shake
        float startFOV = cam.fieldOfView;
        float targetFOV = startFOV - 6;

        t = 0;
        while (t < 1)
        {
            t += Time.deltaTime * 1.5f;

            cam.fieldOfView = Mathf.Lerp(startFOV, targetFOV, t);
            cam.transform.localPosition += Random.insideUnitSphere * 0.02f;

            yield return null;
        }

        // Switch to 1949 look
        normalVolume.weight = 0;
        journalistVolume.weight = 1;

        overlayUI.SetActive(true);

        // Fade back in
        t = 1;
        while (t > 0)
        {
            t -= Time.deltaTime * 2.5f;
            fadeCanvas.alpha = t;
            yield return null;
        }
    }

    IEnumerator TimeTravelBack()
    {
        float t;

        // Fade to black
        t = 0;
        while (t < 1)
        {
            t += Time.deltaTime * 2.5f;
            fadeCanvas.alpha = t;
            yield return null;
        }

        // Zoom back + slight shake
        float startFOV = cam.fieldOfView;
        float targetFOV = startFOV + 6;

        t = 0;
        while (t < 1)
        {
            t += Time.deltaTime * 1.5f;

            cam.fieldOfView = Mathf.Lerp(startFOV, targetFOV, t);
            cam.transform.localPosition += Random.insideUnitSphere * 0.02f;

            yield return null;
        }

        // Switch back to present look
        normalVolume.weight = 1;
        journalistVolume.weight = 0;

        overlayUI.SetActive(false);

        // Fade back in
        t = 1;
        while (t > 0)
        {
            t -= Time.deltaTime * 2.5f;
            fadeCanvas.alpha = t;
            yield return null;
        }
    }
}