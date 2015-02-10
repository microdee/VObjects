#region usings
using System;
using System.ComponentModel.Composition;
using System.Diagnostics;
using System.Runtime.InteropServices; //added for keyboard closure
using System.Windows.Interop; //Keyboard closure - must add reference for WindowsBase

using VVVV.PluginInterfaces.V1;
using VVVV.PluginInterfaces.V2;
using VVVV.Utils.VColor;
using VVVV.Utils.VMath;

using VVVV.Core.Logging;
#endregion usings

namespace VVVV.Nodes
{
	#region PluginInfo
	[PluginInfo(Name = "LaunchOSK", Category = "Windows", AutoEvaluate = true)]
	#endregion PluginInfo
	public class WindowsLaunchOSKNode : IPluginEvaluate
	{
		#region fields & pins
		[Input("Open")]
		public ISpread<bool> FOpen;
		[Input("Close")]
		public ISpread<bool> FClose;

		[Output("Handle")]
		public ISpread<int> FHandle;
		
		[Import()]
		public ILogger FLogger;
		#endregion fields & pins

		//Added for keyboard closure
        [return: MarshalAs(UnmanagedType.Bool)]
        [DllImport("user32.dll", SetLastError = true)]
        public static extern bool PostMessage(int hWnd, uint Msg, int wParam, int lParam);

        [DllImport("user32.dll")]
        public static extern IntPtr FindWindow(String sClassName, String sAppName);
		
		//open keyboard
		void openKeyboard()
		{
            ProcessStartInfo startInfo = new ProcessStartInfo(@"C:\Program Files\Common Files\Microsoft Shared\ink\TabTip.exe");
            startInfo.WindowStyle = ProcessWindowStyle.Hidden;
            Process.Start(startInfo);
		}

		//close keyboard
		void closeKeyboard()
		{
		uint WM_SYSCOMMAND = 274;
            uint SC_CLOSE = 61536;
            IntPtr KeyboardWnd = FindWindow("IPTip_Main_Window", null);
            PostMessage(KeyboardWnd.ToInt32(), WM_SYSCOMMAND, (int)SC_CLOSE, 0);
		}

		//called when data for any output pin is requested
		public void Evaluate(int SpreadMax)
		{
			if(FOpen[0])
				openKeyboard();
			if(FClose[0])
				closeKeyboard();
            FHandle[0] = FindWindow("IPTip_Main_Window", null).ToInt32();

			//FLogger.Log(LogType.Debug, "hi tty!");
		}
	}
}
