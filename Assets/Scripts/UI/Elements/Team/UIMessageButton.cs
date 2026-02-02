using Cysharp.Threading.Tasks;
using SonatFramework.Scripts.UIModule;
using SonatFramework.Systems;
using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

public class UIMessageButton : MonoBehaviour
{
    [Header("Filter Options")]
    [TextArea]
    [Tooltip("ASCII punctuation được phép. Chỉnh trong Inspector nếu cần.")]
    public string allowedPunctuation = " .,!?:;'-\"()[]{}<>/@#&*_+=%~^|\\`$";
    public bool allowSpaces = true;
    public bool allowDigits = false;   // Yêu cầu: chỉ chữ + dấu câu -> để false
    public bool allowNewLine = false;  // Bật nếu muốn cho phép Enter
    public int maxLength = 0;          // 0 = no limit

    private HashSet<char> punctSet;
    private TouchScreenKeyboard keyboard;
    private string committedText = "";   // Chuỗi đã hiển thị/commit
    private string pendingSanitized = ""; // Chuỗi tạm trong lúc gõ (không hiển thị)

    private void Awake()
    {
        BuildPunctSet();
    }

    private void BuildPunctSet()
    {
        punctSet = new HashSet<char>();
        if (!string.IsNullOrEmpty(allowedPunctuation))
        {
            foreach (var c in allowedPunctuation)
                punctSet.Add(c);
        }
        if (allowSpaces) punctSet.Add(' ');
        if (allowNewLine) punctSet.Add('\n');
    }

    public void OnClick()
    {
#if UNITY_IOS || UNITY_ANDROID
        keyboard = TouchScreenKeyboard.Open(
            committedText,          // mở với nội dung đã commit (nếu muốn sửa tiếp)
            TouchScreenKeyboardType.Default,
            false,                  // autocorrect off
            allowNewLine,
            false,                  // secure
            false                   // alert
        );
        pendingSanitized = committedText;
#else
        Debug.LogWarning("TouchScreenKeyboard works on device only. Build to iOS/Android to test.");
#endif
    }

    private void Update()
    {
#if UNITY_IOS || UNITY_ANDROID
        if (keyboard == null) return;

        // Người dùng gõ -> sanitize trong nền, nhưng KHÔNG cập nhật UI
        string raw = keyboard.text ?? "";
        string sanitized = Sanitize(raw);

        // Ghi đè lại lên keyboard để loại ngay ký tự không hợp lệ trên UI của bàn phím
        if (sanitized != raw)
            keyboard.text = sanitized;

        pendingSanitized = sanitized;

        // Khi nhấn Done -> mới commit ra màn hình
        if (keyboard.status == TouchScreenKeyboard.Status.Done)
        {
            SendMessageAsync().Forget();
            keyboard = null;
        }
        else if (keyboard.status == TouchScreenKeyboard.Status.Canceled)
        {
            // Không commit
            keyboard = null;
        }
#endif
    }

    private async UniTaskVoid SendMessageAsync()
    {
        committedText = pendingSanitized ?? "";

        if (string.IsNullOrEmpty(committedText)) return;

        bool isProcessing = true;

        UIData data = new();
        data.Add("condition", new Func<bool>(() => !isProcessing));
        PanelManager.Instance.OpenForget<PopupProcessing>(data);

        var response = await Service<TeamService>.Get().SendMessage(committedText, TeamMessageType.text);

        isProcessing = false;

        if (response.code != ResponseCode.SUCCESS)
            PopupToast.Cretate(response.message);
        else
            committedText = "";
    }

    private string Sanitize(string input)
    {
        if (string.IsNullOrEmpty(input)) return "";
        var sb = new StringBuilder(input.Length);
        foreach (char c in input)
        {
            if (IsAllowed(c))
                sb.Append(c);
        }
        string result = sb.ToString();
        if (maxLength > 0 && result.Length > maxLength)
            result = result.Substring(0, maxLength);
        return result;
    }

    private bool IsAllowed(char c)
    {
        // Cho phép chữ cái tiếng Anh (ASCII)
        if ((c >= 'A' && c <= 'Z') || (c >= 'a' && c <= 'z'))
            return true;

        // (Tuỳ chọn) Cho phép chữ số
        if (allowDigits && (c >= '0' && c <= '9'))
            return true;

        // Cho phép dấu câu/space theo cấu hình
        if (punctSet != null && punctSet.Contains(c))
            return true;

        // Mặc định: không cho (emoji, ký tự có dấu, ký tự non-ASCII...)
        return false;
    }
}
