using System;
using UnityEngine;
using UnityEngine.UI;

public class OxygenBarSlider : MonoBehaviour
{
    private Slider slider;

    void Awake()
    {
        slider = GetComponentInChildren<Slider>();
        slider.minValue = 0f;
        slider.maxValue = 100f;
    }

    private void Start()
    {
        if(!GameLoopManager.Instance.bIsOperating_) gameObject.SetActive(false);
        else gameObject.SetActive(true);
    }

    void OnEnable()
    {
        if (GameLoopManager.Instance == null) return;
        
        slider.value = GameLoopManager.Instance.GAS;
    }

    void OnDisable()
    {
        if (GameLoopManager.Instance == null) return;
    }
    
    private void Update()
    {
        slider.value = GameLoopManager.Instance.GAS;
    }
}
