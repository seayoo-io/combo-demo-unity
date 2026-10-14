using System;
using System.Collections;
using System.Collections.Generic;
using Combo;
using UnityEngine;
using UnityEngine.UI;

[ViewPrefab("Prefabs/ComplainParameterView")]
internal class ComplainParameterView : View<ComplainParameterView>
{
    public Dropdown targetDropdown;
    public InputField targetIdInput;
    public InputField targetNameInput;
    public Text statusText;
    public Button retryButton;
    public Button addContextButton;
    public Button submitButton;
    public Button cancelButton;
    public Transform contextRoot;
    public ComplainContextRow contextRowTemplate;

    // SDK 的单次回调尚未返回时，关闭或重新打开表单也不能再次调用。
    private static bool submitting;
    private static event Action SubmissionChanged;
    private readonly List<ComplainContextRow> rows = new List<ComplainContextRow>();
    private ComplainTargetInfo[] targets = new ComplainTargetInfo[0];
    private bool loading;
    private bool closed;
    private int requestVersion;

    private void Awake()
    {
        retryButton.onClick.AddListener(LoadTargets);
        addContextButton.onClick.AddListener(AddContextRow);
        submitButton.onClick.AddListener(Submit);
        cancelButton.onClick.AddListener(Close);
        SubmissionChanged += UpdateSubmitState;
    }

    private void OnDestroy()
    {
        closed = true;
        SubmissionChanged -= UpdateSubmitState;
        retryButton.onClick.RemoveListener(LoadTargets);
        addContextButton.onClick.RemoveListener(AddContextRow);
        submitButton.onClick.RemoveListener(Submit);
        cancelButton.onClick.RemoveListener(Close);
    }

    private void LoadTargets()
    {
        if (loading || closed) return;
        loading = true;
        targets = new ComplainTargetInfo[0];
        targetDropdown.ClearOptions();
        targetDropdown.interactable = false;
        statusText.text = "正在获取举报类型…";
        retryButton.gameObject.SetActive(false);
        UpdateSubmitState();
        var version = ++requestVersion;
        GameClient.GetComplainTargets(result =>
        {
            if (this == null || closed || version != requestVersion) return;
            loading = false;
            targets = result;
            var names = new List<string>();
            foreach (var target in targets) names.Add(target.displayName);
            targetDropdown.AddOptions(names);
            targetDropdown.value = 0;
            targetDropdown.RefreshShownValue();
            targetDropdown.interactable = targets.Length > 0;
            statusText.text = targets.Length == 0 ? "当前游戏未配置举报类型" : "";
            retryButton.gameObject.SetActive(targets.Length == 0);
            UpdateSubmitState();
        }, message =>
        {
            if (this == null || closed || version != requestVersion) return;
            loading = false;
            statusText.text = message;
            retryButton.gameObject.SetActive(true);
            UpdateSubmitState();
        });
    }

    private void AddContextRow()
    {
        var row = UnityEngine.Object.Instantiate(contextRowTemplate, contextRoot);
        row.Initialize(RemoveContextRow);
        row.gameObject.SetActive(true);
        rows.Add(row);
    }

    private void RemoveContextRow(ComplainContextRow row)
    {
        rows.Remove(row);
        row.gameObject.SetActive(false);
        UnityEngine.Object.Destroy(row.gameObject);
    }

    private void Submit()
    {
        if (submitting || loading || targets.Length == 0 || closed)
        {
            Log.I("[Complain] submit rejected, form not ready or SDK call pending");
            return;
        }
        var data = new ComplainFormData { TargetId = targetIdInput.text, TargetName = targetNameInput.text };
        foreach (var row in rows) data.Context.Add(row.ReadEntry());
        string context;
        string error;
        if (!data.TryBuildContext(out context, out error))
        {
            Log.I($"[Complain] validation failed: {error}");
            Toast.Show(error);
            return;
        }
        var player = PlayerController.GetPlayer();
        var role = player == null ? null : PlayerController.GetRoleInfo(player);
        if (role == null)
        {
            Log.E("[Complain] submit rejected, no active role");
            Toast.Show("未找到当前角色，请重新进入游戏");
            return;
        }
        var options = new ComplainOptions
        {
            TargetType = targets[targetDropdown.value].targetType,
            TargetId = data.TargetId.Trim(),
            TargetName = data.TargetName.Trim(),
            Context = context,
            ServerId = role.serverId.ToString(),
            RoleId = role.roleId,
            RoleName = role.roleName,
            Width = 100,
            Height = 100
        };
        submitting = true;
        SubmissionChanged?.Invoke();
        Log.I($"[Complain] SDK call started, context_count={rows.Count}");
        try
        {
            ComboSDK.Complain(options, result =>
            {
                submitting = false;
                SubmissionChanged?.Invoke();
                if (result.IsSuccess)
                {
                    Log.I("[Complain] SDK flow finished");
                    Toast.Show("举报流程结束");
                    if (this != null && !closed) Close();
                }
                else
                {
                    Log.E($"[Complain] SDK call failed, code={result.Error.Code}");
                    Toast.Show($"举报界面打开失败：{result.Error.Message}");
                }
            });
        }
        catch (Exception ex)
        {
            submitting = false;
            SubmissionChanged?.Invoke();
            Log.E($"[Complain] SDK call threw {ex.GetType().Name}");
            Toast.Show("举报界面打开失败，请重试");
        }
    }

    private void UpdateSubmitState()
    {
        if (closed) return;
        submitButton.interactable = !loading && !submitting && targets.Length > 0;
        submitButton.GetComponentInChildren<Text>().text = submitting ? "举报流程进行中…" : "提交";
    }

    private void Close()
    {
        closed = true;
        requestVersion++;
        Destroy();
    }

    protected override IEnumerator OnShow()
    {
        Log.I("[Complain] form opened");
        LoadTargets();
        yield break;
    }

    protected override IEnumerator OnHide()
    {
        yield break;
    }
}
