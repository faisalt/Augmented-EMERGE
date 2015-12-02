using UnityEngine;
using System.Collections;
using Meta; 
using SocketIO;
using System.Collections.Generic;
using SimpleJSON;
using System;

public class FixedMarkerTracking : MonoBehaviour {

	public GameObject markerdetectorGO;
	public MarkerTargetIndicator marketTargetindicator;
	public MarkerTarget target;

	public GameObject[] bars;
	public GameObject bar;

	public GameObject[] txtValues;
	public GameObject txtValue;
	
	public int id = 1; 
	public int id0 = 0;
	public int barElements = 20;

	public GameObject socketobject;
	public SocketIOComponent socket;
	public bool socketOpened = false;

	public int messageCounter = 0;

	Dictionary<string, string> sockData = new Dictionary<string, string>();

	void Start () {
		markerdetectorGO = MarkerDetector.Instance.gameObject;
		//hide markerindicator
		marketTargetindicator = markerdetectorGO.GetComponent<MarkerTargetIndicator>();
		target = markerdetectorGO.GetComponent<MarkerTarget> ();
		//marketTargetindicator.enabled = false;

		bar = GameObject.CreatePrimitive (PrimitiveType.Cube); 
		bar.transform.localScale = new Vector3 ((float)0, (float)0, (float)0);

		txtValue = new GameObject("TextField");
		txtValue.transform.localScale = new Vector3 ((float)0, (float)0, (float)0);

		bars = new GameObject[100];
		txtValues = new GameObject[100];
		for (var i=0; i<100; i++) {
			bars[i] = Instantiate(bar, new Vector3(0,0,0), Quaternion.identity) as GameObject;
			bars[i].name = "Bar_"+i.ToString();
			bars[i].AddComponent<Canvas>();
			//Canvas temp = bars[i].GetComponent<Canvas>();
			bars[i].GetComponent<RectTransform>().sizeDelta = new Vector2((float)1.5, (float)1.5);
			bars[i].transform.localScale = new Vector3((float)0.01, (float)0.06, (float)0.01);

			bars[i].GetComponent<Renderer>().enabled = false; // Makes the bars invisible.

			txtValues[i] = Instantiate(txtValue, new Vector3(0,0,0), Quaternion.identity) as GameObject;
			txtValues[i].name = "BarValue_"+i.ToString();
			txtValues[i].AddComponent<TextMesh>();
			//txtValues[i].AddComponent<MeshRenderer>();
			txtValues[i].GetComponent<TextMesh>().text = "1"; // Set some default text
			Font ArialFont = (Font)Resources.GetBuiltinResource (typeof(Font), "Arial.ttf"); // Get the font to apply to the textmesh
			txtValues[i].GetComponent<TextMesh>().font = ArialFont;
			txtValues[i].GetComponent<TextMesh>().fontSize = 10;
			txtValues[i].GetComponent<TextMesh>().renderer.sharedMaterial = ArialFont.material; // Important to set the font as a material
		}

		socketobject = GameObject.Find("SocketIO");
		socket = socketobject.GetComponent<SocketIOComponent> ();

		socket.On("open", OnSocketOpen);
		socket.On ("error", SocketError);

		socket.On ("UNITY_PING", dataTransmission);

		socket.On ("UNITYCLIENT_DATASET_WINDOW_UPDATE", datasetUpdate);
	}

	public void datasetUpdate(SocketIOEvent e) {
		//JSONObject jsonobject = e.data;
		Debug.Log (e.data);
		// Get the minimum and maximum values to map to the bar heights. maxz will be default bar height.
		float maxvalue = float.Parse(e.data["maxz"].ToString());
		float minvalue = float.Parse(e.data["minz"].ToString());
		for (var i=0; i<100; i++) {
			// Map the values to the 10x10 grid
			string valueStr = e.data["data"][i].ToString();
			valueStr = valueStr.Replace("\"", "");
			txtValues[i].GetComponent<TextMesh>().text = valueStr;
			// Need to adjust the height of the bars
			float fval = float.Parse(valueStr);
			// Scale the height value sent from the server from 0 to 1 - easier to deal with
			float scaledValue = (fval - minvalue)/(maxvalue-minvalue);
			Vector3 bheight = bars[i].transform.localScale;
			// Use the current height of the virtual bars as maxinum, so multiply by scaled val from above and apply new bar height
			float adjustedValue = (float)0.06*scaledValue;
			bheight.y = adjustedValue;
			bars[i].transform.localScale = bheight;
		}
	}
	public void dataTransmission(SocketIOEvent e) {
		var clientId = e.data.GetField ("client_id").ToString();
		Debug.Log ("Client ID: " + clientId);
		socket.Emit ("UNITY_LOCK", new JSONObject(clientId));
	}

	public void OnSocketOpen(SocketIOEvent ev){
		Debug.Log("Socket opened to server"); 
		if (messageCounter < 1) {
			socket.Emit ("UNITY_SOCKET");
			messageCounter++;
		}
		socketOpened = true;
	}

	public void SocketError(SocketIOEvent e){
		Debug.Log("[SocketIO] Error received: " + e.name + " " + e.data);
		socketOpened = false;
	}

	/**
	 * Renders the virtual bars in the scene
	 * @myTransform - the gameobject reference
	 * @attr - additional attribute, in this case to set the position of the bars in relation to the marker
	 */
	public void displayVirtualBars(Transform myTransform, int attr) {
		float planeWidth = myTransform.localScale.x;
		float marker_xpos = markerdetectorGO.transform.position.x;
		float marker_zpos= markerdetectorGO.transform.position.z;
		Debug.Log ("Plane width is: " + planeWidth);
		// Debug.Log("Marker x position: " + marker_xpos + ", z position: " + marker_zpos + " | Plane position: " + myTransform.position.x);
		// The height and width is already set in the GUI, so we just need to position the plane based on the marker
		// myTransform.position = new Vector3((myTransform.position.x+(planeWidth/2)*attr), myTransform.position.y-(float)0.01, myTransform.position.z+(planeWidth/2));
		// myTransform.position = new Vector3(marker_xpos, myTransform.position.y-(float)0.01, marker_zpos);
		// Angle test below
		// myTransform.transform.eulerAngles = new Vector3 (myTransform.eulerAngles.x, myTransform.eulerAngles.y, myTransform.eulerAngles.z);

		//Vector3 shift = new Vector3 ((float)(planeWidth/2)*attr, (float)0, (float)(planeWidth/2));
		//Vector3 shift = new Vector3 ((marker_xpos+(planeWidth/2))*-1, (float)0, (float)0);
		//this.transform.position += shift;


		
		var rowCounter = 0;
		var colCounter = 0;
		
		float z_pos = myTransform.position.z;
		float x_pos = myTransform.position.x;
		float y_pos = myTransform.position.y;
		float z_position = z_pos+(planeWidth/2);

		float x_rotation = myTransform.eulerAngles.x;
		float y_rotation = myTransform.eulerAngles.y;
		float z_rotation = myTransform.eulerAngles.z;
		
		foreach(var b in bars) {
			float barwidth = b.transform.localScale.x; // Store the width of the bars.
			float barheight = b.transform.localScale.y; // Store the height of the bars.
			float extraSpace = (barwidth+(float)0.01)*rowCounter; // Allow extra space in between bar placements.
			
			// Position the bars.
			b.transform.position = new Vector3((x_pos-(planeWidth/2)) + extraSpace, y_pos+(barheight/2), z_position); 
			// Set the viewing angle of the bars.
			//b.transform.eulerAngles = new Vector3(b.transform.eulerAngles.x,  y_rotation, z_rotation);
			b.transform.eulerAngles = new Vector3(x_rotation,  y_rotation, z_rotation);

			rowCounter++;
			// Each row should contain 10 bars, so once the counter is 10 the z-axis value needs to be shifted.
			if(rowCounter % 10 == 0) {
				rowCounter = 0;
				colCounter++;
				z_position = (z_pos+(planeWidth/2))-(colCounter*(barwidth+(float)0.01));
			} 
		}
		var barCounter=0;
		foreach(var t in txtValues) { 
			t.transform.localScale = new Vector3((float)0.009, (float)0.02, (float)0.009); // Set the scale of the text
			t.transform.position = bars[barCounter].transform.position; // Set the position of the text relative to the bars
			//t.transform.eulerAngles = bars[barCounter].transform.eulerAngles; // Set the angle of the text relative to the bars
			
			// We want the text to be slightly higher than the bar height
			float extraHeight = (float)0.1;
			Vector3 txtpos = t.transform.position;
			txtpos.y = txtpos.y+extraHeight;
			t.transform.position = txtpos;
			
			barCounter++;
		}
	}
	
	
	// Update is called once per frame
	void LateUpdate () {
		if (!markerdetectorGO.activeSelf){
			markerdetectorGO.SetActive(true);
		}
		Transform newTransform = this.transform; // The current plane/GameObject to which the script is attached to (e.g. in this case, GraphPlane).
		Debug.Log ("x pos: " + newTransform.position.x + "z pos: " + newTransform.position.z);
		if (MarkerDetector.Instance != null){
			if (MarkerDetector.Instance.updatedMarkerTransforms.Contains(id) && !MarkerDetector.Instance.updatedMarkerTransforms.Contains(id0)) {
				//Debug.Log("Only ID=1 in view");
				MarkerDetector.Instance.GetMarkerTransform(id, ref newTransform);
				displayVirtualBars(newTransform, 1);
			}
			else if(!MarkerDetector.Instance.updatedMarkerTransforms.Contains(id) && MarkerDetector.Instance.updatedMarkerTransforms.Contains(id0)) {
				// Here, only one of the markers is in view
				// Debug.Log("Only ID=0 in view");
				Debug.Log ("marker detected!");
				MarkerDetector.Instance.GetMarkerTransform(id0, ref newTransform);
				displayVirtualBars(newTransform, -1);
			}
		}
	}	
}