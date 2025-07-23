using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Pinwheel;

[RequireComponent(typeof(PositionAnimation))]
public class UIInOutAnim_PositionAnim : UIInOutAnim {
	public override void FadeIn (float duration)
	{
		PositionAnimation positionAnimation = GetComponent<PositionAnimation> ();
		positionAnimation.Play (positionAnimation.curves [0]);
	}
	public override void FadeOut (float duration){
		PositionAnimation positionAnimation = GetComponent<PositionAnimation> ();
		if(positionAnimation.curves.Length>1)
			positionAnimation.Play (positionAnimation.curves [1]);
	}
}
