using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class LoadingBar : MonoBehaviour
{

    private static GameObject instance;
    public Text percentText;
    public Slider percentSlider;
    private float changeSpeed = 0.0001f;
    private float duration = 0f;
    // Start is called before the first frame update
    public static void Show() {
        instance = (GameObject)Instantiate(Resources.Load("Prefabs/LoadingBar"));
    }

    void Start() {
        Log.I("Game Start Loading");
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        percentSlider.value += changeSpeed;

        // 检查是否需要改变速度
        duration -= Time.deltaTime;
        if (duration <= 0)
        {
            ChangeSpeed();
        }

        percentText.text = $"{(int)Math.Round(percentSlider.value * 100)}%";

        if (percentSlider.value >= 1f) {
            GameLoadFinishedEvent.Invoke(new GameLoadFinishedEvent{});
            Destroy(gameObject);
            Log.I("Game Finish Loading");
        }
    }

    private void ChangeSpeed()
    {
        // 在70%的概率下，选择快速增加的速度；在30%的概率下，选择慢速增加的速度
        if (UnityEngine.Random.value <= 0.6f)
        {
            changeSpeed = 0.007f;
        }
        else
        {
            changeSpeed = 0.0008f;
        }

        // 选择一个新的"持续时间"，这个持续时间可以根据需要进行调整
        duration = UnityEngine.Random.Range(0.5f, 1.5f);
    }
}
