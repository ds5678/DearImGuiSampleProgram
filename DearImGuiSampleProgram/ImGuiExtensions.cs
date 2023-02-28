using DearImGui;

namespace DearImGuiSampleProgram;

internal static class ImGuiExtensions
{
	public static ImDrawList[] GetCmdLists(this ImDrawData data)
	{
		ImDrawList[]? list = null;
		data.GetCmdLists(ref list);
		return list ?? Array.Empty<ImDrawList>();
	}
}
