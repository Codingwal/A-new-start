using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using TMPro;

public class ButtonScript : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    public GameObject newMenu;
    Button button;

    TMP_Text text;
    public int fontSize = 40;
    public int fontSizeOnHover = 60;

    private void Start() {
        text = GetComponentInChildren<TMP_Text>();
        button = GetComponent<Button>();
        button.onClick.AddListener(OnClick);
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        text.fontSize = fontSizeOnHover;
    }   
    public void OnPointerExit(PointerEventData eventData)
    {
        text.fontSize = fontSize;
    }
    private void OnClick()
    {

        if (newMenu == null)
        {
            return;
        }
        newMenu.SetActive(true);
        transform.parent.gameObject.SetActive(false);
    }
}
