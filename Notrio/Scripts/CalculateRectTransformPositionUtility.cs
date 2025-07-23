using UnityEngine;
public static class CalculateRectTransformPositionUtility{
	public static  Vector2 GetNormalizedLocalPointerPosition(Vector2 screenPosition, RectTransform container, Camera canvasCamera = null){
		Vector2 localPointerPosition = Vector2.zero;
		RectTransformUtility.ScreenPointToLocalPointInRectangle( container , screenPosition, canvasCamera, out localPointerPosition);
        Debug.Log(localPointerPosition);
		Vector2 normalizedLocalPointerPositon = new Vector2(localPointerPosition.x*2/container.rect.width,localPointerPosition.y*2/container.rect.height);
		normalizedLocalPointerPositon = Vector2.ClampMagnitude(normalizedLocalPointerPositon ,1);
		return normalizedLocalPointerPositon;
	}

	public static float GetSignedAngleFromScreenPonterPostion(Vector2 screenPosition, RectTransform container){
		Vector2 normalizedLocalPointerPosition = GetNormalizedLocalPointerPosition(screenPosition,container);
		return Vector2.SignedAngle(Vector2.left, normalizedLocalPointerPosition);
	}
}