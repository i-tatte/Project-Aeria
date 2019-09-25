using System;
using System.Collections;
using System.Collections.Generic;
using System.IO.Ports;
using System.Threading;
using UniRx;
using UnityEngine;

public class SerialController : MonoBehaviour {

	public string portName;
	public int baurate;
	int cooldown = 10;
	bool[] signal = new bool[16];
	int[] count = new int[16];

	SerialPort serial;
	bool isLoop = true;

	void Start () {
		this.serial = new SerialPort (portName, baurate, Parity.None, 8, StopBits.One);

		try {
			this.serial.Open ();
			Scheduler.ThreadPool.Schedule (() => ReadData ()).AddTo (this);
			Debug.Log ("Succeed to open the specified serial port \"" + portName + "\"");
		} catch (Exception) {
			Debug.Log ("Failed to open the specified serial port \"" + portName + "\"");
			GameManager.canUseSerialPort = false;
		}
	}

	public void ReadData () {
		while (this.isLoop) {
			string message = this.serial.ReadLine ();
			//Debug.Log (message);
			if (message.Length != 16 + 12) message = "0000000000000000000000000001";
			for (int i = 0; i < 16; i++) {
				count[i]--;
				if (message[i] == '1') {
					count[i] = cooldown;
				}
				signal[i] = count[i] > 0 ? true : false;
			}
			//left side
			for (int i = 0; i < 3; i++) {
				int air = int.Parse (message.Substring (16 + i * 2, 2));
				GVContainer.airSignal[i] = air;
				GVContainer.airSignal[i] = -2.5 + ((double) (GVContainer.airSignal[i] - 10) * 2.5 / 30.0);
				GVContainer.airSignal[i] += 2;
				GVContainer.airSignal[i] *= 4.0;
			}
			//right side
			for (int i = 0; i < 3; i++) {
				int air = int.Parse (message.Substring (16 + (i + 3) * 2, 2));
				GVContainer.airSignal[i + 3] = air;
				GVContainer.airSignal[i + 3] = 2.5 - ((double) (GVContainer.airSignal[i + 3] - 10) * 2.5 / 30.0);
				GVContainer.airSignal[i + 3] += 2;
				GVContainer.airSignal[i + 3] *= 4.0;
			}
		}
	}

	public bool[] getSignal () {
		return signal;
	}

	public bool WriteData (string message) {
		try {
			this.serial.Write (message);
		} catch (Exception) {
			return false;
		}
		return true;
	}

	void OnDestroy () {
		this.isLoop = false;
		this.serial.Close ();
	}

	public bool TestMethod (string s) {
		try {
			Debug.Log (Int32.Parse (s));
		} catch (Exception) {
			return false;
		}
		return true;
	}
}