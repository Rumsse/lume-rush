using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using System.Collections;
using FMODUnity;
using FMOD.Studio;

public class IntroCinematicController : MonoBehaviour
{
    [System.Serializable]
    public class Slide
    {
        public Sprite image;
        public float holdTime;
        public float fadeTime;
        public float startScale = 1f;
        public float endScale = 1f;
    }

    public Image imageA;
    public Image imageB;
    public Image blackFade;

    public Slide[] slides;

    public string sceneToLoad;

    public EventReference musicEvent;

    public KeyCode skipKey = KeyCode.Space;

    private bool skipping;
    private EventInstance musicInstance;

    private void Start()
    {
        StartCoroutine(PlayCinematic());
    }

    private void Update()
    {
        if (Input.GetKeyDown(skipKey))
            Skip();
    }

    private IEnumerator PlayCinematic()
    {
        if (!musicEvent.IsNull)
        {
            musicInstance = RuntimeManager.CreateInstance(musicEvent);
            musicInstance.start();
        }

        SetAlpha(imageA, 0f);
        SetAlpha(imageB, 0f);
        SetAlpha(blackFade, 1f);

        Image current = imageA;
        Image next = imageB;

        current.sprite = slides[0].image;
        current.rectTransform.localScale = Vector3.one * slides[0].startScale;

        SetAlpha(current, 1f);
        SetAlpha(next, 0f);

        BringBlackToFront();

        yield return FadeImage(blackFade, 1f, 0f, 1f);

        yield return AnimateSlide(
            current,
            slides[0].holdTime,
            slides[0].startScale,
            slides[0].endScale
        );

        for (int i = 1; i < slides.Length; i++)
        {
            if (skipping)
                yield break;

            next.sprite = slides[i].image;
            next.rectTransform.localScale = Vector3.one * slides[i].startScale;

            SetAlpha(next, 1f);
            SetAlpha(current, 1f);

            next.transform.SetAsFirstSibling();
            current.transform.SetAsLastSibling();
            BringBlackToFront();

            yield return FadeTopImageOut(current, slides[i].fadeTime);

            SetAlpha(current, 0f);

            Image temp = current;
            current = next;
            next = temp;

            yield return AnimateSlide(
                current,
                slides[i].holdTime,
                slides[i].startScale,
                slides[i].endScale
            );
        }

        yield return FadeImage(blackFade, 0f, 1f, 1f);

        Finish();
    }

    private IEnumerator AnimateSlide(Image image, float duration, float startScale, float endScale)
    {
        float timer = 0f;

        while (timer < duration)
        {
            if (skipping)
                yield break;

            timer += Time.deltaTime;
            float t = Mathf.Clamp01(timer / duration);

            image.rectTransform.localScale = Vector3.one * Mathf.Lerp(startScale, endScale, t);

            yield return null;
        }
    }

    private IEnumerator FadeTopImageOut(Image image, float duration)
    {
        float timer = 0f;

        while (timer < duration)
        {
            if (skipping)
                yield break;

            timer += Time.deltaTime;
            float t = Mathf.Clamp01(timer / duration);

            SetAlpha(image, 1f - t);

            yield return null;
        }

        SetAlpha(image, 0f);
    }

    private IEnumerator FadeImage(Image image, float from, float to, float duration)
    {
        float timer = 0f;

        while (timer < duration)
        {
            if (skipping)
                yield break;

            timer += Time.deltaTime;
            float t = Mathf.Clamp01(timer / duration);

            SetAlpha(image, Mathf.Lerp(from, to, t));

            yield return null;
        }

        SetAlpha(image, to);
    }

    private void SetAlpha(Image image, float alpha)
    {
        if (image == null)
            return;

        Color c = image.color;
        c.a = alpha;
        image.color = c;
    }

    private void BringBlackToFront()
    {
        if (blackFade != null)
            blackFade.transform.SetAsLastSibling();
    }

    public void Skip()
    {
        if (skipping)
            return;

        skipping = true;
        StopAllCoroutines();
        Finish();
    }

    private void Finish()
    {
        if (musicInstance.isValid())
        {
            musicInstance.stop(FMOD.Studio.STOP_MODE.ALLOWFADEOUT);
            musicInstance.release();
        }

        SceneManager.LoadScene(sceneToLoad);
    }
}