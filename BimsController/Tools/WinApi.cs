using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace BimsController.Tools
{
    public static class WinApi
    {
        private const int LB_GETCOUNT = 0x018B;
        private const int LB_SETCURSEL = 0x0186;
        private const int LB_GETTEXTLEN = 0x018A;
        private const int LB_GETTEXT = 0x0189;
        private const int LB_GETITEMDATA = 0x0199;
        private const int LB_FINDSTRING = 0x018F;
        private const int LB_FINDSTRINGEXACT = 0x01A2;
        const UInt32 WM_KEYDOWN = 0x0100;
        const UInt32 WM_KEYUP = 0x0101;

        private const UInt32 WM_CLOSE = 0x0010;

        [DllImport("user32.dll", CharSet = CharSet.Auto)]
        private static extern int SendMessage(IntPtr hwnd, int wMsg, IntPtr wParam, IntPtr lParam);

        [DllImport("user32.dll", CharSet = CharSet.Auto)]
        private static extern IntPtr SendMessage(IntPtr hWnd, UInt32 Msg, IntPtr wParam, IntPtr lParam);

        [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = false)]
        static extern IntPtr SendMessage(IntPtr hWnd, uint Msg, IntPtr wParam, StringBuilder lParam);

        [DllImport("user32.dll", SetLastError = true)]
        static extern IntPtr FindWindowEx(IntPtr hwndParent, IntPtr hwndChildAfter, string lpszClass, string lpszWindow);

        [DllImport("user32.dll", EntryPoint = "FindWindow")]
        private static extern IntPtr FindWindow(string lp1, string lp2);

        [DllImport("user32.dll")]
        static extern bool PostMessage(IntPtr hWnd, UInt32 Msg, int wParam, int lParam);

        public static void CloseWindow(IntPtr hwnd)
        {
            SendMessage(hwnd, WM_CLOSE, IntPtr.Zero, IntPtr.Zero);
        }

        public static List<string> GetPidsList()
        {
            IntPtr pidsHandle = FindWindow("WindowsForms10.Window.8.app.0.141b42a_r9_ad1", null);
            IntPtr pidsListHandle = FindWindowEx((IntPtr)pidsHandle, IntPtr.Zero, "WindowsForms10.LISTBOX.app.0.141b42a_r9_ad1", null);

            int countOfLines = (int)SendMessage(pidsListHandle, LB_GETCOUNT, IntPtr.Zero, IntPtr.Zero);

            List<string> listBoxContent = new List<string>();
            for (int i = 0; i < countOfLines; i++)
            {
                StringBuilder sb = new StringBuilder(256);
                IntPtr getText = SendMessage(pidsListHandle, LB_GETTEXT, (IntPtr)i, sb);
                listBoxContent.Add(sb.ToString());
            }
            return listBoxContent;

            //SendMessage(pidsListHandle, LB_SETCURSEL, (IntPtr)(1), IntPtr.Zero);
        }

        public static async Task EnterCredentials(IntPtr handle, string login, string password)
        {
            List<int> VKs = new List<int>();
            VKs.AddRange(VKsConverter.Convert(login));
            VKs.Add(VKsConverter.TAB_VK);
            VKs.AddRange(VKsConverter.Convert(password));

            for (int i = 0; i < VKs.Count(); i++)
            {
                if (login.IndexOf("@") == i)
                {
                    await Task.Delay(100);
                    PostMessage(handle, WM_KEYDOWN, VKsConverter.SHIFT_VK, 0);
                }

                await Task.Delay(100);
                PostMessage(handle, WM_KEYDOWN, VKs[i], 0);

                if (login.IndexOf("@") == i)
                {
                    await Task.Delay(100);
                    PostMessage(handle, WM_KEYUP, VKsConverter.SHIFT_VK, 0);
                }
            }

            await Task.Delay(5000);
            PostMessage(handle, WM_KEYDOWN, VKsConverter.ENTER_VK, 0);
            await Task.Delay(100);
            PostMessage(handle, WM_KEYUP, VKsConverter.ENTER_VK, 0);
            await Task.Delay(15000);
            PostMessage(handle, WM_KEYDOWN, VKsConverter.ENTER_VK, 0);
            await Task.Delay(100);
            PostMessage(handle, WM_KEYUP, VKsConverter.ENTER_VK, 0);
            await Task.Delay(1000);
        }

        public static async Task EnterString(IntPtr handle, string str)
        {
            List<int> VKs = VKsConverter.Convert(str);

            for (int i = 0; i < VKs.Count(); i++)
            {
                await Task.Delay(100);
                PostMessage(handle, WM_KEYDOWN, VKs[i], 0);
            }
        }
    }
}
