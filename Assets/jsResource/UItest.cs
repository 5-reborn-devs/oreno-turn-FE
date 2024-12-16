using UnityEngine;
using UnityEngine.UI;

public class UItest : MonoBehaviour
{
    [SerializeField] private GameObject targetObject; // 활성화/비활성화할 게임 오브젝트
    [SerializeField] private Button triggerButton; // UI 버튼

    void Start()
    {
        if (triggerButton != null)
        {
            triggerButton.onClick.AddListener(ToggleObjectActive);
        }
    }

    void ToggleObjectActive()
    {
        if (targetObject != null)
        {
            bool isActive = targetObject.activeSelf;
            targetObject.SetActive(!isActive); // 오브젝트 활성화/비활성화 토글
            Debug.Log($"오브젝트가 {(isActive ? "비활성화" : "활성화")}되었습니다.");
        }
        else
        {
            Debug.LogError("타겟 오브젝트가 설정되지 않았습니다.");
        }
    }
}
