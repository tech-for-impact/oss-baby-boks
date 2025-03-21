using System;
using System.Collections.Generic;
using UnityEngine;

public class BluetoothDeviceScript : MonoBehaviour
{
#if UNITY_IOS
    public Dictionary<string, string> BLEStandardUUIDs = new Dictionary<string, string>();
#endif

    public List<string> DiscoveredDeviceList;

	public Action InitializedAction;
	public Action DeinitializedAction;
	public Action<string> ErrorAction;
	public Action<string> ServiceAddedAction;
	public Action StartedAdvertisingAction;
	public Action StoppedAdvertisingAction;
	public Action<string, string> DiscoveredPeripheralAction;
	public Action<string, string, int, byte[]> DiscoveredPeripheralWithAdvertisingInfoAction;
	public Action<BluetoothLEHardwareInterface.iBeaconData> DiscoveredBeaconAction;
	public Action<string, string> RetrievedConnectedPeripheralAction;
	public Action<string, byte[]> PeripheralReceivedWriteDataAction;
	public Action<string> ConnectedPeripheralAction;
	public Action<string> ConnectedDisconnectPeripheralAction;
	public Action<string> DisconnectedPeripheralAction;
	public Action<string, string> DiscoveredServiceAction;
	public Action<string, string, string> DiscoveredCharacteristicAction;
	public Action<string> DidWriteCharacteristicAction;
	public Dictionary<string, Dictionary<string, Action<string>>> DidUpdateNotificationStateForCharacteristicAction;
	public Dictionary<string, Dictionary<string, Action<string, string>>> DidUpdateNotificationStateForCharacteristicWithDeviceAddressAction;
	public Dictionary<string, Dictionary<string, Action<string, byte[]>>> DidUpdateCharacteristicValueAction;
	public Dictionary<string, Dictionary<string, Action<string, string, byte[]>>> DidUpdateCharacteristicValueWithDeviceAddressAction;
	public Action<string, int> RequestMtuAction;
	public Action<string, int> ReadRSSIAction;

	// Use this for initialization
	void Start ()
	{
		DiscoveredDeviceList = new List<string> ();
		DidUpdateNotificationStateForCharacteristicAction = new Dictionary<string, Dictionary<string, Action<string>>> ();
		DidUpdateNotificationStateForCharacteristicWithDeviceAddressAction = new Dictionary<string, Dictionary<string, Action<string, string>>> ();
		DidUpdateCharacteristicValueAction = new Dictionary<string, Dictionary<string, Action<string, byte[]>>> ();
		DidUpdateCharacteristicValueWithDeviceAddressAction = new Dictionary<string, Dictionary<string, Action<string, string, byte[]>>> ();

#if UNITY_IOS
        BLEStandardUUIDs["Heart Rate Measurement"] = "00002A37-0000-1000-8000-00805F9B34FB";
#endif
    }

    // Update is called once per frame
    void Update ()
	{
	}

	const string deviceInitializedString = "Initialized";
	const string deviceDeInitializedString = "DeInitialized";
	const string deviceErrorString = "Error";
	const string deviceServiceAdded = "ServiceAdded";
	const string deviceStartedAdvertising = "StartedAdvertising";
	const string deviceStoppedAdvertising = "StoppedAdvertising";
	const string deviceDiscoveredPeripheral = "DiscoveredPeripheral";
	const string deviceDiscoveredBeacon = "DiscoveredBeacon";
	const string deviceRetrievedConnectedPeripheral = "RetrievedConnectedPeripheral";
	const string devicePeripheralReceivedWriteData = "PeripheralReceivedWriteData";
	const string deviceConnectedPeripheral = "ConnectedPeripheral";
	const string deviceDisconnectedPeripheral = "DisconnectedPeripheral";
	const string deviceDiscoveredService = "DiscoveredService";
	const string deviceDiscoveredCharacteristic = "DiscoveredCharacteristic";
	const string deviceDidWriteCharacteristic = "DidWriteCharacteristic";
	const string deviceDidUpdateNotificationStateForCharacteristic = "DidUpdateNotificationStateForCharacteristic";
	const string deviceDidUpdateValueForCharacteristic = "DidUpdateValueForCharacteristic";
	const string deviceLog = "Log";
	const string deviceRequestMtu = "MtuChanged";
	const string deviceReadRSSI = "DidReadRSSI";

	public void OnBluetoothMessage (string message)
	{
		if (message != null)
		{
			char[] delim = new char[] { '~' };
			string[] parts = message.Split (delim);

			string log = "";
			for (int i = 0; i < parts.Length; ++i)
				log += string.Format("| {0}", parts[i]);
			BluetoothLEHardwareInterface.Log(log);

            if (message.Length >= deviceInitializedString.Length && message.Substring (0, deviceInitializedString.Length) == deviceInitializedString)
			{
				if (InitializedAction != null)
					InitializedAction ();
			}
			else if (message.Length >= deviceLog.Length && message.Substring (0, deviceLog.Length) == deviceLog)
			{
				BluetoothLEHardwareInterface.Log (parts[1]);
			}
			else if (message.Length >= deviceDeInitializedString.Length && message.Substring (0, deviceDeInitializedString.Length) == deviceDeInitializedString)
			{
				BluetoothLEHardwareInterface.FinishDeInitialize ();

				if (DeinitializedAction != null)
					DeinitializedAction ();
			}
			else if (message.Length >= deviceErrorString.Length && message.Substring (0, deviceErrorString.Length) == deviceErrorString)
			{
				string error = "";

				if (parts.Length >= 2)
					error = parts[1];

				if (ErrorAction != null)
					ErrorAction (error);
			}
			else if (message.Length >= deviceServiceAdded.Length && message.Substring (0, deviceServiceAdded.Length) == deviceServiceAdded)
			{
				if (parts.Length >= 2)
				{
					if (ServiceAddedAction != null)
						ServiceAddedAction (parts[1]);
				}
			}
			else if (message.Length >= deviceStartedAdvertising.Length && message.Substring (0, deviceStartedAdvertising.Length) == deviceStartedAdvertising)
			{
				BluetoothLEHardwareInterface.Log ("Started Advertising");

				if (StartedAdvertisingAction != null)
					StartedAdvertisingAction ();
			}
			else if (message.Length >= deviceStoppedAdvertising.Length && message.Substring (0, deviceStoppedAdvertising.Length) == deviceStoppedAdvertising)
			{
				BluetoothLEHardwareInterface.Log ("Stopped Advertising");

				if (StoppedAdvertisingAction != null)
					StoppedAdvertisingAction ();
			}
			else if (message.Length >= deviceDiscoveredPeripheral.Length && message.Substring (0, deviceDiscoveredPeripheral.Length) == deviceDiscoveredPeripheral)
			{
				if (parts.Length >= 3)
				{
					// the first callback will only get called the first time this device is seen
					// this is because it gets added to the a list in the DiscoveredDeviceList
					// after that only the second callback will get called and only if there is
					// advertising data available
					if (!DiscoveredDeviceList.Contains (parts[1] + "|" + parts[2]))
					{
						DiscoveredDeviceList.Add (parts[1] + "|" + parts[2]);

						if (DiscoveredPeripheralAction != null)
							DiscoveredPeripheralAction (parts[1], parts[2]);
					}

					if (parts.Length >= 5 && DiscoveredPeripheralWithAdvertisingInfoAction != null)
					{
						// get the rssi from the 4th value
						int rssi = 0;
						if (!int.TryParse (parts[3], out rssi))
							rssi = 0;

						// parse the base 64 encoded data that is the 5th value
						byte[] bytes = System.Convert.FromBase64String (parts[4]);

						DiscoveredPeripheralWithAdvertisingInfoAction (parts[1], parts[2], rssi, bytes);
					}
				}
			}
			else if (message.Length >= deviceDiscoveredBeacon.Length && message.Substring (0, deviceDiscoveredBeacon.Length) == deviceDiscoveredBeacon)
			{
				if (parts.Length >= 7)
				{
					var iBeaconData = new BluetoothLEHardwareInterface.iBeaconData ();

					iBeaconData.UUID = parts[1];
					if (!int.TryParse (parts[2], out iBeaconData.Major))
						iBeaconData.Major = 0;
					if (!int.TryParse (parts[3], out iBeaconData.Minor))
						iBeaconData.Minor = 0;
					if (!int.TryParse (parts[4], out iBeaconData.RSSI))
						iBeaconData.RSSI = 0;
					if (!int.TryParse (parts[5], out iBeaconData.AndroidSignalPower))
						iBeaconData.AndroidSignalPower = 0;
					int iOSProximity = 0;
					if (!int.TryParse (parts[6], out iOSProximity))
						iOSProximity = 0;
					iBeaconData.iOSProximity = (BluetoothLEHardwareInterface.iOSProximity)iOSProximity;

					if (DiscoveredBeaconAction != null)
						DiscoveredBeaconAction (iBeaconData);
				}
			}
			else if (message.Length >= deviceRetrievedConnectedPeripheral.Length && message.Substring (0, deviceRetrievedConnectedPeripheral.Length) == deviceRetrievedConnectedPeripheral)
			{
				if (parts.Length >= 3)
				{
					DiscoveredDeviceList.Add (parts[1]);

					if (RetrievedConnectedPeripheralAction != null)
						RetrievedConnectedPeripheralAction (parts[1], parts[2]);
				}
			}
			else if (message.Length >= devicePeripheralReceivedWriteData.Length && message.Substring (0, devicePeripheralReceivedWriteData.Length) == devicePeripheralReceivedWriteData)
			{
				if (parts.Length >= 3)
					OnPeripheralData (parts[1], parts[2]);
			}
			else if (message.Length >= deviceConnectedPeripheral.Length && message.Substring (0, deviceConnectedPeripheral.Length) == deviceConnectedPeripheral)
			{
				if (parts.Length >= 2 && ConnectedPeripheralAction != null)
					ConnectedPeripheralAction (parts[1]);
			}
			else if (message.Length >= deviceDisconnectedPeripheral.Length && message.Substring (0, deviceDisconnectedPeripheral.Length) == deviceDisconnectedPeripheral)
			{
				if (parts.Length >= 2)
				{
					if (ConnectedDisconnectPeripheralAction != null)
						ConnectedDisconnectPeripheralAction (parts[1]);

					if (DisconnectedPeripheralAction != null)
						DisconnectedPeripheralAction (parts[1]);
				}
			}
			else if (message.Length >= deviceDiscoveredService.Length && message.Substring (0, deviceDiscoveredService.Length) == deviceDiscoveredService)
			{
				if (parts.Length >= 3 && DiscoveredServiceAction != null)
					DiscoveredServiceAction (parts[1], parts[2]);
			}
			else if (message.Length >= deviceDiscoveredCharacteristic.Length && message.Substring (0, deviceDiscoveredCharacteristic.Length) == deviceDiscoveredCharacteristic)
			{
				if (parts.Length >= 4 && DiscoveredCharacteristicAction != null)
					DiscoveredCharacteristicAction (parts[1], parts[2], parts[3]);
			}
			else if (message.Length >= deviceDidWriteCharacteristic.Length && message.Substring (0, deviceDidWriteCharacteristic.Length) == deviceDidWriteCharacteristic)
			{
				if (parts.Length >= 2 && DidWriteCharacteristicAction != null)
					DidWriteCharacteristicAction (parts[1]);
			}
			else if (message.Length >= deviceDidUpdateNotificationStateForCharacteristic.Length && message.Substring (0, deviceDidUpdateNotificationStateForCharacteristic.Length) == deviceDidUpdateNotificationStateForCharacteristic)
			{
				if (parts.Length >= 3)
				{
					if (DidUpdateNotificationStateForCharacteristicAction != null && DidUpdateNotificationStateForCharacteristicAction.ContainsKey (parts[1]))
					{
						var characteristicAction = DidUpdateNotificationStateForCharacteristicAction[parts[1]];
						if (characteristicAction != null && characteristicAction.ContainsKey (parts[2]))
						{
							var action = characteristicAction[parts[2]];
							if (action != null)
								action (parts[2]);
						}
					}

					if (DidUpdateNotificationStateForCharacteristicWithDeviceAddressAction != null && DidUpdateNotificationStateForCharacteristicWithDeviceAddressAction.ContainsKey (parts[1]))
					{
						var characteristicAction = DidUpdateNotificationStateForCharacteristicWithDeviceAddressAction[parts[1]];
						if (characteristicAction != null && characteristicAction.ContainsKey (parts[2]))
						{
							var action = characteristicAction[parts[2]];
							if (action != null)
								action (parts[1], parts[2]);
						}
					}
				}
			}
			else if (message.Length >= deviceDidUpdateValueForCharacteristic.Length && message.Substring (0, deviceDidUpdateValueForCharacteristic.Length) == deviceDidUpdateValueForCharacteristic)
			{
				if (parts.Length >= 4)
	{	
		
					OnBluetoothData (parts[1], parts[2], parts[3]);}
			}
			else if (message.Length >= deviceRequestMtu.Length && message.Substring(0, deviceRequestMtu.Length) == deviceRequestMtu)
			{
				if (parts.Length >= 3)
                {
					if (RequestMtuAction != null)
					{
						int mtu = 0;
						if (int.TryParse(parts[2], out mtu))
							RequestMtuAction(parts[1], mtu);
					}
                }
			}
			else if (message.Length >= deviceReadRSSI.Length && message.Substring(0, deviceReadRSSI.Length) == deviceReadRSSI)
			{
				if (parts.Length >= 3)
				{
					if (ReadRSSIAction != null)
					{
						int rssi = 0;
						if (int.TryParse(parts[2], out rssi))
							ReadRSSIAction(parts[1], rssi);
					}
				}
			}
		}
	}

	public void OnBluetoothData (string base64Data)
	{
		OnBluetoothData ("", "", base64Data);
	}

	public void OnBluetoothData (string deviceAddress, string characteristic, string base64Data)
	{
		if (base64Data != null)
		{
			byte[] bytes = System.Convert.FromBase64String (base64Data);
			if (bytes.Length > 0)
			{
				deviceAddress = deviceAddress.ToUpper ();
				characteristic = characteristic.ToUpper ();

#if UNITY_IOS
                if (BLEStandardUUIDs.ContainsKey(characteristic))
                    characteristic = BLEStandardUUIDs[characteristic];
#endif

                BluetoothLEHardwareInterface.Log ("Device: " + deviceAddress + " Characteristic Received: " + characteristic);

				string byteString = "";
				foreach (byte b in bytes)
					byteString += string.Format ("{0:X2}", b);

				BluetoothLEHardwareInterface.Log (byteString);

				if (DidUpdateCharacteristicValueAction != null && DidUpdateCharacteristicValueAction.ContainsKey (deviceAddress))
				{
					var characteristicAction = DidUpdateCharacteristicValueAction[deviceAddress];
#if UNITY_ANDROID
					characteristic = characteristic.ToLower ();
#endif
					if (characteristicAction != null && characteristicAction.ContainsKey (characteristic))
					{
						var action = characteristicAction[characteristic];
						if (action != null)
							action (characteristic, bytes);
					}
				}

				if (DidUpdateCharacteristicValueWithDeviceAddressAction != null && DidUpdateCharacteristicValueWithDeviceAddressAction.ContainsKey (deviceAddress))
				{
					var characteristicAction = DidUpdateCharacteristicValueWithDeviceAddressAction[deviceAddress];
#if UNITY_ANDROID
					characteristic = characteristic.ToLower ();
#endif
					if (characteristicAction != null && characteristicAction.ContainsKey (characteristic))
					{
						var action = characteristicAction[characteristic];
						if (action != null)
							action (deviceAddress, characteristic, bytes);
					}
				}
			}
		}
	}

	public void OnPeripheralData (string characteristic, string base64Data)
	{
		if (base64Data != null)
		{
			byte[] bytes = System.Convert.FromBase64String (base64Data);
			if (bytes.Length > 0)
			{
				BluetoothLEHardwareInterface.Log ("Peripheral Received: " + characteristic);

				string byteString = "";
				foreach (byte b in bytes)
					byteString += string.Format ("{0:X2}", b);

				BluetoothLEHardwareInterface.Log (byteString);

				if (PeripheralReceivedWriteDataAction != null)
					PeripheralReceivedWriteDataAction (characteristic, bytes);
			}
		}
	}

#if UNITY_IOS
	private void IncludeCoreLocationFramework()
	{
		// this method is here because Unity now only includes CoreLocation
		// if there are methods in the .cs code that access it
		Input.location.Stop ();
	}
#endif

	public void OnApplicationQuit()
	{
		if (Application.isEditor)
        {
			BluetoothLEHardwareInterface.DeInitialize(() =>
			{
				BluetoothLEHardwareInterface.Log("Deinitialize complete");
			});
        }
	}
}
