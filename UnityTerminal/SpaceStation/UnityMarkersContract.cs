using System;
using System.Collections.Generic;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using Calibration;
using ReacTiVisionHal.Model;
using SpaceStation;
using SpaceStation.HighConcurrence;
using SpaceStation.PeerToPeer;
using UnityEngine;


public partial class UnityMarkersContract : ICalibartionHomeland
{
    public void Test() => Invoke();
    public Task<ContractData> GetDataAsync() => InvokeAsync<ContractData>(GetCurrentMethodInfo());

    public Task<int> AddAsync() => InvokeAsync<int>(GetCurrentMethodInfo());

    //public int Add() => Invoke<int>();
}
public partial class UnityMarkersContract : IUnityAmbassor
{
    public List<MarkerInfo> MarkerInfos { get; } = new List<MarkerInfo>();
     
    public void UpdateMarkerPositions(IEnumerable<MarkerInfo> infos)
    {
        foreach (var info in infos)
        {
            foreach (var markerInfo in MarkerInfos)
            {
                if(markerInfo.Id == info.Id)
                    markerInfo.Copy(info);
            }
        }
    }
}

public sealed partial class UnityMarkersContract :IntelligentCommunication
{
    protected override MethodInfo GetCurrentMethodInfo([CallerMemberName]string name = "")
    {
        Debug.Assert(!string.IsNullOrEmpty(name));
        return GetType().GetMethod(name);
    }

    public UnityMarkersContract()
    {
        for (int i = 0; i < 24; i++)
        {
            MarkerInfos.Add(new MarkerInfo
            {
                Id = i
            });
        }
    }

    //"127.0.0.1"
    // 192.168.0.6
    public Task EstablishAsync() => Establish("localhost", 6666, nameof(ICalibartionHomeland));

    public override Task Establish(string homelandAddress, int port, string homeland)
    {
        Station = new Station(new P2PClientTerminal()); //P2PClientTerminal SaeaClientTerminal
        RegisterService<IUnityAmbassor>(this);

        return base.Establish(homelandAddress, port, homeland);
    }
}

