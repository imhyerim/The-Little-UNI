using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class Buttonclick : MonoBehaviour
{
    public Button[] mainButtons;
    private void SetMainButtonsActive(bool isEnabled)
    {
        foreach (Button btn in mainButtons)
        {
            if (btn != null)
            {
                // 버튼이 화면에 보이지만, 클릭만 가능/불가능하게 만듭니다.
                btn.interactable = isEnabled;
            }
        }
    }
    public void OpenPanel(GameObject targetPanel)
    {
        if (targetPanel != null)
        {
            // 1. 주 메뉴 버튼들을 비활성화합니다. (클릭 불가능)
            SetMainButtonsActive(false);

            // 2. 대상 UI 창을 활성화합니다.
            targetPanel.SetActive(true);
            Debug.Log(targetPanel.name + " UI 창이 열렸고, 메인 버튼이 비활성화되었습니다.");
        }
    }
    public void ClosePanel(GameObject targetPanel)
    {
        if (targetPanel != null)
        {
            // 1. 대상 UI 창을 비활성화합니다.
            targetPanel.SetActive(false);

            // 2. 주 메뉴 버튼들을 다시 활성화합니다. (클릭 가능)
            SetMainButtonsActive(true);

            Debug.Log(targetPanel.name + " UI 창이 닫혔고, 메인 버튼이 재활성화되었습니다.");
        }
    }
}

