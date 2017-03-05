using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NetFwTypeLib;
using System.Collections;
using System.Windows;

namespace sQzLib
{
    public class FirewallHandler
    {
        private int portSrvr0 = 23820;
        private int portSrvr1 = 23821;
        private string portNameSrvr0 = "sQz Server 0";
        private string portNameSrvr1 = "sQz Server 1";
        private int[] portsSocket;
        private string[] portsName;
        private INetFwProfile fwProfile = null;

        public FirewallHandler(int srvrType)
        {
            if (srvrType == 0)
            {
                portsSocket = new int[1];
                portsSocket[0] = portSrvr0;
                portsName = new string[1];
                portsName[0] = portNameSrvr0;
            }
            else if (srvrType == 1)
            {
                portsSocket = new int[2];
                portsSocket[0] = portSrvr0;
                portsSocket[1] = portSrvr1;
                portsName = new string[2];
                portsName[0] = portNameSrvr0;
                portsName[1] = portNameSrvr1;
            }
            else
            {
                portsSocket = new int[1];
                portsSocket[0] = portSrvr1;
                portsName = new string[1];
                portsName[0] = portNameSrvr1;
            }
        }

        public string OpenFirewall()
        {
            INetFwAuthorizedApplications authApps = null;
            INetFwAuthorizedApplication authApp = null;
            INetFwOpenPorts openPorts = null;
            INetFwOpenPort openPort = null;
            string retMsg = null;
            try
            {
                //string productName = Application.Current.MainWindow.GetType().Assembly.GetName().Name;
                string productName = System.Reflection.Assembly.GetExecutingAssembly().GetName().Name + "Server";
                string exePath = System.Reflection.Assembly.GetExecutingAssembly().Location;
                if (isAppFound(productName) == false)
                {
                    SetProfile();
                    authApps = fwProfile.AuthorizedApplications;
                    authApp = GetInstance("INetAuthApp") as INetFwAuthorizedApplication;
                    authApp.Name = productName;
                    authApp.ProcessImageFileName = exePath;
                    authApps.Add(authApp);
                }

                if (isPortFound(portsSocket[0]) == false)
                {
                    SetProfile();
                    openPorts = fwProfile.GloballyOpenPorts;
                    openPort = GetInstance("INetOpenPort") as INetFwOpenPort;
                    openPort.Port = portsSocket[0];
                    openPort.Protocol = NET_FW_IP_PROTOCOL_.NET_FW_IP_PROTOCOL_TCP;
                    openPort.Name = portsName[0];
                    openPorts.Add(openPort);
                }

                if (portsSocket.Length == 2 && isPortFound(portsSocket[1]) == false)
                {
                    SetProfile();
                    openPorts = fwProfile.GloballyOpenPorts;
                    openPort = GetInstance("INetOpenPort") as INetFwOpenPort;
                    openPort.Port = portsSocket[1];
                    openPort.Protocol = NET_FW_IP_PROTOCOL_.NET_FW_IP_PROTOCOL_TCP;
                    openPort.Name = portsName[1];
                    openPorts.Add(openPort);
                }
                retMsg = "Open firewall successfully, app name = " + productName
                    + ", port1 name = " + portsName[0] + ", port1 = " + portsSocket[0];
                if(portsSocket.Length == 2)
                    retMsg += "port2 name = " + portsName[1] + ", port2 = " + portsSocket[1];
                retMsg += ".\n";
            }
            catch (Exception ex)
            {
                //MessageBox.Show(ex.Message);
                retMsg = "Fw Error with the message = " + ex.Message;
            }
            finally
            {
                if (authApps != null) authApps = null;
                if (authApp != null) authApp = null;
                if (openPorts != null) openPorts = null;
                if (openPort != null) openPort = null;
            }
            return retMsg;
        }

        protected internal void CloseFirewall()
        {
            INetFwAuthorizedApplications apps = null;
            INetFwOpenPorts ports = null;
            try
            {
                string exePath = System.Reflection.Assembly.GetExecutingAssembly().Location;
                string productName = System.Reflection.Assembly.GetExecutingAssembly().GetName().Name;
                if (isAppFound(productName + " Server") == true)
                {
                    SetProfile();
                    apps = fwProfile.AuthorizedApplications;
                    apps.Remove(exePath);
                }

                if (isPortFound(portsSocket[0]) == true)
                {
                    SetProfile();
                    ports = fwProfile.GloballyOpenPorts;
                    ports.Remove(portsSocket[0], NET_FW_IP_PROTOCOL_.NET_FW_IP_PROTOCOL_TCP);
                }

                if (isPortFound(portsSocket[1]) == true)
                {
                    SetProfile();
                    ports = fwProfile.GloballyOpenPorts;
                    ports.Remove(portsSocket[1], NET_FW_IP_PROTOCOL_.NET_FW_IP_PROTOCOL_TCP);
                }
            }
            catch (Exception ex)
            {
                string byteMsg = ex.Message;
            }
            finally
            {
                if (apps != null) apps = null;
                if (ports != null) ports = null;
            }
        }

        protected internal bool isAppFound(string appName)
        {
            bool boolResult = false;
            Type progID = null;
            INetFwMgr firewall = null;
            INetFwAuthorizedApplications apps = null;
            INetFwAuthorizedApplication app = null;
            try
            {
                progID = Type.GetTypeFromProgID("HNetCfg.FwMgr");
                firewall = Activator.CreateInstance(progID) as INetFwMgr;
                if (firewall.LocalPolicy.CurrentProfile.FirewallEnabled)
                {
                    apps = firewall.LocalPolicy.CurrentProfile.AuthorizedApplications;
                    IEnumerator appEnumerate = apps.GetEnumerator();
                    while ((appEnumerate.MoveNext()))
                    {
                        app = appEnumerate.Current as INetFwAuthorizedApplication;
                        if (app.Name == appName)
                        {
                            boolResult = true;
                            break;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                string byteMsg = ex.Message;
            }
            finally
            {
                if (progID != null) progID = null;
                if (firewall != null) firewall = null;
                if (apps != null) apps = null;
                if (app != null) app = null;
            }
            return boolResult;
        }

        protected internal bool isPortFound(int portNumber)
        {
            bool boolResult = false;
            INetFwOpenPorts ports = null;
            Type progID = null;
            INetFwMgr firewall = null;
            INetFwOpenPort currentPort = null;
            try
            {
                progID = Type.GetTypeFromProgID("HNetCfg.FwMgr");
                firewall = Activator.CreateInstance(progID) as INetFwMgr;
                ports = firewall.LocalPolicy.CurrentProfile.GloballyOpenPorts;
                IEnumerator portEnumerate = ports.GetEnumerator();
                while ((portEnumerate.MoveNext()))
                {
                    currentPort = portEnumerate.Current as INetFwOpenPort;
                    if (currentPort.Port == portNumber)
                    {
                        boolResult = true;
                        break;
                    }
                }
            }
            catch (Exception ex)
            {
                //MessageBox.Show(ex.Message);
                Console.Write(ex.Message);
            }
            finally
            {
                if (ports != null) ports = null;
                if (progID != null) progID = null;
                if (firewall != null) firewall = null;
                if (currentPort != null) currentPort = null;
            }
            return boolResult;
        }

        protected internal void SetProfile()
        {
            INetFwMgr fwMgr = null;
            INetFwPolicy fwPolicy = null;
            try
            {
                fwMgr = GetInstance("INetFwMgr") as INetFwMgr;
                fwPolicy = fwMgr.LocalPolicy;
                fwProfile = fwPolicy.CurrentProfile;
            }
            catch (Exception ex)
            {
                //MessageBox.Show(ex.Message);
                Console.Write(ex.Message);
            }
            finally
            {
                if (fwMgr != null) fwMgr = null;
                if (fwPolicy != null) fwPolicy = null;
            }
        }

        protected internal object GetInstance(string typeName)
        {
            Type tpResult = null;
            switch (typeName)
            {
                case "INetFwMgr":
                    tpResult = Type.GetTypeFromCLSID(new Guid("{304CE942-6E39-40D8-943A-B913C40C9CD4}"));
                    return Activator.CreateInstance(tpResult);
                case "INetAuthApp":
                    tpResult = Type.GetTypeFromCLSID(new Guid("{EC9846B3-2762-4A6B-A214-6ACB603462D2}"));
                    return Activator.CreateInstance(tpResult);
                case "INetOpenPort":
                    tpResult = Type.GetTypeFromCLSID(new Guid("{0CA545C6-37AD-4A6C-BF92-9F7610067EF5}"));
                    return Activator.CreateInstance(tpResult);
                default:
                    return null;
            }
        }
    }
}
