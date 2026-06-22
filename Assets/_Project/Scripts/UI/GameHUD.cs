using System.Collections.Generic;
using Bloomtown.Client.Console;
using Bloomtown.Client.Network;
using Bloomtown.Shared.Protocol;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Bloomtown.Client.UI
{
    /// <summary>
    /// In-game HUD: connection status, notifications, command bar (T), status panel.
    /// </summary>
    public sealed class GameHUD : MonoBehaviour
    {
        private const int MaxNotifications = 8;
        private const float NotificationLifetime = 12f;
        private const string CommandFieldName = "BloomtownCommandField";

        private readonly ClientCommandDispatcher _dispatcher = new();
        private readonly List<Notification> _notifications = new();

        private bool   _commandOpen;
        private string _commandInput = "";
        private bool   _showDetailPanel;
        private string _detailTitle  = "";
        private string _detailBody   = "";
        private Vector2 _detailScroll;
        // Flag untuk auto-focus command field saat bar dibuka.
        // Harus di-consume di dalam OnGUI (bukan Update) karena GUI.FocusControl
        // hanya efektif ketika dipanggil dalam konteks IMGUI rendering.
        private bool _requestFocusCommand;

        private struct Notification
        {
            public string Text;
            public NotificationKind Kind;
            public float ExpireAt;
        }

        private enum NotificationKind { Info, Success, Error }

        // ── Cached GUIStyles (lazy-init di OnGUI pertama kali) ──────────────
        // FIX: GUIStyle tidak boleh dibuat di Awake/Start karena GUI.skin belum tersedia.
        // Lazy-init di OnGUI pertama kali adalah pola yang benar untuk Unity IMGUI.
        private GUIStyle _statusBoxStyle;
        private GUIStyle _notificationStyle;
        private GUIStyle _commandBoxStyle;
        private GUIStyle _commandFieldStyle;
        private GUIStyle _detailPanelStyle;
        private GUIStyle _detailTitleStyle;
        private GUIStyle _detailBodyStyle;
        private bool _stylesInitialized;

        public static GameHUD Instance { get; private set; }

        /// <summary>True saat command bar terbuka — gameplay input di-pause.</summary>
        public bool IsCommandOpen => _commandOpen;

        private void Awake()
        {
            Instance = this;
        }

        private void OnDestroy()
        {
            if (Instance == this) Instance = null;
        }

        private void OnEnable()
        {
            NetworkEvents.OnNpcInteractionResponse += OnNpcInteractionResponse;
            NetworkEvents.OnClientQueryResponse    += OnClientQueryResponse;
            NetworkEvents.OnNpcAmbientComment       += OnNpcAmbientComment;
            NetworkEvents.OnMilestoneNotification   += OnMilestoneNotification;
        }

        private void OnDisable()
        {
            NetworkEvents.OnNpcInteractionResponse -= OnNpcInteractionResponse;
            NetworkEvents.OnClientQueryResponse    -= OnClientQueryResponse;
            NetworkEvents.OnNpcAmbientComment       -= OnNpcAmbientComment;
            NetworkEvents.OnMilestoneNotification   -= OnMilestoneNotification;
        }

        private void Update()
        {
            var keyboard = Keyboard.current;
            if (keyboard != null)
            {
                if (keyboard.tKey.wasPressedThisFrame)
                    SetCommandOpen(!_commandOpen);

                if (_showDetailPanel && keyboard.escapeKey.wasPressedThisFrame)
                    _showDetailPanel = false;
            }

            PruneNotifications();
        }

        private void OnGUI()
        {
            // FIX: Inisialisasi style hanya sekali, bukan setiap OnGUI call.
            // OnGUI dipanggil beberapa kali per frame (Layout, Repaint, input events) —
            // membuat GUIStyle baru setiap kali itu menyebabkan alokasi GC yang tidak perlu.
            if (!_stylesInitialized)
                InitStyles();

            DrawConnectionStatus();
            DrawNotifications();
            DrawCommandBar();
            DrawDetailPanel();
        }

        // ── Network handlers ────────────────────────────────────────────────
        private void OnNpcInteractionResponse(NpcInteractionResponse response)
        {
            var npcLabel = NpcNameLookup.GetDisplayNameOrDefault(response.NpcEntityId);

            if (!response.Success)
            {
                var reason = response.FailureReason == NpcInteractionFailureReason.None
                    ? string.Empty
                    : $" ({response.FailureReason})";
                AddNotification($"{response.Message}{reason}", NotificationKind.Error);
                return;
            }

            var verb = response.Kind == NpcInteractionKind.Greet ? "greeted" : "talked with";
            AddNotification($"You {verb} {npcLabel}.", NotificationKind.Success);
            if (!string.IsNullOrWhiteSpace(response.Message))
                AddNotification($"{npcLabel}: \"{response.Message}\"", NotificationKind.Info);
        }

        private void OnClientQueryResponse(ClientQueryResponse response)
        {
            if (!response.Success)
            {
                AddNotification(response.Message, NotificationKind.Error);
                return;
            }

            _detailTitle = response.Kind switch
            {
                ClientQueryKind.Status => "Status",
                ClientQueryKind.Nearby => "Nearby",
                ClientQueryKind.Goal   => "Goal",
                _                      => "Info",
            };
            _detailBody      = response.Message;
            _showDetailPanel = true;
            _detailScroll    = Vector2.zero;
        }

        private void OnNpcAmbientComment(uint npcEntityId, string comment)
        {
            var name = NpcNameLookup.GetDisplayNameOrDefault(npcEntityId);
            AddNotification($"{name}: \"{comment}\"", NotificationKind.Info);
        }

        private void OnMilestoneNotification(string message)
        {
            if (!string.IsNullOrWhiteSpace(message))
                AddNotification(message, NotificationKind.Success);
        }

        // ── Command dispatch ────────────────────────────────────────────────
        private void SubmitCommand()
        {
            if (string.IsNullOrWhiteSpace(_commandInput)) return;

            var line = _commandInput.Trim();
            _commandInput = "";

            var result = _dispatcher.TryDispatch(line, out var message);
            switch (result)
            {
                case ClientCommandDispatcher.DispatchResult.HandledLocally:
                    _detailTitle     = "Help";
                    _detailBody      = message;
                    _showDetailPanel = true;
                    _detailScroll    = Vector2.zero;
                    break;

                case ClientCommandDispatcher.DispatchResult.SentToServer:
                    if (!string.IsNullOrEmpty(message))
                        AddNotification(message, NotificationKind.Info);
                    SetCommandOpen(false);
                    break;

                case ClientCommandDispatcher.DispatchResult.Failed:
                    AddNotification(message, NotificationKind.Error);
                    break;
            }
        }

        // ── Drawing ─────────────────────────────────────────────────────────
        private void DrawConnectionStatus()
        {
            var nc = NetworkClient.Instance;
            string line = nc != null && nc.IsConnected
                ? $"✅ Connected | Ping: {nc.Ping}ms | T = commands"
                : "⏳ Not connected";

            GUI.Box(new Rect(10, 10, 320, 28), line, _statusBoxStyle);
        }

        private void DrawNotifications()
        {
            if (_notifications.Count == 0) return;

            float y = Screen.height - 20f;
            for (var i = _notifications.Count - 1; i >= 0; i--)
            {
                var n     = _notifications[i];
                var color = n.Kind switch
                {
                    NotificationKind.Success => "#8FE38F",
                    NotificationKind.Error   => "#FF8A8A",
                    _                        => "#E8E8E8",
                };

                var content = new GUIContent($"<color={color}>{n.Text}</color>");
                var height  = _notificationStyle.CalcHeight(content, Screen.width * 0.45f);
                y -= height + 4f;
                GUI.Label(new Rect(10, y, Screen.width * 0.45f, height), content, _notificationStyle);
            }
        }

        private void DrawCommandBar()
        {
            if (!_commandOpen) return;

            const float w = 520f;
            const float h = 32f;
            var x = (Screen.width - w) * 0.5f;
            var y = Screen.height - 80f;

            GUI.Box(new Rect(x - 8, y - 28, w + 16, 68), "Command (Enter = send, T = close)", _commandBoxStyle);

            GUI.SetNextControlName(CommandFieldName);
            _commandInput = GUI.TextField(new Rect(x, y, w, h), _commandInput, _commandFieldStyle);

            // Auto-focus: consume flag di sini (dalam konteks OnGUI) agar kursor
            // langsung masuk ke field tanpa user perlu klik mouse.
            if (_requestFocusCommand)
            {
                GUI.FocusControl(CommandFieldName);
                _requestFocusCommand = false;
            }

            var e = Event.current;
            if (e.type == EventType.KeyDown && e.keyCode == KeyCode.Return)
            {
                SubmitCommand();
                e.Use();
            }
        }

        private void DrawDetailPanel()
        {
            if (!_showDetailPanel) return;

            var panelW = Mathf.Min(Screen.width * 0.55f, 640f);
            var panelH = Mathf.Min(Screen.height * 0.65f, 480f);
            var x      = (Screen.width  - panelW) * 0.5f;
            var y      = (Screen.height - panelH) * 0.5f;

            GUI.Box(new Rect(x, y, panelW, panelH), "");

            GUI.Label(new Rect(x + 12, y + 8, panelW - 80, 24), _detailTitle, _detailTitleStyle);

            if (GUI.Button(new Rect(x + panelW - 72, y + 6, 60, 22), "Close"))
                _showDetailPanel = false;

            var scrollRect = new Rect(x + 12, y + 36, panelW - 24, panelH - 48);
            var contentH   = _detailBodyStyle.CalcHeight(new GUIContent(_detailBody), scrollRect.width - 20f);
            _detailScroll  = GUI.BeginScrollView(scrollRect, _detailScroll,
                new Rect(0, 0, scrollRect.width - 20f, Mathf.Max(contentH, scrollRect.height)));
            GUI.Label(new Rect(0, 0, scrollRect.width - 20f, contentH), _detailBody, _detailBodyStyle);
            GUI.EndScrollView();
        }

        // ── Style initialization (lazy, sekali saja) ────────────────────────
        private void InitStyles()
        {
            _stylesInitialized = true;

            _statusBoxStyle = new GUIStyle(GUI.skin.box)
            {
                fontSize  = 14,
                alignment = TextAnchor.UpperLeft,
                richText  = true,
            };

            _notificationStyle = new GUIStyle(GUI.skin.label)
            {
                fontSize  = 13,
                wordWrap  = true,
                richText  = true,
            };

            _commandBoxStyle = new GUIStyle(GUI.skin.box)
            {
                fontSize = 14,
            };

            _commandFieldStyle = new GUIStyle(GUI.skin.textField)
            {
                fontSize = 16,
            };

            _detailPanelStyle = new GUIStyle(GUI.skin.box);

            _detailTitleStyle = new GUIStyle(GUI.skin.label)
            {
                fontSize  = 16,
                fontStyle = FontStyle.Bold,
            };

            _detailBodyStyle = new GUIStyle(GUI.skin.label)
            {
                fontSize = 13,
                wordWrap = true,
            };
        }

        // ── Helpers ─────────────────────────────────────────────────────────
        private void AddNotification(string text, NotificationKind kind)
        {
            if (string.IsNullOrWhiteSpace(text)) return;

            _notifications.Add(new Notification
            {
                Text     = text,
                Kind     = kind,
                ExpireAt = Time.time + NotificationLifetime,
            });

            while (_notifications.Count > MaxNotifications)
                _notifications.RemoveAt(0);

            Debug.Log($"[GameHUD] {text}");
        }

        private void PruneNotifications()
        {
            var now = Time.time;
            for (var i = _notifications.Count - 1; i >= 0; i--)
            {
                if (_notifications[i].ExpireAt <= now)
                    _notifications.RemoveAt(i);
            }
        }

        private void SetCommandOpen(bool open)
        {
            _commandOpen = open;
            if (open)
            {
                // Set flag agar DrawCommandBar() auto-focus field di OnGUI berikutnya.
                // Tidak bisa panggil GUI.FocusControl di sini (konteks Update, bukan OnGUI).
                _requestFocusCommand = true;
                Cursor.lockState = CursorLockMode.None;
                Cursor.visible   = true;
            }
            else
            {
                _requestFocusCommand = false;
                GUIUtility.keyboardControl = 0;
                _commandInput = string.Empty;
                Cursor.lockState = CursorLockMode.Locked;
                Cursor.visible   = false;
            }
        }
    }
}
