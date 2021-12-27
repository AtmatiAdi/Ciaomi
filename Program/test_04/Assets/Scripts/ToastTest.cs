using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ToastTest : MonoBehaviour {

    public Text Paired;
    public AndroidJavaClass unityPlayerClass;
    public AndroidJavaObject BtPlugin;
    public AndroidJavaObject unityActivity;
    public string device;

    public void CallNativePlugin()
    {
        string RainBovv = "20:16:12:15:66:05";

        unityPlayerClass = new AndroidJavaClass("com.unity3d.player.UnityPlayer");                  // Retrieve the UnityPlayer class.
        unityActivity = unityPlayerClass.GetStatic<AndroidJavaObject>("currentActivity");           // Retrieve the UnityPlayerActivity object ( a.k.a. the current context )
        BtPlugin = new AndroidJavaObject("com.adi.atmati.new_bt_module.BtClass");                   // Retrieve the "Bridge" from our native plugin.
        

        object[] parameters = new object[1];                                                        // Setup the parameters we want to send to our native plugin.    
        parameters[0] = unityActivity;
        //parameters[1] = "This is an call to native android!";

        //BtPlugin.Call("createInstance");                                                            // Tworzymy instancje klasy, ale czy aby na pewno to potrzebne??
        BtPlugin.Call("EnableBT", parameters);                                                      // Poruchamiamy BT   
        //BtPlugin.Call("ForceEnableBT");                                                      // Poruchamiamy BT po chamsku :D
        BtPlugin.Call("GetPairedDevices");
        device = BtPlugin.Call<string>("GetPairedDevice");                                          // Pobieramy urzadzenia zesparowane
        while (device != "")                              
        {
            Paired.text = Paired.text + device;
            device = BtPlugin.Call<string>("GetPairedDevice");

        }

        int isDiscovering = BtPlugin.Call<int>("StartDiscovery");                                 // Uruchamiamy szukanie urzadzen
         if (isDiscovering == 0)
         {
             Paired.text = Paired.text + "IsDiscovering...";
         }
         else {
             Paired.text = Paired.text + "Discovering fail...";
         }
         
        //BtPlugin.Call("enableDiscoverable");
        parameters[0] = RainBovv;

        int isEnabledBT = BtPlugin.Call<int>("IsEnabledBT");                                 // Uruchamiamy szukanie urzadzen
        if (isEnabledBT == 0)
        {
            BtPlugin.Call("ConnectToDevice", parameters);
        }
        
    }
    void Update()
    {
        device = BtPlugin.Call<string>("GetFoundDevice");                                   // Pobieramy urzadzenia Znalezione
        if (device != "")
        {
            Paired.text = Paired.text + device;
        }
        if ("Ready;" == BtPlugin.GetStatic<string>("ConnectingStatus"))
        {
            BtPlugin.Call("SendToDevice", "Welcome");
            Paired.text = Paired.text + "Send..";
        }
        device = BtPlugin.Call<string>("RecivedData");                                   // Pobieramy urzadzenia Znalezione
        if (device != "")
        {
            Paired.text = Paired.text + device;
        }
        //BtPlugin.SetStatic<string>("elo", 4);
    }


}
