using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Takuzu{
	public class WalkthroughStep : MonoBehaviour {
		protected TutorialManager4 tutorialManager;
		[HideInInspector]
		public bool isFinished = false;
		virtual public void StartWalkthrough(TutorialManager4 tutorialManager){
			this.tutorialManager = tutorialManager;
		}

        public IEnumerator CR_WaitNext(float seconds, bool enableCheckInput)
        {
            float t = 0;
            while( t < seconds)
            {
                t += Time.deltaTime;
                if (enableCheckInput && Input.GetMouseButtonDown(0))
                    break;
                yield return null;
            }
        }
	}
}
