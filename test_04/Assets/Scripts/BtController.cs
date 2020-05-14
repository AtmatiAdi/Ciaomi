using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;



public class BtController : MonoBehaviour {

    byte SET_MOTOR_1_SPEED_FORWAD   = 64;   // 0
    byte SET_MOTOR_1_SPEED_BACK     = 65;   // 1
    byte SET_MOTOR_2_SPEED_FORWAD   = 66;  // 2
    byte SET_MOTOR_2_SPEED_BACK     = 67;   // 3
    byte SET_MOTOR_3_SPEED_FORWAD   = 68;  // 4
    byte SET_MOTOR_3_SPEED_BACK     = 69;  // 5

    byte SET_MOTOR_1_POWER          = 70;   // 6
    byte SET_MOTOR_2_POWER          = 71;  // 7
    byte SET_MOTOR_3_POWER          = 72;  // 8

    byte MOTOR_1_ENABLE             = 73;  // 9
    byte MOTOR_2_ENABLE             = 74;  // 10
    byte MOTOR_3_ENABLE             = 75;  // 11

    byte MOTOR_1_DISABLE            = 76;  // 12
    byte MOTOR_2_DISABLE            = 77;  // 13
    byte MOTOR_3_DISABLE            = 78;   // 14

    Ray ray;
    RaycastHit hit;
    public Camera camera;
    private WebCamTexture Oczy;
    private Color32[] data;
    public int CamHeight = 640;
    public int CamWidth = 480;
    public Texture2D texture;
    public Text Paired;

    public Transform SenPool;
    public GameObject TextPrefab;
    private Transform TextObject;

    public AndroidJavaClass unityPlayerClass;
    public AndroidJavaObject BtPlugin;
    public AndroidJavaObject unityActivity;

    private string device = "";
    private int num;
    private object[] parameters = new object[1];
    public GameObject PairedDeviceObject;
    private GameObject PairedDevicePrefab;
    Transform ConnectingDevice;
    public Material BtConnected;
    public GameObject Ekran;
    public Transform Workspace;
    public Transform ProgramSpace;

    public Button SendBt;
    public InputField FunctionIf;
    public InputField DataIf;

    public Button Left_Up;
    public Button Left_Zero;
    public Button Left_Down;

    public Button Right_Up;
    public Button Right_Zero;
    public Button Right_Down;
    
    public Button All_Up;
    public Button All_Zero;
    public Button All_Down;

    public Button ManualButton;

    public int DelSpeed = 1;
    public int DelPower = 1;

    public int Left_Speed = 0;
    public int Right_Speed = 0;

    public GameObject LControlPanel;
    public Button RunProg;
    public InputField ValField;
    public InputField ProgField;
    public InputField SpeedField;
    int Prog = -1;

    Motor RightMotor;
    Motor LeftMotor;
    public Transform RMSpeed;
    public Transform LMSpeed;

    public GameObject SpeedRank;
    public GameObject SpeedRankBonus;

    bool LeftAngleTrigger = false;
    bool LeftAngleTurn = false;
    bool RightAngleTrigger = false;
    bool RightAngleTurn = false;
    int MBS = 30;        // MotorBaseSpeed
    int LMBS = 30;       // LeftMotorBaseSpeed
    int RMBS = 30;       // RightMotorBaseSpeed
    struct Motor
    {
        int MOTOR_NUMBER;
        int Speed;
        int Power;
        public bool IsActive;
        SendValuesDelegate SendValues;

        public void InitMotor(int Num, SendValuesDelegate SendVal)
        {
            SendValues = SendVal;
            MOTOR_NUMBER = Num;
            IsActive = false;
            SendValues((byte)(76 + MOTOR_NUMBER), 1);
            Speed = 0;
        }
        public void SetSpeed (int S)
        {
            if (S == Speed) return;
            else if (IsActive)
            {
                if (S >= 0)
                {
                    SendValues((byte)(65 + MOTOR_NUMBER * 2), (ushort)(S));         // Powinno byc 64 ale robot jezdzi do przodu tyłem
                }
                else
                {
                    SendValues((byte)(64 + MOTOR_NUMBER * 2), (ushort)(S * -1));
                }
                
                Speed = S;

            }
        }

        public void SetPower (int P)
        {
            if (P == Power) return;
            else
            {
                SendValues((byte)(70 + MOTOR_NUMBER), (ushort)(P));
                Power = P;
            }
        }

        public void SetActive (bool A)
        {
            if (A)
            {
                SendValues((byte)(73 + MOTOR_NUMBER), 1);
                IsActive = true;
            }else
            {
                SendValues((byte)(76 + MOTOR_NUMBER), 1);
                IsActive = false;
            }
        }
    }
    struct Sensor
    {
        private int X;
        private int Y;
        private int H;
        private int W;
        private int ScreenH;
        private int ScreenW;
        private Color32 SensorColor;
        private Color32 ActiveColor;
        private Color32 TmpColor;
        private int Value;
        private Transform ValueTex;
        private int Threshold;

        public bool IsActive;

        public void InitSenor(Vector2Int Cords, Vector2Int Dim, Color32 SColor, Color32 AColor, Vector2Int ScreenRes, Transform valueTex, int Value) {
            X = Cords.x;
            Y = Cords.y;
            H = Dim.x;
            W = Dim.y;
            ScreenH = ScreenRes.x;
            ScreenW = ScreenRes.y;
            SensorColor = SColor;
            ActiveColor = AColor;
            ValueTex = valueTex;
            Threshold = Value;
        }
        public Color32[] Update(Color32[] data)
        {
            int A;
            int B;

            //data[X * ScreenH + Y] = new Color32(255, 0, 0, 255);
            // Srednia z czujnika
            A = (X+W) * ScreenH + Y;
            Color32 Pixel;
            uint Red, Green, Blue;
            Red = Green = Blue = 0;
            for (int a = X * ScreenH + Y; a < A; a += ScreenH)
            {
                //data[a] = new Color32(0, 0, 255, 255);
                B = H + a;
                for (int b = a; b < B; b++)
                {
                    //data[b] = new Color32(0, 0, 255, 255);
                    Pixel = data[b];
                    Red += Pixel.r;
                    Green += Pixel.g;
                    Blue += Pixel.b;
                }
            }
            A = H * W;
            // Konwersja na monochrome, taka dla czlowieka :p
            Value = (byte)(Red / A * 0.299 + Green / A * 0.587 + Blue / A * 0.114);
            ValueTex.gameObject.GetComponent<TextMesh>().text = (Value).ToString() ;
            // Aktualizacja stanu 
            if (Value < Threshold)
            {
                IsActive = true;
                TmpColor = ActiveColor;
            }else
            {
                IsActive = false;
                TmpColor = SensorColor;
            }
            // Rysowanie Okienka
            A = (X - 1) * ScreenH + (Y - 1) + H + 1;    // Kolumna + wiersz
            B = (W + 1) * ScreenH;                      // Do kolumny (szerokosc)

            // Tutaj lecimy z gory na dol 
            for (int a = (X - 1) * ScreenH + (Y - 1); a < A; a++)
            {

                data[a] = TmpColor;
                data[a + B] = TmpColor;
            }
            B = A + B;
            // Tutaj lecimy poziomo
            for (int a = (X - 1) * ScreenH + (Y - 1); a < B; a += ScreenH)
            {
                data[a] = TmpColor;
                data[a + H + 1] = TmpColor;
            }
            return data;
        }

        public Sensor Copy()
        {
            Sensor New = new Sensor();
            New.InitSenor(
                new Vector2Int(X, Y),
                new Vector2Int(H, W),
                new Color32(SensorColor.r, SensorColor.g, SensorColor.b, SensorColor.a), 
                new Color32(ActiveColor.r, ActiveColor.g, ActiveColor.b, ActiveColor.a), 
                new Vector2Int(ScreenH, ScreenW), ValueTex,
                Threshold);
            return New;
        }
    }

    Sensor[] S;
    // Use this for initialization
    IEnumerator Start() {
        yield return Application.RequestUserAuthorization(UserAuthorization.WebCam);
        unityPlayerClass = new AndroidJavaClass("com.unity3d.player.UnityPlayer");                  // Retrieve the UnityPlayer class.
        unityActivity = unityPlayerClass.GetStatic<AndroidJavaObject>("currentActivity");           // Retrieve the UnityPlayerActivity object ( a.k.a. the current context )
        BtPlugin = new AndroidJavaObject("com.adi.atmati.new_bt_module.BtClass");                   // Retrieve the "Bridge" from our native plugin.
        Oczy = new WebCamTexture();

        SendBt.onClick.AddListener(() => SendValues( (byte)int.Parse(FunctionIf.text), ushort.Parse(DataIf.text)));
        ManualButton.onClick.AddListener(Hide_Show);

        Left_Up.onClick.AddListener(() => Manual(false, -DelSpeed));
        Left_Zero.onClick.AddListener(() => Manual(false, 0));
        Left_Down.onClick.AddListener(() => Manual(false, DelSpeed));

        Right_Up.onClick.AddListener(() => Manual(true, -DelSpeed));
        Right_Zero.onClick.AddListener(() => Manual(true, 0));
        Right_Down.onClick.AddListener(() => Manual(true, DelSpeed));

        All_Up.onClick.AddListener(() => { Manual(true, -DelSpeed); Manual(false, -DelSpeed); });
        All_Zero.onClick.AddListener(() => { Manual(true, 0); Manual(false, 0); });
        All_Down.onClick.AddListener(() => { Manual(true, DelSpeed); Manual(false, DelSpeed); });

        RunProg.onClick.AddListener(EnableProgram);
        //LineFollower();  //TESTY XD
    }
    
    void Hide_Show()
    {
        Transform ManualPanel = ManualButton.transform.parent.transform;
        if (ManualPanel.localPosition.x == 400)
        {
            ManualPanel.localPosition = new Vector3(-230, -300, 0);
        } else
        {
            ManualPanel.localPosition = new Vector3(400, -300, 0);
        }
    }

    // Wywolanie tej funckji z paremetrem Val = 0 sprawia ze silnik zostanie wylaczony
    // jednak gdy w skutek nastepnych obliczen predkosc bedze = 0 to silnik dalej bedzie aktywny, narazie to sprawia ze zachowuje se jak nieaktywny
    void Manual(bool Num, int Val)
    {
        byte Func;
        if (Num)   // LEFT
        {
            if (Val == 0)           // Sekwencja zatrzymania
            {
                SendValues(MOTOR_1_DISABLE, 1);
                Left_Speed = 0;
                Left_Zero.GetComponentInChildren<Text>().text = "Left: 0";
                Left_Zero.image.color = new Color(255, 0, 0);
                return;
            }

            if (Left_Speed == 0)    // Gdy startujemy od zera
            {
                SendValues(MOTOR_1_ENABLE, 1);
                SendValues(SET_MOTOR_1_POWER, 3);
                Left_Zero.image.color = new Color(0, 255, 0);
            }
            Left_Speed += Val;
            Left_Zero.GetComponentInChildren<Text>().text = "Left: " + Left_Speed.ToString();
            if (Left_Speed < 0)     // Gdy jedziemy do tylu
            {
                SendValues(SET_MOTOR_1_SPEED_BACK, (ushort)(Left_Speed * -1));
            }
            else                  // Gdy jedziemy do przodu
            {
                SendValues(SET_MOTOR_1_SPEED_FORWAD, (ushort)(Left_Speed));
            }
            
            return;
        }
        else     // RIGHT
        {
            if (Val == 0)           // Sekwencja zatrzymania
            {
                SendValues(MOTOR_3_DISABLE, 1);
                Right_Speed = 0;
                Right_Zero.GetComponentInChildren<Text>().text = "Right: 0";
                Right_Zero.image.color = new Color(255, 0, 0);
                return;
            }

            if (Right_Speed == 0)    // Gdy startujemy od zera
            {
                SendValues(MOTOR_3_ENABLE, 1);
                SendValues(SET_MOTOR_3_POWER, 3);
                Right_Zero.image.color = new Color(0, 255, 0);
            }
            Right_Speed += Val;
            Right_Zero.GetComponentInChildren<Text>().text = "Right: " + Right_Speed.ToString();
            if (Right_Speed < 0)     // Gdy jedziemy do tylu
            {
                SendValues(SET_MOTOR_3_SPEED_BACK, (ushort)(Right_Speed * -1));
            }
            else                  // Gdy jedziemy do przodu
            {
                SendValues(SET_MOTOR_3_SPEED_FORWAD, (ushort)Right_Speed);
            }
            return;
        }
    }

    // Update is called once per frame
    void Update() {
        device = BtPlugin.Call<string>("RecivedData") + "";                                   // Pobieramy odebrane dane
        if (device != "")
        {
            for (int a = 0; a < device.Length; a++)
            {
                Paired.text = Paired.text + "+" + ((byte)device[a]).ToString();
            }
            Paired.text = Paired.text + ";";
        }
        if (Input.GetMouseButtonDown(0))
        { // if left button pressed...
            ray = camera.ScreenPointToRay(Input.mousePosition);
            if (Physics.Raycast(ray, out hit))
            {
                if (hit.collider.name == "BtButton")
                {
                    BtPlugin.Call("ForceEnableBT");                                                      // Poruchamiamy BT po chamsku :D

                    BtPlugin.Call("GetPairedDevices");
                    device = BtPlugin.Call<string>("GetPairedDevice");                                          // Pobieramy urzadzenia zesparowane
                    num = 0;
                    while (device != "")
                    //device = "testin|10:0320:020";
                    {
                        PairedDevicePrefab = Instantiate(PairedDeviceObject) as GameObject;
                        PairedDevicePrefab.name = "BtDevice";
                        PairedDevicePrefab.transform.SetParent(this.transform.parent.transform);
                        PairedDevicePrefab.transform.localPosition = new Vector3(-2f, 5f - (0.8f * num), -4f);
                        PairedDevicePrefab.transform.localRotation = new Quaternion(0f, 0f, 0f, 0f);
                        PairedDevicePrefab.SetActive(true);
                        num = num + 1;
                        int pos = device.IndexOf("|");
                        //Paired.text = device;
                        PairedDevicePrefab.transform.Find("Name").transform.GetComponent<TextMesh>().text = device.Substring(0, pos);
                        PairedDevicePrefab.transform.Find("MAC").transform.GetComponent<TextMesh>().text = device.Substring(pos + 1, device.Length - pos - 1);
                        PairedDevicePrefab.transform.Find("Rssi").transform.GetComponent<TextMesh>().text = "Not Connected";
                        device = BtPlugin.Call<string>("GetPairedDevice");
                    }
                }
                else if (hit.collider.name == "BtDevice")
                {
                    ConnectingDevice = hit.transform.Find("Object").transform;
                    parameters[0] = hit.transform.Find("MAC").transform.GetComponent<TextMesh>().text;
                    Paired.text = hit.transform.Find("Name").transform.GetComponent<TextMesh>().text; ;
                    int isEnabledBT = BtPlugin.Call<int>("IsEnabledBT");                                 // Uruchamiamy szukanie urzadzen
                    if (isEnabledBT == 0)
                    {
                        BtPlugin.Call("ConnectToDevice", parameters);
                        ConnectingDevice = hit.transform.Find("Object").transform;
                    }
                }
                else if (hit.collider.name == "BtConnected")
                {
                    LineFollower();
                }
            }
        }
        if (ConnectingDevice != null)
        {
            ConnectingDevice.Rotate(Vector3.up * Time.deltaTime * 180);
            if ("Ready;" == BtPlugin.GetStatic<string>("ConnectingStatus"))
            {
                //BtDeviceMat.color = BtConnected.color;
                ConnectingDevice.GetComponent<Renderer>().material = BtConnected;
                //ConnectingDevice.Find("Object") = Instantiate(BtConnected) as GameObject;
                ConnectingDevice.localRotation = new Quaternion(0f, 0f, 0f, 0f);
                ConnectingDevice.parent.name = "BtConnected";
                ConnectingDevice.parent.transform.Find("Rssi").transform.GetComponent<TextMesh>().text = "Connected :D";
                ConnectingDevice = null;
            }
        }
        if (Oczy.didUpdateThisFrame)
        {
            Paired.text = "FPS: " + (1f / Time.deltaTime).ToString();
            Oczy.GetPixels32(data);
            UpdateSenors();
            ////////////////////////////////////////////////////
            // PROGRAMY OBLUGUJACE SENSORY //
            ////////////////////////////////////////////////////

            Program();

            ////////////////////////////////////////////////////
            texture.SetPixels32(data);
            // actually apply all SetPixels32, don't recalculate mip levels
            texture.Apply(false);
            
        }
        
    }

    void Program()
    {
        switch (Prog)
        {
            case 1:
                {
                    if ((S[3].IsActive) && (S[4].IsActive))               // Oba centralne widza
                    {
                        RightMotor.SetSpeed(30);
                        LeftMotor.SetSpeed(30);
                    }

                    else if ((S[2].IsActive) || (S[1].IsActive))          // Linia z lewej na drogim lub trzecim czujniku
                    {
                        RightMotor.SetSpeed(50);
                        LeftMotor.SetSpeed(30);
                    }
                    else if (S[3].IsActive)                                          // Linia z lewej strony
                    {
                        RightMotor.SetSpeed(40);
                        LeftMotor.SetSpeed(30);
                    }
                    else if ((S[5].IsActive) || (S[6].IsActive))          // Linia z prawej strony na drogim czujniku
                    {
                        RightMotor.SetSpeed(30);
                        LeftMotor.SetSpeed(50);
                    }
                    else if (S[4].IsActive)                                          // Linia z prawej strony
                    {
                        RightMotor.SetSpeed(30);
                        LeftMotor.SetSpeed(40);
                    }
                    else
                    {
                        RightMotor.SetSpeed(30);
                        LeftMotor.SetSpeed(30);
                    }
                    break;
                }
            case 2:
                {
                    /////////////////////////////////////////////////////////// WYKRYWANIE KATOW PROSTYCH
                    // Jezeli prawe skrzydlo nic nie widzi
                    if (!RightWing())
                    {
                        // Jezeli lewy krancowy widzi a inny lewy nie (wykluczajac ostre wejscie w prosta linie)
                        if (S[8].IsActive)
                        {
                            if (S[0].IsActive || S[16].IsActive) LeftAngleTrigger = false;
                            else LeftAngleTrigger = true;
                        }
                    }
                    // Cos sie dzieje z prawej strony, nie ma kata prostego
                    else LeftAngleTrigger = false;
                    // Jeeli jest trigger i linia jest na 2 czujnikach to ejst to kat prosty
                    if (LeftAngleTrigger && ((S[10].IsActive) && (S[9].IsActive || S[11].IsActive)))
                    {
                        ShowToast("Lewy kat");
                        LeftAngleTurn = true;
                        LeftAngleTrigger = false;
                    }
                    ////////////////////////////////////////////////////////////////////////
                    // Jezeli prawe skrzydlo nic nie widzi
                    if (!LeftWing())
                    {
                        // Jezeli lewy krancowy widzi a inny lewy nie (wykluczajac ostre wejscie w prosta linie)
                        if (S[15].IsActive)
                        {
                            if (S[7].IsActive || S[17].IsActive) RightAngleTrigger = false;
                            else RightAngleTrigger = true;
                        }
                    }
                    // Cos sie dzieje z prawej strony, nie ma kata prostego
                    else RightAngleTrigger = false;
                    // Jeeli jest trigger i linia jest na 2 czujnikach to ejst to kat prosty
                    if (RightAngleTrigger && ((S[13].IsActive) && (S[14].IsActive || S[12].IsActive)))
                    {
                        ShowToast("Prawy kat");
                        RightAngleTurn = true;
                        RightAngleTrigger = false;
                    }
                    /////////////////////////////////////////////////////////////////////////
                    if (S[5].IsActive) LeftAngleTurn = false;
                    if (S[2].IsActive) RightAngleTurn = false;

                    if (RightAngleTurn) LMBS = MBS + 40; else LMBS = MBS;
                    if (LeftAngleTurn) RMBS = MBS + 40; else RMBS = MBS;
                    ////////////////////////////////////////////////////////////////////////// ZWYKLY PROGRAM

                    if ((S[3].IsActive) && (S[4].IsActive))               // Oba centralne widza
                    {
                        RightMotor.SetSpeed(RMBS);
                        LeftMotor.SetSpeed(LMBS);
                    }
                    ///////////////////////////////////// LINIA Z LEWEJ STRONY
                    else if (S[0].IsActive)
                    {
                        RightMotor.SetSpeed(RMBS + 40);
                        LeftMotor.SetSpeed(LMBS);
                    }
                    else if (S[1].IsActive)
                    {
                        RightMotor.SetSpeed(RMBS + 30);
                        LeftMotor.SetSpeed(LMBS);
                    }
                    else if (S[2].IsActive)
                    {
                        RightMotor.SetSpeed(RMBS + 20);
                        LeftMotor.SetSpeed(LMBS);
                    }
                    else if (S[3].IsActive)
                    {
                        RightMotor.SetSpeed(RMBS + 10);
                        LeftMotor.SetSpeed(LMBS);
                    }
                    ///////////////////////////////// LINIA Z PRAWEJ STRPNY
                    else if (S[7].IsActive)
                    {
                        RightMotor.SetSpeed(RMBS);
                        LeftMotor.SetSpeed(LMBS + 40);
                    }
                    else if (S[6].IsActive)
                    {
                        RightMotor.SetSpeed(RMBS);
                        LeftMotor.SetSpeed(LMBS + 30);
                    }
                    else if (S[5].IsActive)
                    {
                        RightMotor.SetSpeed(RMBS);
                        LeftMotor.SetSpeed(LMBS + 20);
                    }
                    else if (S[4].IsActive)
                    {
                        RightMotor.SetSpeed(RMBS);
                        LeftMotor.SetSpeed(LMBS + 10);
                    }
                    else
                    {
                        RightMotor.SetSpeed(RMBS);
                        LeftMotor.SetSpeed(LMBS);
                    }
                    break;
                }
            case 3:
                {
                    bool TurnRight = false;
                    bool TurnLeft = false;
                    LMBS = MBS;
                    RMBS = MBS;

                    if (LeftWing() && !(RightWing() || RightArrow()))
                    {
                        RightMotor.SetPower(5);
                        RightMotor.SetSpeed(RMBS + 120);
                        Paired.text = "LeftWing";
                        TurnLeft = true;
                    }
                    else
                    {
                        for (int L = 0; L < 9; L++)
                        {
                            if (S[L].IsActive)
                            {
                                if (L == 8)
                                {   // Tutaj tylko lekko pzryspieszamy
                                    RightMotor.SetSpeed(RMBS + 5);
                                    TurnLeft = true;
                                    break;
                                }
                                // Większa moc przy wiekszych predkosciach
                                if (9 - L > 2) RightMotor.SetPower(4);
                                RightMotor.SetSpeed(RMBS + (9 - L) * 10);
                                TurnLeft = true;
                                //LeftMotor.SetSpeed(LMBS);
                                Paired.text = Paired.text + "R+" + (9 - L).ToString();
                                break;
                            }
                        }
                    }
                    // Ustawiamy normalana predkosc gdy nic sensory nie wykryja
                    if (!TurnLeft)
                    {
                        RightMotor.SetPower(3);
                        RightMotor.SetSpeed(RMBS);
                    }

                    if (RightWing() && !(LeftWing() || LeftArrow()))
                    {
                        LeftMotor.SetPower(5);
                        LeftMotor.SetSpeed(LMBS + 120);
                        Paired.text = "RightWing";
                        TurnRight = true;
                    }
                    else
                    {
                        for (int L = 17; L > 8; L--)
                        {
                            if (S[L].IsActive)
                            {
                                if (L == 9)
                                {   // Tutaj tylko lekko pzryspieszamy
                                    LeftMotor.SetSpeed(LMBS + 5);
                                    TurnRight = true;
                                    break;
                                }
                                // Większa moc przy wiekszych predkosciach
                                if (L - 8 > 2) LeftMotor.SetPower(4);
                                LeftMotor.SetSpeed(LMBS + (L - 8) * 10);

                                TurnRight = true;
                                Paired.text = Paired.text + "L+" + (L - 8).ToString();
                                break;
                            }
                        }
                    }
                    // Ustawiamy normalana predkosc gdy nic sensory nie wykryja
                    if (!TurnRight)
                    {
                        LeftMotor.SetPower(3);
                        LeftMotor.SetSpeed(LMBS);
                    }

                    break;
                }
        }
    }

    bool RightWing()
    {
        switch (Prog)
        {
            case 2:
                {
                    if (S[15].IsActive || S[14].IsActive || S[13].IsActive || S[12].IsActive) return true;
                    break;
                }
            case 3:
                {
                    if (S[27].IsActive || S[28].IsActive || S[29].IsActive || S[30].IsActive || S[31].IsActive || S[32].IsActive 
                        || S[33].IsActive || S[34].IsActive || S[35].IsActive ||  S[17].IsActive) return true;
                    break;
                }
        }
                
        return false;
    }

    bool LeftWing()
    {
        switch (Prog)
        {
            case 2:
                {
                    if (S[8].IsActive || S[9].IsActive || S[10].IsActive || S[11].IsActive) return true;
                    break;
                }
            case 3:
                {
                    if (S[18].IsActive || S[19].IsActive || S[20].IsActive || S[21].IsActive || S[22].IsActive || S[23].IsActive 
                        || S[24].IsActive || S[25].IsActive || S[26].IsActive ||  S[0].IsActive) return true;
                    break;
                }
        }
        
        return false;
    }

    bool RightArrow()
    {
        switch (Prog)
        {
            case 3:
                {
                    if (S[43].IsActive || S[44].IsActive || S[45].IsActive || S[46].IsActive || S[47].IsActive || S[48].IsActive || S[49].IsActive) return true;
                    break;
                }
        }

        return false;
    }

    bool LeftArrow()
    {
        switch (Prog)
        {
            case 3:
                {
                    if (S[36].IsActive || S[37].IsActive || S[38].IsActive || S[39].IsActive || S[40].IsActive || S[41].IsActive || S[42].IsActive) return true;
                    break;
                }
        }

        return false;
    }

    void EnableProgram()
    {
        if (Prog >= 0)
        {
            RunProg.GetComponentInChildren<Text>().text = "Stopped";
            RunProg.image.color = new Color(255, 0, 0);
            Prog = -1;
            RightMotor.SetActive(false);
            LeftMotor.SetActive(false);
            //SendValues(MOTOR_3_DISABLE, 1);
            //SendValues(MOTOR_1_DISABLE, 1);
            S = null;
            foreach (Transform child in SenPool)
            {
                GameObject.Destroy(child.gameObject);
            }
        }
        else
        {
            // ENABLE
            int Threshold = int.Parse(ValField.text);      // Watrosc graniczna dla czujnikow
            Prog = int.Parse(ProgField.text);
            MBS = int.Parse(SpeedField.text);
            RunProg.GetComponentInChildren<Text>().text = "Running";
            RunProg.image.color = new Color(0, 255, 0);
            S = null;

            int H = CamHeight/3;    // Offset wysokosci wszystkich czunikow
            int W = 0;              // Ofset wysokosci skrzydeł
            int O = 5;              // Offset odleglosci czunikow od srodka
            int Size = 10;          // wielkosc czunikow
            int SizeH = 20;
            switch (Prog)
            {
                case 0:
                    {
                        CreateSensor(10, 610, 20, 60, new Color32(255, 0, 0, 255), new Color32(0, 255, 0, 255), Threshold);          //1
                   
                        return;
                        break;
                    }
                case 1:
                    {
                        CreateSensor(20, 90, 20, 20, new Color32(255, 0, 0, 255), new Color32(0, 255, 0, 255), Threshold);          //1
                        CreateSensor(60, 100, 20, 20, new Color32(255, 0, 0, 255), new Color32(0, 255, 0, 255), Threshold);          //2
                        CreateSensor(100, 100, 20, 20, new Color32(255, 0, 0, 255), new Color32(0, 255, 0, 255), Threshold);         //3
                        CreateSensor(160, 80, 20, 20, new Color32(255, 0, 0, 255), new Color32(0, 255, 0, 255), Threshold);         //4

                        CreateSensor(480 - 180, 80, 20, 20, new Color32(255, 0, 0, 255), new Color32(0, 255, 0, 255), Threshold);   //5
                        CreateSensor(360, 100, 20, 20, new Color32(255, 0, 0, 255), new Color32(0, 255, 0, 255), Threshold);   //6
                        CreateSensor(480 - 80, 100, 20, 20, new Color32(255, 0, 0, 255), new Color32(0, 255, 0, 255), Threshold);   //7
                        CreateSensor(480 - 40, 90, 20, 20, new Color32(255, 0, 0, 255), new Color32(0, 255, 0, 255), Threshold);    //8
                        break;
                    }
                case 2:
                    {
                        CreateSensor(70, 160 ,20, 20, new Color32(255, 69, 0, 255), new Color32(0, 255, 0, 255), Threshold);       
                        CreateSensor(100, 160 ,20, 20, new Color32(255, 69, 0, 255), new Color32(0, 255, 0, 255), Threshold);          
                        CreateSensor(130, 160 ,20, 20, new Color32(255, 69, 0, 255), new Color32(0, 255, 0, 255), Threshold);      
                        CreateSensor(160, 140 ,20, 20, new Color32(255, 165, 0, 255), new Color32(0, 255, 0, 255), Threshold);       

                        CreateSensor(300, 140 ,20, 20, new Color32(255, 165, 0, 255), new Color32(0, 255, 0, 255), Threshold);  
                        CreateSensor(330, 160 ,20, 20, new Color32(255, 69, 0, 255), new Color32(0, 255, 0, 255), Threshold);  
                        CreateSensor(360, 160 ,20, 20, new Color32(255, 69, 0, 255), new Color32(0, 255, 0, 255), Threshold);   
                        CreateSensor(390, 160 ,20, 20, new Color32(255, 0, 0, 255), new Color32(0, 255, 0, 255), Threshold);   

                        CreateSensor(10, 20 ,20, 20, new Color32(0, 191, 255, 255), new Color32(0, 255, 0, 255), Threshold);         

                        CreateSensor(60, 80 ,20, 20, new Color32(30, 144, 255, 255), new Color32(0, 255, 0, 255), Threshold);        
                        CreateSensor(70, 100 ,20, 20, new Color32(30, 144, 255, 255), new Color32(0, 255, 0, 255), Threshold);        
                        CreateSensor(80, 120 ,20, 20, new Color32(30, 144, 255, 255), new Color32(0, 255, 0, 255), Threshold);        

                        CreateSensor(380, 120 ,20, 20, new Color32(30, 144, 255, 255), new Color32(0, 255, 0, 255), Threshold);  
                        CreateSensor(390, 100 ,20, 20, new Color32(30, 144, 255, 255), new Color32(0, 255, 0, 255), Threshold); 
                        CreateSensor(400, 80 ,20, 20, new Color32(30, 144, 255, 255), new Color32(0, 255, 0, 255), Threshold);  

                        CreateSensor(450,20 ,20, 20, new Color32(0, 191, 255, 255), new Color32(0, 255, 0, 255), Threshold);  

                        CreateSensor(10, 150 ,20, 20, new Color32(255, 0, 0, 255), new Color32(0, 255, 0, 255), Threshold);         
                        CreateSensor(450, 150 ,20, 20, new Color32(255, 0, 0, 255), new Color32(0, 255, 0, 255), Threshold);
                        break;
                    }
                case 3:
                    {
                        CreateSensor(40 - O, 40 + H + W, Size, Size, new Color32(255, 69, 0, 255), new Color32(0, 255, 0, 255), Threshold);
                        CreateSensor(60 - O, 30 + H, Size, Size, new Color32(255, 69, 0, 255), new Color32(0, 255, 0, 255), Threshold);
                        CreateSensor(80 - O, 20 + H, Size, Size, new Color32(255, 69, 0, 255), new Color32(0, 255, 0, 255), Threshold);
                        CreateSensor(100 - O, 10 + H, Size, Size, new Color32(255, 69, 0, 255), new Color32(0, 255, 0, 255), Threshold);
                        CreateSensor(120 - O, -0 + H, Size, Size, new Color32(255, 69, 0, 255), new Color32(0, 255, 0, 255), Threshold);
                        CreateSensor(140 - O, -10 + H, Size, Size, new Color32(255, 69, 0, 255), new Color32(0, 255, 0, 255), Threshold);
                        CreateSensor(160 - O, -35 + H, Size, Size, new Color32(255, 165, 0, 255), new Color32(0, 255, 0, 255), Threshold);
                        CreateSensor(180 - O, -40 + H, Size, Size, new Color32(255, 165, 0, 255), new Color32(0, 255, 0, 255), Threshold);
                        CreateSensor(200 - O, -45 + H, Size, Size, new Color32(255, 165, 0, 255), new Color32(0, 255, 0, 255), Threshold);


                        CreateSensor(270 + O, -45 + H, Size, Size, new Color32(255, 165, 0, 255), new Color32(0, 255, 0, 255), Threshold);
                        CreateSensor(290 + O, -40 + H, Size, Size, new Color32(255, 165, 0, 255), new Color32(0, 255, 0, 255), Threshold);
                        CreateSensor(310 + O, -35 + H, Size, Size, new Color32(255, 165, 0, 255), new Color32(0, 255, 0, 255), Threshold);
                        CreateSensor(330 + O, -10 + H, Size, Size, new Color32(255, 69, 0, 255), new Color32(0, 255, 0, 255), Threshold);
                        CreateSensor(350 + O, 0 + H, Size, Size, new Color32(255, 69, 0, 255), new Color32(0, 255, 0, 255), Threshold);
                        CreateSensor(370 + O, 10 + H, Size, Size, new Color32(255, 69, 0, 255), new Color32(0, 255, 0, 255), Threshold);
                        CreateSensor(390 + O, 20 + H, Size, Size, new Color32(255, 69, 0, 255), new Color32(0, 255, 0, 255), Threshold);
                        CreateSensor(410 + O, 30 + H, Size, Size, new Color32(255, 69, 0, 255), new Color32(0, 255, 0, 255), Threshold);
                        CreateSensor(430 + O, 40 + H + W, Size, Size, new Color32(255, 69, 0, 255), new Color32(0, 255, 0, 255), Threshold);

                        CreateSensor(40 - O, -20 + H + W, SizeH, Size, new Color32(255, 0, 0, 255), new Color32(0, 255, 0, 255), Threshold);
                        CreateSensor(40 - O, 10 + H + W, SizeH, Size, new Color32(255, 0, 0, 255), new Color32(0, 255, 0, 255), Threshold);
                        CreateSensor(40 - O, 60 + H + W, SizeH, Size, new Color32(255, 0, 0, 255), new Color32(0, 255, 0, 255), Threshold);
                        CreateSensor(40 - O, 90 + H + W, SizeH, Size, new Color32(255, 0, 0, 255), new Color32(0, 255, 0, 255), Threshold);
                        CreateSensor(40 - O, 120 + H + W, SizeH, Size, new Color32(255, 0, 0, 255), new Color32(0, 255, 0, 255), Threshold);
                        CreateSensor(40 - O, 150 + H + W, SizeH, Size, new Color32(255, 0, 0, 255), new Color32(0, 255, 0, 255), Threshold);
                        CreateSensor(40 - O, 180 + H + W, SizeH, Size, new Color32(255, 0, 0, 255), new Color32(0, 255, 0, 255), Threshold);
                        CreateSensor(40 - O, 210 + H + W, SizeH, Size, new Color32(255, 0, 0, 255), new Color32(0, 255, 0, 255), Threshold);
                        CreateSensor(40 - O, 240 + H + W, SizeH, Size, new Color32(255, 0, 0, 255), new Color32(0, 255, 0, 255), Threshold);

                        CreateSensor(430 + O, -20 + H + W, SizeH, Size, new Color32(255, 0, 0, 255), new Color32(0, 255, 0, 255), Threshold);
                        CreateSensor(430 + O, 10 + H + W, SizeH, Size, new Color32(255, 0, 0, 255), new Color32(0, 255, 0, 255), Threshold);
                        CreateSensor(430 + O, 60 + H + W, SizeH, Size, new Color32(255, 0, 0, 255), new Color32(0, 255, 0, 255), Threshold);
                        CreateSensor(430 + O, 90 + H + W, SizeH, Size, new Color32(255, 0, 0, 255), new Color32(0, 255, 0, 255), Threshold);
                        CreateSensor(430 + O, 120 + H + W, SizeH, Size, new Color32(255, 0, 0, 255), new Color32(0, 255, 0, 255), Threshold);
                        CreateSensor(430 + O, 150 + H + W, SizeH, Size, new Color32(255, 0, 0, 255), new Color32(0, 255, 0, 255), Threshold);
                        CreateSensor(430 + O, 180 + H + W, SizeH, Size, new Color32(255, 0, 0, 255), new Color32(0, 255, 0, 255), Threshold);
                        CreateSensor(430 + O, 210 + H + W, SizeH, Size, new Color32(255, 0, 0, 255), new Color32(0, 255, 0, 255), Threshold);
                        CreateSensor(430 + O, 240 + H + W, SizeH, Size, new Color32(255, 0, 0, 255), new Color32(0, 255, 0, 255), Threshold);

                        CreateSensor(60 - O, -120 + H + W, SizeH, Size, new Color32(30, 144, 255, 255), new Color32(0, 255, 0, 255), Threshold);
                        CreateSensor(60 - O, -90 + H + W, SizeH, Size, new Color32(30, 144, 255, 255), new Color32(0, 255, 0, 255), Threshold);
                        CreateSensor(60 - O, -60 + H + W, SizeH, Size, new Color32(30, 144, 255, 255), new Color32(0, 255, 0, 255), Threshold);
                        CreateSensor(60 - O, -30 + H + W, SizeH, Size, new Color32(30, 144, 255, 255), new Color32(0, 255, 0, 255), Threshold);
                        CreateSensor(60 - O, 0 + H + W, SizeH, Size, new Color32(30, 144, 255, 255), new Color32(0, 255, 0, 255), Threshold);
                        CreateSensor(60 - O, 250 + H + W, SizeH, Size, new Color32(30, 144, 255, 255), new Color32(0, 255, 0, 255), Threshold);
                        CreateSensor(60 - O, 280 + H + W, SizeH, Size, new Color32(30, 144, 255, 255), new Color32(0, 255, 0, 255), Threshold);

                        CreateSensor(410 + O, -120 + H + W, SizeH, Size, new Color32(30, 144, 255, 255), new Color32(0, 255, 0, 255), Threshold);
                        CreateSensor(410 + O, -90 + H + W, SizeH, Size, new Color32(30, 144, 255, 255), new Color32(0, 255, 0, 255), Threshold);
                        CreateSensor(410 + O, -60 + H + W, SizeH, Size, new Color32(30, 144, 255, 255), new Color32(0, 255, 0, 255), Threshold);
                        CreateSensor(410 + O, -30 + H + W, SizeH, Size, new Color32(30, 144, 255, 255), new Color32(0, 255, 0, 255), Threshold);
                        CreateSensor(410 + O, 0 + H + W, SizeH, Size, new Color32(30, 144, 255, 255), new Color32(0, 255, 0, 255), Threshold);
                        CreateSensor(410 + O, 250 + H + W, SizeH, Size, new Color32(30, 144, 255, 255), new Color32(0, 255, 0, 255), Threshold);
                        CreateSensor(410 + O, 280 + H + W, SizeH, Size, new Color32(30, 144, 255, 255), new Color32(0, 255, 0, 255), Threshold);
                        break;
                    }
            }
            RightMotor.InitMotor(0, SendValues);    // Silnik 3
            LeftMotor.InitMotor(2, SendValues);     // Silnik 1
            //SendValues(MOTOR_1_ENABLE, 1);
            //SendValues(MOTOR_3_ENABLE, 1);
            RightMotor.SetActive(true);
            LeftMotor.SetActive(true);
            //SendValues(SET_MOTOR_1_POWER, 1);
            //SendValues(SET_MOTOR_3_POWER, 1);
            RightMotor.SetPower(1);
            LeftMotor.SetPower(1);
        }
    }

    void LineFollower()
    {
        Workspace.position = new Vector3(0f, -10f, 0f);
        ProgramSpace.position = new Vector3(0f, 10f, 0f);

        Oczy = new WebCamTexture();
        //Application.targetFrameRate = 300;
        Oczy.requestedFPS = 300;
        Oczy.requestedHeight = CamHeight;
        Oczy.requestedWidth = CamWidth;
        data = new Color32[CamHeight * CamWidth];
        texture = new Texture2D(CamHeight, CamWidth, TextureFormat.RGBA32, false);
        Ekran.GetComponent<Renderer>().material.mainTexture = texture;
        Oczy.Play();
        LControlPanel.SetActive(true);
        ManualButton.transform.parent.gameObject.SetActive(true);


        ShowToast("Linefollower :D");
    }

    void CreateSensor(int x, int y, int h, int w, Color32 SColor, Color32 AColor, int Threshold)
    {
        if (S == null)
        {
            S = new Sensor[1];
            TextObject = Instantiate(TextPrefab, SenPool).transform;
            TextObject.localPosition = new Vector3((float)y / CamHeight * -10f + 0.15f, 0, (float)x / CamWidth * -10f);
            S[0].InitSenor(new Vector2Int(x, y), new Vector2Int(h, w), SColor, AColor, new Vector2Int(CamHeight, CamWidth), TextObject, Threshold);
        } else
        {
            int l = S.Length;
            Sensor[] tmp = new Sensor[l];
            tmp = (Sensor[])S.Clone();
            S = new Sensor[l + 1];
            Sensor sen;
            for (int a = 0; a < l; a++)
            {
                sen = tmp[a];
                //S[a].InitSenor(new Vector3(sen.X, sen.Y, sen.R), new Color32(173, 255, 47, 255));
                S[a] = tmp[a].Copy();
            }
            TextObject = Instantiate(TextPrefab, SenPool).transform;
            TextObject.localPosition = new Vector3((float)y / CamHeight * -10f + 0.15f, 0, (float)x / CamWidth * -10f);
            S[l].InitSenor(new Vector2Int(x, y), new Vector2Int(h, w), SColor, AColor, new Vector2Int(CamHeight, CamWidth), TextObject, Threshold);
        }
    }

    void UpdateSenors()
    {
        if (S != null)
        {
            int l = S.Length;
            for (int a = 0; a < S.Length; a++)
            {
                data = S[a].Update(data);
            }
        }
    }

    public delegate void SendValuesDelegate(byte Func, ushort Data);
    void SendValues(byte Func, ushort Data)
    {
        string Message = null;

        if (Data < 16)
        {
            Message = "" + (char)Func
                + (char)(15 & (Data));
        } else if (Data < 256)
        {
            Message = "" + (char)Func
                + (char)((15 & (Data)) + 16)
                + (char)((15 & (Data >> 4)) + 16);
                
        } else if (Data < 4096)
        {
            Message = "" + (char)Func
                + (char)((15 & (Data)) + 32)
                + (char)((15 & (Data >> 4)) + 32)
                + (char)((15 & (Data >> 8)) + 32);
        } else
        {
            Message = "" + (char)Func
                + (char)((15 & (Data)) + 48)
                + (char)((15 & (Data >> 4)) + 48)
                + (char)((15 & (Data >> 8)) + 48)
                + (char)((15 & (Data >> 12)) + 48);
        }
        BtPlugin.Call("SendToDevice", Message);

        /*
        Paired.text = "";
        for (int a = 0; a < Message.Length; a++)
        {
            Paired.text = Paired.text + "+" + ((byte)Message[a]).ToString();
        }
        Paired.text = Paired.text + ">";
        */
    }

    void ShowToast(string Message)
    {
        object[] parameters = new object[2];                                                        // Setup the parameters we want to send to our native plugin.    
        parameters[0] = unityActivity;
        parameters[1] = Message;
        BtPlugin.Call("PrintString", parameters);
    }


}
