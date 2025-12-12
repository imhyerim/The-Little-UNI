using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class BorderImage : MonoBehaviour ,IPointerEnterHandler, IPointerExitHandler
{


    //MonoBehaviour = 이 클래스가 Unity 게임 오브젝트에 컴포넌트로 붙을 수 있도록 해주는 시본 클래스
    //IPointerEnterHandler = 마우스 포인터가  UI요소 안으로 들어왔을 때 호출되는 함수
    //IPointerExitHandler = 마우스 포인터가 UI 요소 밖으로나갔을 때 호출되는 함수
    public GameObject borderObject;
    //테두리 이미지 오브젝트

    public GameObject tooltipObject;
    //Inspector 에서 툴팁 텍스트 오브젝트를 연결하는 것


    public void OnPointerEnter(PointerEventData eventData)
    {
        if (tooltipObject != null)
        {
            tooltipObject.SetActive(true); // 텍스트 활성화
        }

        if (borderObject != null)
        {
            borderObject.SetActive(true);
        }
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        if (tooltipObject != null)
        {
            tooltipObject.SetActive(false);
        }
        if (borderObject != null)
        {
            borderObject.SetActive(false);
        }
    }



}
