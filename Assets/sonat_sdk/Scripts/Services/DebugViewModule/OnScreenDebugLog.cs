using System;
using System.Collections.Generic;
using System.Linq;
using Sonat.Debugger;
using Sonat.FirebaseModule;
using Sonat.TrackingModule;
using UnityEngine;

namespace Sonat.DebugViewModule
{
    // ReSharper disable UnusedMember.Local
// ReSharper disable InconsistentNaming

    public class OnScreenDebugLog : ConsoleBase
    {
        public static OnScreenDebugLog Instance;

        private static IList<string> events = new List<string>();

        public bool IsShow;

        public bool ShowButton = true;
        public bool autoHideOnDevice;

        protected override void Awake()
        {
            base.Awake();
            Instance = this;
#if !UNITY_EDITOR
        if(autoHideOnDevice)
            gameObject.SetActive(false);
#endif
        }

        public static void Log(string message)
        {
            SonatDebugType.Tracking.Log(message);
            if (events.Count > 30)
                events.RemoveAt(events.Count - 1);
            if (Instance != null)
                events.Insert(0, string.Format("{0}\n", message));
            else
                SonatDebugType.Tracking.Log("Please set a Instance");
        }

        public static void Clear(string message)
        {
            if (Instance != null)
                events.Clear();
            else
                SonatDebugType.Tracking.Log("Please set a Instance");
        }

        public void Show()
        {
            if (Instance != null)
                Instance.IsShow = !Instance.IsShow;
            else
                SonatDebugType.Tracking.Log("Please set a Instance");
        }

        private float timeClose1;

        protected void OnGUI()
        {
            if (IsShow)
            {
                GUILayout.BeginVertical();
                GUILayout.BeginHorizontal();

                if (this.Button("Close 1"))
                {
                    timeClose1 = Time.time;
                }

                if (this.Button("Clear"))
                {
                    events.Clear();
                }

                if (this.Button("Close 2"))
                {
                    if (Time.time - timeClose1 < 2f)
                    {
                        SonatDebugView.CloseDebugLogScreen();
                    }
                }


                GUILayout.EndHorizontal();
                // GUILayout.BeginHorizontal();
                // if (this.Button("Check"))
                // {
                //     Log("FireBaseController.FireBaseRemoteReady " + SonatFirebase.FirebaseRemoteReady);
                // }
                //
                // // if (this.Button("Fire a test log"))
                // // {
                // //     new SonatLogScreenView().Post();
                // // }
                //
                //
                // GUILayout.EndHorizontal();

                GUILayout.BeginHorizontal();
                if (this.Button("Size +"))
                {
                    DpiScalingFactorEditor = DpiScalingFactorEditor + 10;
                    DpiScalingFactor = DpiScalingFactor + 10;
                    FontScale = FontScale + 0.2f;
                    FontScaleEditor = FontScaleEditor + 0.2f;
                    Clear();
                }

                if (this.Button("Size -"))
                {
                    DpiScalingFactorEditor = DpiScalingFactorEditor - 10;
                    DpiScalingFactor = DpiScalingFactor - 10;
                    FontScale = FontScale - 0.2f;
                    FontScaleEditor = FontScaleEditor - 0.2f;
                    Clear();
                }

                if (this.Button("Color"))
                {
                    Nextcolor();
                    Clear();
                }

                GUILayout.EndHorizontal();


#if UNITY_IOS || UNITY_ANDROID || UNITY_WP8
                if (Input.touchCount > 0 && Input.GetTouch(0).phase == TouchPhase.Moved)
                {
                    Vector2 scrollPosition = this.ScrollPosition;
                    scrollPosition.y += Input.GetTouch(0).deltaPosition.y;
                    this.ScrollPosition = scrollPosition;
                }
#endif
                this.ScrollPosition = GUILayout.BeginScrollView(
                    this.ScrollPosition,
                    GUILayout.MinWidth(ConsoleBase.MainWindowFullWidth));

                GUI.enabled = false;

                GUILayout.TextArea(
                    string.Join("\n", events.ToArray()),
                    this.TextStyle,
                    GUILayout.ExpandHeight(true),
                    GUILayout.MaxWidth(ConsoleBase.MainWindowWidth));
                GUI.enabled = true;

                GUILayout.EndScrollView();

                GUILayout.EndVertical();
            }

            else if (ShowButton)
            {
                GUILayout.BeginVertical();
                GUILayout.BeginHorizontal();
                if (this.Button("Show"))
                {
                    IsShow = true;
                }

                GUILayout.EndHorizontal();
                GUILayout.EndVertical();
            }
        }
    }


    public class ConsoleBase : MonoBehaviour
    {
        public GUISkin GUISkin;


        protected static int ButtonHeight = Screen.height / 40;
        protected static int MainWindowWidth = Screen.width;
        protected static int MainWindowFullWidth = Screen.width;
        protected static int MarginFix = 0;

        public int DpiScalingFactor = 80;
        private static Stack<string> menuStack = new Stack<string>();
        private string status = "Ready";
        private string lastResponse = string.Empty;
        private Vector2 scrollPosition = Vector2.zero;

        // DPI scaling
        private float? scaleFactor;
        private GUIStyle textStyle;
        private GUIStyle buttonStyle;
        private GUIStyle textInputStyle;
        private GUIStyle labelStyle;
        private GUIStyle toggleStyle;

        public void Clear()
        {
            textStyle = null;
            buttonStyle = null;
            textInputStyle = null;
            labelStyle = null;
            toggleStyle = null;
        }

        protected static Stack<string> MenuStack
        {
            get { return ConsoleBase.menuStack; }

            set { ConsoleBase.menuStack = value; }
        }

        protected string Status
        {
            get { return this.status; }

            set { this.status = value; }
        }

        protected Texture2D LastResponseTexture { get; set; }

        protected string LastResponse
        {
            get { return this.lastResponse; }

            set { this.lastResponse = value; }
        }

        protected Vector2 ScrollPosition
        {
            get { return this.scrollPosition; }

            set { this.scrollPosition = value; }
        }

        // Note we assume that these styles will be accessed from OnGUI otherwise the
        // unity APIs will fail.
        protected float ScaleFactor
        {
            get
            {
                if (!this.scaleFactor.HasValue)
                {
#if UNITY_EDITOR
                    this.scaleFactor = Screen.dpi / DpiScalingFactorEditor;
#endif

                    this.scaleFactor = Screen.dpi / DpiScalingFactor;
                }

                return this.scaleFactor.Value;
            }
        }

        protected int FontSize
        {
            get
            {
#if UNITY_EDITOR
                return (int)Math.Round(this.ScaleFactor * 16 * FontScaleEditor);
#endif
                return (int)Math.Round(this.ScaleFactor * 16 * FontScale);
            }
        }

        public float DpiScalingFactorEditor = 50;
        public float FontScaleEditor = 1;
        public float FontScale = 1;


        private static Color[] fontColor = new[] { Color.white, Color.green, Color.yellow, Color.red, Color.cyan, };

        private int _color;

        protected void Nextcolor()
        {
            _color = (_color + 1) % fontColor.Length;
        }

        protected GUIStyle TextStyle
        {
            get
            {
                if (this.textStyle == null)
                {
                    if (GUISkin == null)
                    {
                        this.textStyle = new GUIStyle(GUI.skin.textArea);
                        this.textStyle.alignment = TextAnchor.UpperLeft;
                        this.textStyle.wordWrap = true;
                        this.textStyle.padding = new RectOffset(10, 10, 10, 10);
                        this.textStyle.stretchHeight = true;
                        this.textStyle.stretchWidth = false;
                        this.textStyle.fontSize = this.FontSize;
                        this.textStyle.normal.textColor = fontColor[_color];
                    }
                    else
                    {
                        this.textStyle = new GUIStyle(GUISkin.textArea);
                        this.textStyle.alignment = TextAnchor.UpperLeft;
                        this.textStyle.wordWrap = true;
                        this.textStyle.padding = new RectOffset(10, 10, 10, 10);
                        this.textStyle.stretchHeight = true;
                        this.textStyle.stretchWidth = false;
                        this.textStyle.fontSize = this.FontSize;
                    }
                }

                return this.textStyle;
            }
        }


        protected GUIStyle ButtonStyle
        {
            get
            {
                if (this.buttonStyle == null)
                {
                    this.buttonStyle = new GUIStyle(GUI.skin.button);
                    this.buttonStyle.fontSize = this.FontSize;
                }

                return this.buttonStyle;
            }
        }

        protected GUIStyle TextInputStyle
        {
            get
            {
                if (this.textInputStyle == null)
                {
                    this.textInputStyle = new GUIStyle(GUI.skin.textField);
                    this.textInputStyle.fontSize = this.FontSize;
                }

                return this.textInputStyle;
            }
        }

        protected GUIStyle LabelStyle
        {
            get
            {
                if (this.labelStyle == null)
                {
                    this.labelStyle = new GUIStyle(GUI.skin.label);
                    this.labelStyle.fontSize = this.FontSize;
                }

                return this.labelStyle;
            }
        }

        protected GUIStyle ToggleStyle
        {
            get
            {
                if (this.toggleStyle == null)
                {
                    this.toggleStyle = new GUIStyle(GUI.skin.toggle);
                    this.toggleStyle.fontSize = this.FontSize;
                }

                return this.toggleStyle;
            }
        }

        protected virtual void Awake()
        {
        }

        protected bool Button(string label)
        {
            return GUILayout.Button(
                label,
                this.ButtonStyle,
                GUILayout.MinHeight(ConsoleBase.ButtonHeight * this.ScaleFactor),
                GUILayout.MaxWidth(ConsoleBase.MainWindowWidth));
        }

        protected void LabelAndTextField(string label, ref string text)
        {
            GUILayout.BeginHorizontal();
            GUILayout.Label(label, this.LabelStyle, GUILayout.MaxWidth(200 * this.ScaleFactor));
            text = GUILayout.TextField(
                text,
                this.TextInputStyle,
                GUILayout.MaxWidth(ConsoleBase.MainWindowWidth - 150));
            GUILayout.EndHorizontal();
        }

        protected bool IsHorizontalLayout()
        {
#if UNITY_IOS || UNITY_ANDROID
            return Screen.orientation == ScreenOrientation.LandscapeLeft;
#else
            return true;
#endif
        }

        protected void SwitchMenu(Type menuClass)
        {
            ConsoleBase.menuStack.Push(this.GetType().Name);
            Application.LoadLevel(menuClass.Name);
        }

        protected void GoBack()
        {
            if (ConsoleBase.menuStack.Any())
            {
                Application.LoadLevel(ConsoleBase.menuStack.Pop());
            }
        }
    }
}