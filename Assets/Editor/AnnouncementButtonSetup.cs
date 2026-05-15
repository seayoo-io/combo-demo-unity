using UnityEditor;
using UnityEditor.Events;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

// 在 Login 和 Game 场景中一键创建并绑定「获取公告(Mock)」按钮
// 菜单入口：Demo → Setup GetAnnouncements Button
public static class AnnouncementButtonSetup
{
    [MenuItem("Demo/Setup GetAnnouncements Button")]
    public static void SetupInActiveScene()
    {
        var login = Object.FindObjectOfType<Login>();
        if (login != null)
        {
            SetupForLogin(login);
            return;
        }

        var game = Object.FindObjectOfType<Game>();
        if (game != null)
        {
            SetupForGame(game);
            return;
        }

        EditorUtility.DisplayDialog("提示", "当前场景中未找到 Login 或 Game 组件，请先打开对应场景再执行此菜单。", "确定");
    }

    // 在 Login 场景中创建按钮并绑定
    private static void SetupForLogin(Login login)
    {
        if (login.openAnnouncementsBtn == null)
        {
            EditorUtility.DisplayDialog("提示", "Login.openAnnouncementsBtn 为空，请先在 Inspector 中赋值。", "确定");
            return;
        }

        if (login.getAnnouncementsBtn != null)
        {
            EditorUtility.DisplayDialog("提示", "Login.getAnnouncementsBtn 已存在，无需重复创建。", "确定");
            return;
        }

        var btn = CreateButton(login.openAnnouncementsBtn.gameObject, "getAnnouncementsBtn");
        login.getAnnouncementsBtn = btn;
        UnityEventTools.AddPersistentListener(btn.onClick, (UnityAction)login.GetAnnouncementsMock);

        Undo.RecordObject(login, "Setup getAnnouncementsBtn");
        EditorUtility.SetDirty(login);
        EditorSceneManager.MarkSceneDirty(login.gameObject.scene);

        Debug.Log("[AnnouncementButtonSetup] Login 场景按钮创建完成，请保存场景（Ctrl+S）。");
    }

    // 在 Game 场景中创建按钮并绑定
    private static void SetupForGame(Game game)
    {
        if (game.openAnnouncementsBtn == null)
        {
            EditorUtility.DisplayDialog("提示", "Game.openAnnouncementsBtn 为空，请先在 Inspector 中赋值。", "确定");
            return;
        }

        if (game.getAnnouncementsBtn != null)
        {
            EditorUtility.DisplayDialog("提示", "Game.getAnnouncementsBtn 已存在，无需重复创建。", "确定");
            return;
        }

        var btn = CreateButton(game.openAnnouncementsBtn.gameObject, "getAnnouncementsBtn");
        game.getAnnouncementsBtn = btn;
        UnityEventTools.AddPersistentListener(btn.onClick, (UnityAction)game.GetAnnouncementsMock);

        Undo.RecordObject(game, "Setup getAnnouncementsBtn");
        EditorUtility.SetDirty(game);
        EditorSceneManager.MarkSceneDirty(game.gameObject.scene);

        Debug.Log("[AnnouncementButtonSetup] Game 场景按钮创建完成，请保存场景（Ctrl+S）。");
    }

    // 在 sibling 同级下创建一个简单按钮，紧靠 sibling 下方偏移 10px
    private static Button CreateButton(GameObject sibling, string goName)
    {
        var sibRt = sibling.GetComponent<RectTransform>();

        // 用 DefaultControls 创建标准 Button
        var res = new DefaultControls.Resources();
        var go = DefaultControls.CreateButton(res);
        go.name = goName;

        Undo.RegisterCreatedObjectUndo(go, "Create " + goName);
        go.transform.SetParent(sibling.transform.parent, false);

        // 对齐 sibling 的 anchor/pivot，紧靠其下方
        var rt = go.GetComponent<RectTransform>();
        rt.anchorMin = sibRt.anchorMin;
        rt.anchorMax = sibRt.anchorMax;
        rt.pivot = sibRt.pivot;
        rt.sizeDelta = new Vector2(Mathf.Max(sibRt.sizeDelta.x, 160), 48);

        float sibHalfH = sibRt.sizeDelta.y / 2f;
        float btnHalfH = rt.sizeDelta.y / 2f;
        rt.anchoredPosition = sibRt.anchoredPosition - new Vector2(0, sibHalfH + 10 + btnHalfH);

        // 设置按钮文字
        var label = go.GetComponentInChildren<Text>();
        if (label != null)
        {
            label.text = "获取公告(Mock)";
            label.fontSize = 14;
        }

        return go.GetComponent<Button>();
    }
}
