using System;
using System.Diagnostics;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Security.Permissions;
using System.Threading;
using System.Threading.Tasks;
using UnityDebug = UnityEngine.Debug;

//
    // Summary:
    //     Represents a 2-tuple, or pair.
    //
    // Type parameters:
    //   T1:
    //     The type of the tuple's first component.
    //
    //   T2:
    //     The type of the tuple's second component.
public class Tuple<T1, T2>
{
    //
    // Summary:
    //     Initializes a new instance of the System.Tuple`2 class.
    //
    // Parameters:
    //   item1:
    //     The value of the tuple's first component.
    //
    //   item2:
    //     The value of the tuple's second component.
    public Tuple(T1 item1, T2 item2)
    {
        Item1 = item1;
        Item2 = item2;
    }

    //
    // Summary:
    //     Gets the value of the current System.Tuple`2 object's first component.
    //
    // Returns:
    //     The value of the current System.Tuple`2 object's first component.
    public T1 Item1 { get; }
    //
    // Summary:
    //     Gets the value of the current System.Tuple`2 object's second component.
    //
    // Returns:
    //     The value of the current System.Tuple`2 object's second component.
    public T2 Item2 { get; }
}

public class EventArgs<T> : EventArgs
{
    public T Item { get; set; }
}

public static class TcpAsyncTools
{

    [HostProtection(SecurityAction.LinkDemand, ExternalThreading = true)]
    public static Task ConnectAsync(this TcpClient that, string host, int port)
        => Task.Factory.FromAsync(that.BeginConnect, that.EndConnect, host, port, null);

    public static Task WriteAsync(this NetworkStream that, byte[] buffer, int offset, int count) => Task.Factory.FromAsync(that.BeginWrite, that.EndWrite, buffer, offset, count, null);


    public static Task<int> ReadAsync(this NetworkStream that, byte[] buffer, int offset, int count) => Task<int>.Factory.FromAsync(that.BeginRead, that.EndRead, buffer, offset, count, null);
}

internal static class Debug
{
    public static void WriteLine(string message)
        => UnityDebug.Log(message);

    public static void Warning(object message)
        => UnityDebug.LogWarning(message);

    public static void Assert(bool condition)
        => UnityDebug.Assert(condition);
}
