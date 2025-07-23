using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Takuzu;
public class UIFadeInOutAnim : UIInOutAnim {
	List<Image> images = new List<Image> ();
	List<Text> texts = new List<Text> ();
    public bool IsFadeIn { get; private set; }
    public bool IsShowAtStart;

	void Awake(){
		images.Add (gameObject.GetComponent<Image> ());
		images.AddRange (gameObject.GetComponentsInChildren<Image> ());
		texts.Add (gameObject.GetComponent<Text> ());
		texts.AddRange (gameObject.GetComponentsInChildren<Text> ());
	}
    private void Start()
    {
        if (!IsShowAtStart)
        {
            for (int i = 0; i < images.Count; i++)
            {
                if (images[i])
                    images[i].color = GetLerpAlPhaColor(images[i].color, 0);
            }
            for (int i = 0; i < texts.Count; i++)
            {
                if (texts[i])
                    texts[i].color = GetLerpAlPhaColor(texts[i].color, 0); ;
            }
            IsFadeIn = false;
        }
        else
        {
            IsFadeIn = true;
        }
    }
    public override void FadeIn(float duration){
        if (!IsFadeIn)
        {
            base.FadeIn(duration);
            StartCoroutine(CR_FadeIn(duration));
            IsFadeIn = true;
        }
	}
	public override void FadeOut(float duration){
        if (IsFadeIn)
        {
            base.FadeOut(duration);
            StartCoroutine(CR_FadeOut(duration));
            IsFadeIn = false;
        }
	}
	private IEnumerator CR_FadeIn(float duration){
		float timeLeft = duration;
		while (timeLeft >0) {
			timeLeft -= Time.deltaTime;
			for (int i = 0; i < images.Count; i++) {
				if (images [i])
					images [i].color = GetLerpAlPhaColor(images[i].color,1 - (timeLeft / duration));
			}
			for (int i = 0; i < texts.Count; i++) {
				if (texts [i]) {
					texts [i].color =  GetLerpAlPhaColor(texts[i].color,1 - (timeLeft / duration));
				}
			}
			yield return new WaitForEndOfFrame ();
		}
		for (int i = 0; i < images.Count; i++) {
			if (images [i])
				images [i].color = GetLerpAlPhaColor(images[i].color,1);
		}
		for (int i = 0; i < texts.Count; i++) {
			if (texts [i])
				texts [i].color = GetLerpAlPhaColor(texts[i].color,1);
		}
		yield return null;
	}
	private IEnumerator CR_FadeOut(float duration){
		float timeLeft = duration;
		while (timeLeft >0) {
			timeLeft -= Time.deltaTime;
			for (int i = 0; i < images.Count; i++) {
				if (images [i])
					images [i].color = GetLerpAlPhaColor(images[i].color,(timeLeft / duration));
			}
			for (int i = 0; i < texts.Count; i++) {
				if (texts [i])
					texts [i].color = GetLerpAlPhaColor(texts[i].color,(timeLeft / duration));
			}
			yield return new WaitForEndOfFrame ();
		}
		for (int i = 0; i < images.Count; i++) {
			if (images [i])
				images [i].color = GetLerpAlPhaColor(images[i].color,0);
		}
		for (int i = 0; i < texts.Count; i++) {
			if (texts [i])
				texts [i].color = GetLerpAlPhaColor(texts[i].color,0);
		}
		yield return null;
	}
    private Color GetLerpAlPhaColor(Color c, float x)
    {
        Color newColor = c;
        newColor.a = x;
        return newColor;
    }
}
