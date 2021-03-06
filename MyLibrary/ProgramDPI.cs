﻿using System;
using System.Windows.Forms;

namespace MyLibrary
{
	static class ProgramDPI
	{
		///	解決DPI顯示問題 (1) 設定 Form1.AutoScaleMode = DPI
		///	
		
		///	解決DPI顯示問題 (2)
		///	
		[System.Runtime.InteropServices.DllImport("user32.dll")]
		private static extern bool SetProcessDPIAware();
		

		/// <summary>
		/// 應用程式的主要進入點。
		/// </summary>
		[STAThread]
		static void Main()
		{
			///	解決DPI顯示問題 (3)
			///	
			if (System.Environment.OSVersion.Version.Major >= 6)
				SetProcessDPIAware(); 
			
			Application.EnableVisualStyles();
			Application.SetCompatibleTextRenderingDefault(false);
			Application.Run(new MyLibrary.Forms.AutoResizeControlsForm());
		}
	}
}
