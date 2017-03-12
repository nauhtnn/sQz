using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Win32;

namespace sQzLib
{
    public class HotkeyHandler
    {
        public static void DisableTaskManager(ref UICbMsg cbMsg)
        {
            RegistryKey regkey = default(RegistryKey);
            string keyValueInt = "1";
            string subKey = "Software\\Microsoft\\Windows\\CurrentVersion\\Policies\\System";
            try
            {
                regkey = Registry.CurrentUser.CreateSubKey(subKey);
                regkey.SetValue("DisableTaskMgr", keyValueInt);
                regkey.Close();
            }
            catch (UnauthorizedAccessException ex)
            {
                cbMsg += ex.Message;
            }
        }

        public static void EnableTaskManager(ref UICbMsg cbMsg)
        {
            RegistryKey regkey = default(RegistryKey);
            string keyValueInt = "0";
            //0x00000000 (0)
            string subKey = "Software\\Microsoft\\Windows\\CurrentVersion\\Policies\\System";
            try
            {
                regkey = Registry.CurrentUser.CreateSubKey(subKey);
                regkey.SetValue("DisableTaskMgr", keyValueInt);
                regkey.Close();
            }
            catch (Exception ex)
            {
                cbMsg += ex.Message;
            }

        }
    }
}
