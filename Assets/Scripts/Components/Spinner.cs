using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Spinner : MonoBehaviour
{
    public float rotationSpeed = 60f;

    // 获取Image组件
    private Image image;

    void Start()
    {
        // 获取Image组件
        image = GetComponent<Image>();
    }

    void Update()
    {
        // 获取当前RectTransform的旋转角度
        float currentRotation = image.rectTransform.eulerAngles.z;

        // 计算新的旋转角度
        float newRotation = currentRotation - rotationSpeed * Time.deltaTime;

        // 将新的旋转角度应用到RectTransform
        image.rectTransform.rotation = Quaternion.Euler(0f, 0f, newRotation);
    }
}
