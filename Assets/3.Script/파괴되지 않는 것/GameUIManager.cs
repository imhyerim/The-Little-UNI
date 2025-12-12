using System;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using System.Collections.Generic;
using System.Linq;
using TMPro;

public class GameUIManager : MonoBehaviour
{
    public static GameUIManager Instance;
    public GameObject inventoryPanel;

    public GameObject togglePanel;

    public List<GameObject> subOptionPanels;

    //[SerializeField]
    private Button[] buttons;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
            SceneManager.sceneLoaded += OnSceneLoaded;
            DontDestroyOnLoad(gameObject);
            
        }
        else
        {
            Destroy(gameObject);
        }
    }
    private void OnDestroy()
    {
        // 오브젝트가 파괴될 때 이벤트 구독을 해제합니다.
        if (Instance == this)
        {
            SceneManager.sceneLoaded -= OnSceneLoaded;
        }
    }
    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        // 1. 씬에 있는 모든 Button 컴포넌트를 찾아서 배열에 할당합니다.
        //    (씬마다 버튼 구성이 다르므로 매번 새로 찾아야 합니다.)
        buttons = FindObjectsOfType<Button>();
        // 2. 새로운 씬이 로드될 때, 설정 창이 꺼져 있다면 버튼은 활성화 상태여야 합니다.
        if (togglePanel != null)
        {
            buttons_Act(!togglePanel.activeSelf);
        }
    }
    void Start()
    {
        if (togglePanel != null)
        {
            togglePanel.SetActive(false);
        }
        if (subOptionPanels != null)
        {
            foreach (var panel in subOptionPanels)
            {
                if (panel != null) panel.SetActive(false);
            }
        }
        buttons = FindObjectsOfType<Button>();
    }
    public void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            HandleEscapeKey();
        }
    }
    public void HandleEscapeKey()
    {

        GameObject activeSubPanel = GetActiveSubPanel();
        if (activeSubPanel != null)
        {
            // 1순위: 서브 옵션 창이 켜져 있을 때 (subOptionPanels 목록)

            // 1-A. 해당 서브 창을 끕니다.
            activeSubPanel.SetActive(false);

            // 1-B. ESC 창(`togglePanel`)을 켭니다. (이미 켜져 있다면 그대로 둠)
            if (togglePanel != null && !togglePanel.activeSelf)
            {
                // TogglePanel()을 호출하여 ESC 창을 켜고 버튼을 비활성화합니다.
                TogglePanel();
            }
        }
        else if (inventoryPanel != null && inventoryPanel.activeSelf)
        {
            // 2순위: 인벤토리 창이 켜져 있을 때

            // 2-A. 인벤토리 창을 끕니다.
            inventoryPanel.SetActive(false);

            // 2-B. ESC 창(`togglePanel`)을 켭니다. (이미 켜져 있다면 그대로 둠)
            if (togglePanel != null && !togglePanel.activeSelf)
            {
                TogglePanel();
            }
        }
        else
        {
            // 3순위: 다른 모든 창이 꺼져 있을 때

            // ESC 창을 토글합니다. (열기 또는 닫기)
            TogglePanel();
        }
    }
    private GameObject GetActiveSubPanel()
    {
        if (subOptionPanels == null) return null;
        foreach (GameObject panel in subOptionPanels)
        {
            if (panel != null && panel.activeSelf)
            {
                return panel;
            }
        }
        return null;
    }
    public void TogglePanel()
    {
        if (togglePanel == null) return;
        bool newState = !togglePanel.activeSelf;
        togglePanel.SetActive(newState);
        buttons_Act(!newState);
    }
    public void buttons_Act(bool isact)
    {
        if (buttons == null) return;
        foreach (Button b in buttons)
        {
            b.interactable = isact;
        }
    }
}
