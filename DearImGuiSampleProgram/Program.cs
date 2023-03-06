using DearImGui;
using System.Diagnostics;
using System.Numerics;
using System.Runtime.InteropServices;
using Veldrid;
using Veldrid.Sdl2;
using Veldrid.StartupUtilities;

namespace DearImGuiSampleProgram;

internal static class Program
{
	private static readonly Sdl2Window _window;
	private static readonly GraphicsDevice _gd;
	private static readonly CommandList _cl;
	private static readonly ImGuiController _controller;

	// UI state
	private static float _f = 0.0f;
	private static int _counter = 0;
	private static int _dragInt = 0;
	private static Vector3 _clearColor = new Vector3(0.45f, 0.55f, 0.6f);
	private static Span<float> ClearColorSpan => MemoryMarshal.Cast<Vector3, float>(MemoryMarshal.CreateSpan(ref _clearColor, 1));
	private static bool _showImGuiDemoWindow = true;
	private static bool _showAnotherWindow = false;
	private static readonly bool[] basic_opened = { true, true, true };
	private static readonly bool[] advanced_opened = { true, true, true, true };
	private static uint advanced_tab_bar_flags = (uint)ImGuiTabBarFlags.Reorderable;

	static Program()
	{
		// Create window, GraphicsDevice, and all resources necessary for the demo.
		VeldridStartup.CreateWindowAndGraphicsDevice(
			new WindowCreateInfo(50, 50, 1280, 720, WindowState.Normal, "DearImGui Sample Program"),
			new GraphicsDeviceOptions(true, null, true, ResourceBindingModel.Improved, true, true),
			out _window,
			out _gd);

		_cl = _gd.ResourceFactory.CreateCommandList();
		_controller = new ImGuiController(_gd, _gd.MainSwapchain.Framebuffer.OutputDescription, _window.Width, _window.Height);

		_window.Resized += () =>
		{
			_gd.MainSwapchain.Resize((uint)_window.Width, (uint)_window.Height);
			_controller.WindowResized(_window.Width, _window.Height);
		};
	}

	static void Main()
	{
		var stopwatch = Stopwatch.StartNew();
		// Main application loop
		while (_window.Exists)
		{
			float deltaTime = stopwatch.ElapsedTicks / (float)Stopwatch.Frequency;
			stopwatch.Restart();
			InputSnapshot snapshot = _window.PumpEvents();
			if (!_window.Exists) { break; }
			_controller.Update(deltaTime, snapshot); // Feed the input events to our ImGui controller, which passes them through to ImGui.

			SubmitUI();

			_cl.Begin();
			_cl.SetFramebuffer(_gd.MainSwapchain.Framebuffer);
			_cl.ClearColorTarget(0, new RgbaFloat(_clearColor.X, _clearColor.Y, _clearColor.Z, 1f));
			_controller.Render(_gd, _cl);
			_cl.End();
			_gd.SubmitCommands(_cl);
			_gd.SwapBuffers(_gd.MainSwapchain);
		}

		// Clean up Veldrid resources
		_gd.WaitForIdle();
		_controller.Dispose();
		_cl.Dispose();
		_gd.Dispose();
	}

	private static void SubmitUI()
	{
		// Demo code adapted from the official Dear ImGui demo program:
		// https://github.com/ocornut/imgui/blob/master/examples/example_win32_directx11/main.cpp#L172

		// 1. Show a simple window.
		// Tip: if we don't call ImGui.BeginWindow()/ImGui.EndWindow() the widgets automatically appears in a window called "Debug".
		{
			ImGui.Text("Hello, world!");                                        // Display some text (you can use a format string too)
			ImGui.SliderFloat("float", ref _f, 0, 1, _f.ToString("0.000"));  // Edit 1 float using a slider from 0.0f to 1.0f
			ImGui.ColorEdit3("clear color", ClearColorSpan);                   // Edit 3 floats representing a color

			ImGui.Text($"Mouse position: {ImGui.GetMousePos()}");

			ImGui.Checkbox("ImGui Demo Window", ref _showImGuiDemoWindow);                 // Edit bools storing our windows open/close state
			ImGui.Checkbox("Another Window", ref _showAnotherWindow);
			if (ImGui.Button("Button"))                                         // Buttons return true when clicked (NB: most widgets return true when edited/activated)
			{
				_counter++;
			}

			ImGui.SameLine(0, -1);
			ImGui.Text($"counter = {_counter}");

			ImGui.DragInt("Draggable Int", ref _dragInt);

			float framerate = ImGui.GetIO().Framerate;
			ImGui.Text($"Application average {1000.0f / framerate:0.##} ms/frame ({framerate:0.#} FPS)");
		}

		// 2. Show another simple window. In most cases you will use an explicit Begin/End pair to name your windows.
		if (_showAnotherWindow)
		{
			ImGui.Begin("Another Window", ref _showAnotherWindow);
			ImGui.Text("Hello from another window!");
			if (ImGui.Button("Close Me"))
			{
				_showAnotherWindow = false;
			}

			ImGui.End();
		}

		// 3. Show the ImGui demo window. Most of the sample code is in ImGui.ShowDemoWindow(). Read its code to learn more about Dear ImGui!
		if (_showImGuiDemoWindow)
		{
			// Normally user code doesn't need/want to call this because positions are saved in .ini file anyway.
			// Here we just want to make the demo initial state a bit more friendly!
			ImGui.SetNextWindowPos(new Vector2(650, 20), ImGuiCond.FirstUseEver);
			ImGui.ShowDemoWindow(ref _showImGuiDemoWindow);
		}

		if (ImGui.TreeNode("Tabs"))
		{
			if (ImGui.TreeNode("Basic"))
			{
				if (ImGui.BeginTabBar("MyTabBar"))
				{
					if (ImGui.BeginTabItem("Avocado", ref basic_opened[0]))
					{
						ImGui.Text("This is the Avocado tab!\nblah blah blah blah blah");
						ImGui.EndTabItem();
					}
					if (ImGui.BeginTabItem("Broccoli", ref basic_opened[1]))
					{
						ImGui.Text("This is the Broccoli tab!\nblah blah blah blah blah");
						ImGui.EndTabItem();
					}
					if (ImGui.BeginTabItem("Cucumber", ref basic_opened[2]))
					{
						ImGui.Text("This is the Cucumber tab!\nblah blah blah blah blah");
						ImGui.EndTabItem();
					}
					ImGui.EndTabBar();
				}
				ImGui.Separator();
				ImGui.TreePop();
			}

			if (ImGui.TreeNode("Advanced & Close Button"))
			{
				// Expose a couple of the available flags. In most cases you may just call BeginTabBar() with no flags (0).
				ImGui.CheckboxFlags("ImGuiTabBarFlags_Reorderable", ref advanced_tab_bar_flags, (uint)ImGuiTabBarFlags.Reorderable);
				ImGui.CheckboxFlags("ImGuiTabBarFlags_AutoSelectNewTabs", ref advanced_tab_bar_flags, (uint)ImGuiTabBarFlags.AutoSelectNewTabs);
				ImGui.CheckboxFlags("ImGuiTabBarFlags_NoCloseWithMiddleMouseButton", ref advanced_tab_bar_flags, (uint)ImGuiTabBarFlags.NoCloseWithMiddleMouseButton);
				if ((advanced_tab_bar_flags & (uint)ImGuiTabBarFlags.FittingPolicyMask) == 0)
				{
					advanced_tab_bar_flags |= (uint)ImGuiTabBarFlags.FittingPolicyDefault;
				}

				if (ImGui.CheckboxFlags("ImGuiTabBarFlags_FittingPolicyResizeDown", ref advanced_tab_bar_flags, (uint)ImGuiTabBarFlags.FittingPolicyResizeDown))
				{
					advanced_tab_bar_flags &= ~((uint)ImGuiTabBarFlags.FittingPolicyMask ^ (uint)ImGuiTabBarFlags.FittingPolicyResizeDown);
				}

				if (ImGui.CheckboxFlags("ImGuiTabBarFlags_FittingPolicyScroll", ref advanced_tab_bar_flags, (uint)ImGuiTabBarFlags.FittingPolicyScroll))
				{
					advanced_tab_bar_flags &= ~((uint)ImGuiTabBarFlags.FittingPolicyMask ^ (uint)ImGuiTabBarFlags.FittingPolicyScroll);
				}

				// Tab Bar
				string[] names = { "Artichoke", "Beetroot", "Celery", "Daikon" };

				for (int n = 0; n < advanced_opened.Length; n++)
				{
					if (n > 0) { ImGui.SameLine(); }
					ImGui.Checkbox(names[n], ref advanced_opened[n]);
				}

				// Passing a ref bool to BeginTabItem() is similar to passing one to Begin(): the underlying bool will be set to false when the tab is closed.
				if (ImGui.BeginTabBar("MyTabBar", (ImGuiTabBarFlags)advanced_tab_bar_flags))
				{
					for (int n = 0; n < advanced_opened.Length; n++)
					{
						if (advanced_opened[n] && ImGui.BeginTabItem(names[n], ref advanced_opened[n]))
						{
							ImGui.Text($"This is the {names[n]} tab!");
							if ((n & 1) != 0)
							{
								ImGui.Text("I am an odd tab.");
							}

							ImGui.EndTabItem();
						}
					}

					ImGui.EndTabBar();
				}
				ImGui.Separator();
				ImGui.TreePop();
			}
			ImGui.TreePop();
		}

		ImGui.GetIO().DeltaTime = 2f;
	}
}