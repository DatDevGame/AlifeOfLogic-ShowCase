using System.Collections;
using System.Collections.Generic;
using UnityEngine;
namespace Takuzu{
	public class WalkthroughStepTimeCoundown : WalkthroughStep {
		public override void StartWalkthrough (TutorialManager4 tutorialManager)
		{
			base.StartWalkthrough (tutorialManager);
			StartCoroutine (CR_CountDown());
		}

		private IEnumerator CR_CountDown(){
			Debug.Log("CountDownWalkThrough");
			yield return null;
			string puzzle = "100.1.0.0011.110";
			string solution = "1001110000110110";
			Index2D[] intereactableIndexes = null;
			base.tutorialManager.RequestNewLogicalBoard (puzzle, solution, intereactableIndexes);
			yield return new WaitForSeconds (5);
			base.isFinished = true;
		}
	}
}
