using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using SpaceStation;
using UnityEditor;
using UnityEngine;

public class Test : MonoBehaviour
{

    private UnityMarkersContract _contract;

    List<GameObject> _markers = new List<GameObject>();

        // Use this for initialization
    async void Start ()
    {
        Application.runInBackground = true;

        for (int i = 0; i < 24; i++)
        {
            var markerObject = GameObject.CreatePrimitive(PrimitiveType.Capsule);

            markerObject.AddComponent<MarkerController>();
            //markerObject.SetActive(false);

            _markers.Add(markerObject);
        }


        _contract = new UnityMarkersContract();

        await _contract.EstablishAsync();

        _contract.Test();
        //await LaunchAsync();
    }

    public async void GetData()
    {
        //await AsyncTools.ToThreadPool();
        var result = await _contract.GetDataAsync();

        Debug.LogWarning($"GetData result: {result.StringProperty}, subdata: {result.SubData.DateTime}");
    }

    public async void Add()
    {
        var result = await _contract.AddAsync();
        Debug.LogWarning($"{nameof(_contract.AddAsync)} = {result}");
    }

	// Update is called once per frame
	async void Update () {
		//_contract.Test();
	    for (int i = 0; i < 24; i++)
	    {
	        var info = _contract.MarkerInfos[i];
	        var marker = _markers[i];

            marker.GetComponent<MarkerController>().Target = new Vector3((float)info.X / 100.0f, 0, (float)info.Y / 100.0f);
            //marker.transform.position = new Vector3((float) info.X / 100.0f, 0, (float) info.Y / 100.0f);
        }
    }

    private void OnApplicationQuit() => _contract.Shutdown();
}

[CustomEditor(typeof(Test))]
public class ObjectBuilderEditor : Editor
{
    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();

        Test myScript = (Test)target;
        if (GUILayout.Button("Get Data"))
        {
            myScript.GetData();
            Debug.ClearDeveloperConsole();
        }
        if (GUILayout.Button("Add"))
        {
            myScript.Add();
        }
    }
}