using System.Collections.Generic;
using _Combo;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    public int ZoneId { get; set; }
    public int ServerId { get; set; }
    public Dictionary<int, Sprite> RoleDic = new Dictionary<int, Sprite>();

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject); // 保证在切换场景时不会被销毁
        }
        else
        {
            Destroy(gameObject); // 确保只有一个实例存在
        }
    }
}
