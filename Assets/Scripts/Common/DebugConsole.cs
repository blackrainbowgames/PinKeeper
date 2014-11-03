using System.Text;
using UnityEngine;

namespace Assets.Scripts.Common
{
    public class DebugConsole : Script
    {
        public bool ShowOutput = true;
        public bool ShowStack = true;
        public Rect ButtonRect = new Rect(10, 30, 70, 20);
        public Rect PosRect = new Rect(0, 75, 400, 400);
        public Rect ViewRect = new Rect(0, 0, 400, 60000);
        public bool Show = false;
        Vector2 _scrollPos;

        public void Awake()
        {
            _stringBuilder.AppendLine("Console:");
        }

        public void Update()
        {
            if (!Input.GetKeyDown(KeyCode.BackQuote)) return;

            Show = !Show;
            Debug.Log("~");
        }

        readonly StringBuilder _stringBuilder = new StringBuilder();

        public void OnEnable()
        {
            Application.RegisterLogCallback(HandleLog);
        }

        public void OnDisable()
        {
            Application.RegisterLogCallback(null);
        }
    
        public void HandleLog(string message, string stackTrace, LogType type)
        {
            if (!ShowOutput && !ShowStack) return;

            if (ShowOutput)
            {
                _stringBuilder.AppendLine(message);
            }
            
            if (ShowStack || type == LogType.Exception || type == LogType.Error)
            {
                _stringBuilder.AppendLine(stackTrace);
            }
        }

        public void OnGUI()
        {
            if (GUI.Button(ButtonRect, "Console"))
            {
                Show = !Show;
            }

            if (!Show) return;

            _scrollPos = GUI.BeginScrollView(PosRect, _scrollPos, ViewRect);
            GUI.TextArea(new Rect(0, 0, ViewRect.width - 50, ViewRect.height), _stringBuilder.ToString());
            GUI.EndScrollView();
        }
    }
}