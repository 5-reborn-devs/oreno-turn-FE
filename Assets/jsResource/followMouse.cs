using UnityEngine;
using UnityEngine.UI;

public class followMouse : MonoBehaviour
{
    public Image followImage; // 마우스 포인터를 따라다닐 이미지
	public Canvas canvas; // UI 캔버스
    public float maxY; // y축 최대값

    void Start()
    { // 스크립트가 위치한 오브젝트나 상위 오브젝트에서 Canvas를 찾습니다.
	  canvas = GetComponentInParent<Canvas>();
        if (canvas == null) { Debug.LogError("Canvas를 찾을 수 없습니다.");
        }
    }

        // Update is called once per frame
        void Update()
    {
        if (followImage != null)
        { // 마우스 위치를 캔버스의 월드 좌표로 변환
		  Vector2 mousePos;
          RectTransformUtility.ScreenPointToLocalPointInRectangle(canvas.transform as RectTransform, Input.mousePosition, canvas.worldCamera, out mousePos);

          // y축 위치 제한
	      if (mousePos.y > maxY) { mousePos.y = maxY; }

          // 이미지 위치 업데이트
          followImage.rectTransform.anchoredPosition = mousePos;
        }
    }
}
