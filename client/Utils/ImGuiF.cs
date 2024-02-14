using ImGuiNET;

namespace client;

public static class ImGuiF
{
    public static void TextCentered(string text, float minIntend = 20f)
    {
        var winWidth = ImGui.GetWindowWidth();
        var textWidth = ImGui.CalcTextSize(text).X;

        var intend = MathF.Max(minIntend, winWidth - textWidth) / 2f;
        ImGui.SetCursorPosX(intend);
        ImGui.PushTextWrapPos(winWidth - intend);
        ImGui.TextWrapped(text);
        ImGui.PopTextWrapPos();
    }

    public static bool BeginPopupModal(string id, ref bool _is_open)
    {
        _is_open = ImGui.IsPopupOpen(id);
        return ImGui.BeginPopupModal(id, ref _is_open);
    }
}

