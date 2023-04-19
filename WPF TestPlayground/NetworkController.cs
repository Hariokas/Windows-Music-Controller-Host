using System;
using System.Net;
using System.Net.NetworkInformation;
using System.Net.Sockets;

namespace WPF_TestPlayground;

public static class NetworkController
{
    public static string GetLocalIPAddress()
    {
        return "192.168.0.106";
        if (!NetworkInterface.GetIsNetworkAvailable())
            throw new Exception("No network is available");

        var host = Dns.GetHostEntry(Dns.GetHostName());

        foreach (var ip in host.AddressList)
            if (ip.AddressFamily == AddressFamily.InterNetwork)
                return ip.ToString();

        throw new Exception("No network adapters with an IPv4 address in the system!");
    }
}