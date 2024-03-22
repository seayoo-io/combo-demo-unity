using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.UI;

internal class ShareOptionViewModel {
    public string title;
    public string content;
    public List<string> imageUrls;
}

[ViewPrefab("Prefabs/ShareOptionView")]
internal class ShareOptionView : View<ShareOptionView>
{
    public Button submitBtn;
    public Button cancelBtn;
    private Action<ShareOptionViewModel> OnSubmit;
    private Action OnCancel;
    private bool allowMultiImage = false;

    public Text titleLabel;
    public Text contentLabel;

    public InputField titleInput;
    public InputField contentInput;
    public Dropdown imageTypeInput;
    public InputField imageCountInput;

    void Start() {
        imageCountInput.onValidateInput += delegate(string input, int charIndex, char addedChar) 
        {
            return ValidateNumberInput(input, addedChar);
        };

        submitBtn.onClick.AddListener(()=>{
            var imageUrls = new List<string>();
            int.TryParse(imageCountInput.text, out var count);
            for (var i = 0; i < count; i++) {
                imageUrls.Add(GenerateImagePath(i));
            }
            OnSubmit?.Invoke(new ShareOptionViewModel {
                title = string.IsNullOrEmpty(titleInput.text) ? null : titleInput.text,
                content = string.IsNullOrEmpty(contentInput.text) ? null : contentInput.text,
                imageUrls = imageUrls
            });
        });
        cancelBtn.onClick.AddListener(()=>{
            OnCancel?.Invoke();
        });
    }

    void OnDestroy() {
        submitBtn.onClick.RemoveAllListeners();
        cancelBtn.onClick.RemoveAllListeners();
    }

    public void SetTitleLabel(string val) => titleLabel.text = val;
    public void SetContentLabel(string val) => contentLabel.text = val;

    public void SetSubmitCallback(Action<ShareOptionViewModel> OnSubmit) => this.OnSubmit = OnSubmit;
    public void SetCancelCallback(Action OnCancel) => this.OnCancel = OnCancel;

    public void SetMultiImageEnabled(bool enable) => allowMultiImage = enable;
    public void SetImageTypeEnabled(bool enable) => imageTypeInput.enabled = enable;

    protected override IEnumerator OnHide()
    {
        yield return null;
    }

    protected override IEnumerator OnShow()
    {
        yield return null;
    }

    private string GenerateImagePath(int index) {
        if (imageTypeInput.value == 0) {
            var bytes = ScreenCapture.CaptureScreenshotAsTexture().EncodeToPNG();
            var path = Path.Combine(Application.temporaryCachePath, $"share_{index}.png");
            File.WriteAllBytes(path, bytes);
            return path;
        } else {
            return "https://cn.bing.com/th?id=OHR.CERNCenter_EN-US9854867489_1920x1080.jpg";
        }
    }

    char ValidateNumberInput(string input, char addedChar)
    {
        // 只允许输入数字
        if (!char.IsDigit(addedChar))
        {
            return '\0';
        }

        var proposedInput = input + addedChar;
        var proposedValue = int.Parse(proposedInput);

        if (!allowMultiImage) {
            if (proposedValue != 0 && proposedValue != 1) {
                return '\0';
            }
        }
        else if (proposedValue < 0 || proposedValue > 20)
        {
            return '\0';
        }

        return addedChar;
    }
}