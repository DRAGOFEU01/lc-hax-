using System;
using System.Collections.Generic;
using System.Threading;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Hax
{
    public class MenuUI : MonoBehaviour
    {
        private bool showMenu;
        private bool cursorMode;
        private Rect windowRect = new Rect(50, 50, 750, 650);
        private int tabIndex;
        private Vector2 scrollPosition;

        private bool rainbowMode;
        private float rainbowSpeed = 0.5f;
        private float menuAlpha = 0.95f; 
        private bool customColorMode; 
        private Color customAccentColor = Color.cyan;
        private bool showWatermark = true;
        private bool pulseTitle = false; 
        
        private bool godMode, infiniteStamina, noclip, unlimitedJump;
        private bool stunClick, killClick, invis, hearAll, rapidUse;
        private bool esp, brightVision;
        
        private float speedHack = 1f;
        private float brightness = 1f;
        private float uiScale = 1f;
        
        private string xpInput = "1000";
        private string visitInput = "Titan";
        private string moneyInput = "5000";
        private string quotaInput = "2000";
        private string buyInput = "shovel";
        private string buyQty = "1";
        private string chatInput = "Alastor and Kikou on Top!";
        private string noiseDuration = "30";

        private string poisonDmg = "15";
        private string poisonDur = "30";
        private string poisonDelay = "2";
        
        private bool fakeLag, invertControls, headSpin;
        private bool cameraShake, fovPulse, rainbowScreen;
        private bool timeJitter, fakeFreeze, notifSpam;
        private bool drunkCamera, uiGlitch;
        private bool extremeHeadSpin;
        private float headSpinSpeed = 1000f;
        
        private string spinInput = "10"; 

        private string enemyToSpawn = "Girl";
        private string spawnAmount = "1";

        private bool spammSuit;
        private CancellationTokenSource? spammToken;
        private bool notifications = true;
        private int themeIndex;
        private Key openKey = Key.Insert;

        private Color bgColor, buttonColor, textColor, accentColor;
        private GUIStyle tabStyle = null!; 
        private GUIStyle activeTab = null!; 
        private GUIStyle toggleStyle = null!; 
        private GUIStyle windowStyle = null!; 
        private GUIStyle labelStyle = null!; 
        private GUIStyle boxStyle = null!; 
        private GUIStyle buttonStyle = null!; 
        private GUIStyle textFieldStyle = null!;
        private GUIStyle watermarkStyle = null!; 
        
        private string notif = "";
        private float notifTimer;

        public Transform? playerHead; 

        void Update()
        {
            if (Keyboard.current[Key.LeftCtrl].isPressed && Keyboard.current[Key.RightCtrl].isPressed && Keyboard.current[openKey].wasPressedThisFrame)
            {
                showMenu = true;
                cursorMode = true;
                UpdateCursorState();
                Notify("MENU RÉVEILLÉ (FORCÉ)");
            }

            if (Keyboard.current[openKey].wasPressedThisFrame)
                ToggleMenu();

            if (Keyboard.current[Key.LeftAlt].wasPressedThisFrame)
            {
                cursorMode = !cursorMode;
                UpdateCursorState();
            }

            if (notifTimer > 0) notifTimer -= Time.deltaTime;

            if (rainbowMode)
            {
                Color rainbow = Color.HSVToRGB((Time.time * rainbowSpeed) % 1f, 1f, 1f);
                customAccentColor = rainbow;
            }

            if (timeJitter) Time.timeScale = UnityEngine.Random.Range(0.3f, 1.7f);
            else if (!fakeFreeze) Time.timeScale = 1f;
            if (fakeFreeze) Time.timeScale = 0f;

            if (extremeHeadSpin && playerHead != null)
            {
                playerHead.localRotation *= Quaternion.Euler(0, headSpinSpeed * Time.deltaTime, 0);
            }
            
            if (!spammSuit && spammToken != null) 
            { 
                spammToken.Cancel(); 
                spammToken = null; 
            }
        }

        void ToggleMenu()
        {
            showMenu = !showMenu;
            UpdateCursorState();
        }

        void UpdateCursorState()
        {
            bool shouldShow = showMenu || cursorMode;
            Cursor.visible = shouldShow;
            Cursor.lockState = shouldShow ? CursorLockMode.None : CursorLockMode.Locked;
        }

        void OnGUI()
        {
            ApplyTheme();
            InitStyles();

            if (notifications && notifTimer > 0)
            {
                GUI.color = accentColor;
                GUI.Box(new Rect(15, Screen.height - 55, 610, 50), ""); 
                GUI.Label(new Rect(20, Screen.height - 50, 600, 40), notif, labelStyle);
                GUI.color = Color.white;
            }

            if (!showMenu)
            {
                GUI.backgroundColor = new Color(0, 0, 0, 0.7f);
                GUI.contentColor = accentColor;
                if (GUI.Button(new Rect(Screen.width - 160, Screen.height - 40, 150, 30), "OUVRIR (INS)", buttonStyle))
                {
                    ToggleMenu();
                }
                return;
            }

            float glitchScale = uiGlitch ? UnityEngine.Random.Range(0.99f, 1.01f) : 1f;
            GUI.matrix = Matrix4x4.TRS(Vector3.zero, Quaternion.identity, Vector3.one * uiScale * glitchScale);

            GUI.backgroundColor = bgColor;
            GUI.contentColor = textColor;

            string title = "DRAGO V1";
            if (pulseTitle)
            {
                GUI.color = Color.Lerp(Color.white, accentColor, Mathf.PingPong(Time.time * 2, 1));
            }
            
            windowRect = GUI.Window(0, windowRect, DrawWindow, title, windowStyle);
            GUI.color = Color.white; 
        }

        void DrawWindow(int id)
        {
            DrawTabs();
            GUILayout.Space(10);

            scrollPosition = GUILayout.BeginScrollView(scrollPosition, false, true);

            switch (tabIndex)
            {
                case 0: DrawSelf(); break;
                case 1: DrawRealTimePlayers(); break;
                case 2: DrawWorld(); break;
                case 3: DrawGameAndItems(); break;
                case 4: DrawTrollAndFX(); break;
                case 5: DrawSettings(); break;
            }

            GUILayout.EndScrollView();

            GUILayout.FlexibleSpace();
            GUILayout.BeginHorizontal();
            GUI.backgroundColor = new Color(0.8f, 0.1f, 0.1f, 0.8f);
            if (GUILayout.Button("FERMER", GUILayout.Height(30))) ToggleMenu();
            if (GUILayout.Button("QUITTER LE JEU", GUILayout.Height(30))) Application.Quit();
            GUI.backgroundColor = buttonColor;
            GUILayout.EndHorizontal();
            
            GUI.DragWindow(new Rect(0, 0, 10000, 25));
        }

        void DrawTabs()
        {
            GUILayout.BeginHorizontal();
            DrawTab("PERSO", 0);
            DrawTab("JOUEURS", 1);
            DrawTab("MONDE", 2);
            DrawTab("HOST", 3);
            DrawTab("TROLL", 4);
            DrawTab("CONFIG", 5);
            GUILayout.EndHorizontal();
        }

        void DrawTab(string name, int id)
        {
            GUI.backgroundColor = tabIndex == id ? accentColor : buttonColor;
            GUI.contentColor = tabIndex == id ? Color.black : textColor;

            if (GUILayout.Button(name, tabIndex == id ? activeTab : tabStyle))
            {
                tabIndex = id;
                scrollPosition = Vector2.zero;
            }
            
            GUI.backgroundColor = buttonColor;
            GUI.contentColor = textColor;
        }

        void DrawSelf()
        {
            GUILayout.Label("--- ETAT DU JOUEUR ---", labelStyle);
            DrawToggle(ref godMode, " God Mode (Invincible)", "/god");
            DrawToggle(ref infiniteStamina, " Stamina Infinie", "/stamina");
            DrawToggle(ref noclip, " NoClip (Voler)", "/noclip");
            DrawToggle(ref unlimitedJump, " Sauts Infinis", "/jump");
            DrawToggle(ref rapidUse, " Rapid Fire (Action Rapide)", "/rapid");
            DrawToggle(ref invis, " Invisible", "/invis");
            DrawToggle(ref hearAll, " Tout Entendre (Talkie Global)", "/hear");

            GUILayout.Space(10);
            GUILayout.Label("--- CLICS MAGIQUES ---", labelStyle);
            DrawToggle(ref stunClick, " Clic Gauche = STUN", "/stunclick");
            DrawToggle(ref killClick, " Clic Gauche = KILL", "/killclick");

            GUILayout.Space(10);
            GUILayout.Label("--- MOUVEMENT ---", labelStyle);
            GUILayout.Label($"Vitesse de course: {speedHack:F1}");
            float newSpeed = GUILayout.HorizontalSlider(speedHack, 1f, 10f);
            if (Mathf.Abs(newSpeed - speedHack) > 0.1f) { speedHack = newSpeed; ExecuteCommand($"/speed {speedHack:F1}"); }

            GUILayout.Space(10);
            GUILayout.Label("--- XP & LEVEL ---", labelStyle);
            GUILayout.BeginHorizontal();
            GUILayout.Label("XP:", GUILayout.Width(40));
            xpInput = GUILayout.TextField(xpInput);
            if (GUILayout.Button("DEFINIR")) ExecuteCommand($"/xp {xpInput}");
            GUILayout.EndHorizontal();

            GUILayout.Space(10);
            GUILayout.Label("--- COMBINAISONS ---", labelStyle);
            GUILayout.BeginHorizontal();
            if (GUILayout.Button("Orange")) ExecuteCommand("/suit orange");
            if (GUILayout.Button("Vert")) ExecuteCommand("/suit green");
            if (GUILayout.Button("Hazmat")) ExecuteCommand("/suit hazard");
            if (GUILayout.Button("Pyjama")) ExecuteCommand("/suit pajama");
            GUILayout.EndHorizontal();
            if (GUILayout.Button(spammSuit ? "ARRETER SPAM SUIT" : "LANCER SPAM SUIT"))
            {
                if (spammSuit) { spammSuit = false; spammToken?.Cancel(); }
                else { spammSuit = true; spammToken = new CancellationTokenSource(); RunSpammWear(spammToken.Token); }
            }
        }

        void DrawRealTimePlayers()
        {
            GUILayout.Label("--- JOUEURS EN LIGNE ---", labelStyle);
            string selfName = Helper.LocalPlayer != null ? Helper.LocalPlayer.playerUsername : "";

            GUILayout.BeginHorizontal();
            GUILayout.Label("Config Poison :", GUILayout.Width(100));
            GUILayout.Label("Dmg:", GUILayout.Width(35));
            poisonDmg = GUILayout.TextField(poisonDmg, GUILayout.Width(40));
            GUILayout.Label("Sec:", GUILayout.Width(30));
            poisonDur = GUILayout.TextField(poisonDur, GUILayout.Width(40));
            GUILayout.Label("Tic:", GUILayout.Width(30));
            poisonDelay = GUILayout.TextField(poisonDelay, GUILayout.Width(30));
            GUILayout.EndHorizontal();
            GUILayout.Space(5);

            var players = Helper.Players; 
            if (players == null || players.Length == 0) { GUILayout.Label("Aucun joueur détecté."); return; }

            foreach (var player in players)
            {
                if (player == null) continue;
                string playerName = player.playerUsername;
                bool isMe = player == Helper.LocalPlayer;
                
                GUI.backgroundColor = isMe ? new Color(accentColor.r, accentColor.g, accentColor.b, 0.5f) : buttonColor;
                GUILayout.BeginVertical(boxStyle);
                GUILayout.BeginHorizontal();
                GUILayout.Label($"<b>{playerName}</b> {(isMe ? "(MOI)" : "")}", labelStyle);
                GUILayout.FlexibleSpace();
                if (!isMe)
                {
                    if (GUILayout.Button("TP A", GUILayout.Width(60))) ExecuteCommand($"/tp {playerName}");
                    if (GUILayout.Button("TP ICI", GUILayout.Width(60))) { if (!string.IsNullOrEmpty(selfName)) ExecuteCommand($"/tp {playerName} {selfName}"); }
                }
                GUILayout.EndHorizontal();

                if (!isMe)
                {
                    GUILayout.BeginHorizontal();
                    if (GUILayout.Button("KILL")) ExecuteCommand($"/kill {playerName}");
                    if (GUILayout.Button("BOMB")) ExecuteCommand($"/bomb {playerName}");
                    if (GUILayout.Button("VOID")) ExecuteCommand($"/void {playerName}");
                    if (GUILayout.Button("MASK")) ExecuteCommand($"/mask {playerName}");
                    GUILayout.EndHorizontal();
                    GUILayout.BeginHorizontal();
                    if (GUILayout.Button("CARPET")) ExecuteCommand($"/carpet {playerName}");
                    if (GUILayout.Button("JAIL")) ExecuteCommand($"/jail {playerName}");
                    if (GUILayout.Button("HEAL")) ExecuteCommand($"/heal {playerName}");
                    if (GUILayout.Button("POISON")) ExecuteCommand($"/poison {playerName} {poisonDmg} {poisonDur} {poisonDelay}");
                    if (GUILayout.Button("RANDOM")) ExecuteCommand($"/random {playerName}");
                    GUILayout.EndHorizontal();
                    GUILayout.BeginHorizontal();
                    if (GUILayout.Button("FATALITY (Giant)")) ExecuteCommand($"/fatality {playerName} ForestGiant");
                    if (GUILayout.Button("FATALITY (Jester)")) ExecuteCommand($"/fatality {playerName} Jester");
                    GUILayout.EndHorizontal();
                }
                GUILayout.EndVertical();
                GUI.backgroundColor = buttonColor; 
                GUILayout.Space(5);
            }
        }

        void DrawWorld()
        {
            GUILayout.Label("--- VISION ---", labelStyle);
            DrawToggle(ref esp, " ESP (Wallhack)", "/esp");
            DrawToggle(ref brightVision, " Vision Nocturne", "/bright");
            GUILayout.Label($"Luminosité: {brightness:F1}");
            float newBright = GUILayout.HorizontalSlider(brightness, 0.5f, 5f);
            if (Mathf.Abs(newBright - brightness) > 0.1f) { brightness = newBright; ExecuteCommand($"/brightness {brightness:F1}"); }

            GUILayout.Space(10);
            GUILayout.Label("--- SECURITÉ ---", labelStyle);
            GUILayout.BeginHorizontal();
            if (GUILayout.Button("UNLOCK ALL")) ExecuteCommand("/unlock");
            if (GUILayout.Button("LOCK ALL")) ExecuteCommand("/lock");
            GUILayout.EndHorizontal();
            GUILayout.BeginHorizontal();
            if (GUILayout.Button("Garage")) ExecuteCommand("/garage");
            if (GUILayout.Button("Explose Mines")) ExecuteCommand("/explode mine");
            if (GUILayout.Button("Explose Turrets")) ExecuteCommand("/explode turret");
            if (GUILayout.Button("Berserk")) ExecuteCommand("/berserk");
            GUILayout.EndHorizontal();

            GUILayout.Space(10);
            GUILayout.Label("--- VOYAGE ---", labelStyle);
            GUILayout.BeginHorizontal();
            GUILayout.Label("Lune:", GUILayout.Width(40));
            visitInput = GUILayout.TextField(visitInput);
            if (GUILayout.Button("GO")) ExecuteCommand($"/visit {visitInput}");
            GUILayout.EndHorizontal();
            GUILayout.BeginHorizontal();
            if (GUILayout.Button("Titan")) ExecuteCommand("/visit Titan");
            if (GUILayout.Button("Rend")) ExecuteCommand("/visit Rend");
            if (GUILayout.Button("Artifice")) ExecuteCommand("/visit Artifice");
            if (GUILayout.Button("Dine")) ExecuteCommand("/visit Dine");
            GUILayout.EndHorizontal();
        }

        void DrawGameAndItems()
        {
            GUILayout.Label("--- GESTION PARTIE ---", labelStyle);
            GUILayout.BeginHorizontal();
            if (GUILayout.Button("START")) ExecuteCommand("/start");
            if (GUILayout.Button("LAND")) ExecuteCommand("/land");
            if (GUILayout.Button("ORBIT")) ExecuteCommand("/end");
            GUILayout.EndHorizontal();
            GUILayout.BeginHorizontal();
            if (GUILayout.Button("REVIVE ALL")) ExecuteCommand("/revive");
            if (GUILayout.Button("GODS ALL")) ExecuteCommand("/gods");
            if (GUILayout.Button("EJECT ALL")) ExecuteCommand("/eject");
            GUILayout.EndHorizontal();
            
            GUILayout.Label("--- SPAWNER (HOST) ---", labelStyle);
            GUILayout.BeginHorizontal();
            GUILayout.Label("Mob:", GUILayout.Width(30));
            enemyToSpawn = GUILayout.TextField(enemyToSpawn);
            GUILayout.Label("Qté:", GUILayout.Width(30));
            spawnAmount = GUILayout.TextField(spawnAmount, GUILayout.Width(30));
            if (GUILayout.Button("SPAWN")) ExecuteCommand($"/spawn {enemyToSpawn} me {spawnAmount}");
            GUILayout.EndHorizontal();

            GUILayout.Space(10);
            GUILayout.Label("--- BUILD ---", labelStyle);
            GUILayout.BeginHorizontal();
            if (GUILayout.Button("TP")) ExecuteCommand("/build Teleporter");
            if (GUILayout.Button("Inv-TP")) ExecuteCommand("/build Inverse Teleporter");
            if (GUILayout.Button("CozyLights")) ExecuteCommand("/build Cozy lights");
            if (GUILayout.Button("TV")) ExecuteCommand("/build Television");
            GUILayout.EndHorizontal();
            
            GUILayout.BeginHorizontal();
            if (GUILayout.Button("Toilet")) ExecuteCommand("/build Toilet");
            if (GUILayout.Button("Shower")) ExecuteCommand("/build Shower");
            if (GUILayout.Button("Record")) ExecuteCommand("/build Record player");
            if (GUILayout.Button("Disco")) ExecuteCommand("/build Disco Ball");
            GUILayout.EndHorizontal();

            GUILayout.BeginHorizontal();
            if (GUILayout.Button("Table")) ExecuteCommand("/build Table");
            if (GUILayout.Button("RomantTable")) ExecuteCommand("/build Romantic table");
            if (GUILayout.Button("Translator")) ExecuteCommand("/build Signal translator");
            if (GUILayout.Button("Horn")) ExecuteCommand("/build Loud horn");
            GUILayout.EndHorizontal();

            GUILayout.BeginHorizontal();
            if (GUILayout.Button("JackO")) ExecuteCommand("/build JackOLantern");
            if (GUILayout.Button("Mat")) ExecuteCommand("/build Welcome mat");
            if (GUILayout.Button("Goldfish")) ExecuteCommand("/build Goldfish");
            if (GUILayout.Button("Plushie")) ExecuteCommand("/build Plushie pajama man");
            GUILayout.EndHorizontal();

            GUILayout.BeginHorizontal();
            if (GUILayout.Button("Cupboard")) ExecuteCommand("/build Cupboard");
            if (GUILayout.Button("File Cabinet")) ExecuteCommand("/build File Cabinet");
            if (GUILayout.Button("Bunkbeds")) ExecuteCommand("/build Bunkbeds");
            GUILayout.EndHorizontal();

            GUILayout.Space(10);
            GUILayout.Label("--- ECONOMIE ---", labelStyle);
            GUILayout.BeginHorizontal();
            GUILayout.Label("Crédits:", GUILayout.Width(50));
            moneyInput = GUILayout.TextField(moneyInput);
            if (GUILayout.Button("SET")) ExecuteCommand($"/credit {moneyInput}");
            GUILayout.EndHorizontal();
            GUILayout.BeginHorizontal();
            GUILayout.Label("Quota:", GUILayout.Width(50));
            quotaInput = GUILayout.TextField(quotaInput);
            if (GUILayout.Button("SET")) ExecuteCommand($"/quota {quotaInput}");
            GUILayout.EndHorizontal();

            GUILayout.Space(10);
            GUILayout.Label("--- ACHATS ---", labelStyle);
            GUILayout.BeginHorizontal();
            GUILayout.Label("Item:", GUILayout.Width(40));
            buyInput = GUILayout.TextField(buyInput);
            if (GUILayout.Button("BUY 1")) ExecuteCommand($"/buy {buyInput} 1");
            GUILayout.EndHorizontal();
            GUILayout.BeginHorizontal();
            if (GUILayout.Button("Pelle")) ExecuteCommand("/buy shovel");
            if (GUILayout.Button("Lampe")) ExecuteCommand("/buy pro");
            if (GUILayout.Button("Zap")) ExecuteCommand("/buy zap");
            if (GUILayout.Button("Shotgun")) ExecuteCommand("/buy shotgun");
            GUILayout.EndHorizontal();
            GUILayout.Space(10);
            if (GUILayout.Button("VENDRE TOUT (DESK)")) ExecuteCommand("/sell");
        }

        void DrawTrollAndFX()
        {
            GUILayout.Label("--- CHAT ---", labelStyle);
            chatInput = GUILayout.TextField(chatInput);
            GUILayout.BeginHorizontal();
            if (GUILayout.Button("SAY HOST")) ExecuteCommand($"/say Host {chatInput}");
            if (GUILayout.Button("SIGNAL")) ExecuteCommand($"/signal {chatInput}");
            GUILayout.EndHorizontal();
            if (GUILayout.Button("CLEAR CHAT")) ExecuteCommand("/clear");

            GUILayout.Space(10);
            GUILayout.Label("--- ANNOY ---", labelStyle);
            GUILayout.BeginHorizontal();
            GUILayout.Label("Noise (s):", GUILayout.Width(60));
            noiseDuration = GUILayout.TextField(noiseDuration);
            GUILayout.EndHorizontal();
            if (GUILayout.Button("NOISE SPAM (ALL)")) 
            {
                if (Helper.Players != null) 
                    foreach(var p in Helper.Players) ExecuteCommand($"/noise {p.playerUsername} {noiseDuration}");
            }
            
            GUILayout.BeginHorizontal();
            if (GUILayout.Button("SPIN MAP (10s)")) ExecuteCommand("/spin 10");
            
            GUILayout.Label("Durée:", GUILayout.Width(45));
            spinInput = GUILayout.TextField(spinInput, GUILayout.Width(40));
            if (GUILayout.Button("FAST SPIN")) ExecuteCommand($"/fastspin {spinInput}");
            GUILayout.EndHorizontal();

            if (GUILayout.Button("STUN ENEMIES (5s)")) ExecuteCommand("/stun 5");
            if (GUILayout.Button("CRASH")) ExecuteCommand("/crash");
            if (GUILayout.Button("PANICDOOR")) ExecuteCommand("/panicdoor");

            GUILayout.Space(10);
            GUILayout.Label("--- CLIENT FX ---", labelStyle);
            fakeLag = GUILayout.Toggle(fakeLag, " Fake Lag");
            headSpin = GUILayout.Toggle(headSpin, " Head Spin (Normal)");
            rainbowScreen = GUILayout.Toggle(rainbowScreen, " Rainbow Screen");
            uiGlitch = GUILayout.Toggle(uiGlitch, " UI Glitch");
            GUILayout.Label($"Vitesse Spin: {headSpinSpeed:F0}");
            headSpinSpeed = GUILayout.HorizontalSlider(headSpinSpeed, 100f, 5000f);
            if (GUILayout.Button(extremeHeadSpin ? "STOP EXTREME SPIN" : "START EXTREME SPIN"))
            {
                extremeHeadSpin = !extremeHeadSpin;
                Notify(extremeHeadSpin ? "SPIN ACTIVÉ !" : "Spin arrêté.");
            }
        }

        void DrawSettings()
        {
            GUILayout.Label("--- INTERFACE ---", labelStyle);
            notifications = GUILayout.Toggle(notifications, " Notifications");
            showWatermark = GUILayout.Toggle(showWatermark, " Afficher Watermark");
            pulseTitle = GUILayout.Toggle(pulseTitle, " Animation Titre");
            
            GUILayout.Space(5);
            GUILayout.Label($"Transparence: {menuAlpha:F2}");
            menuAlpha = GUILayout.HorizontalSlider(menuAlpha, 0.2f, 1f);
            
            GUILayout.Label($"Taille UI: {uiScale:F1}");
            uiScale = GUILayout.HorizontalSlider(uiScale, 0.8f, 1.5f);

            GUILayout.Space(10);
            GUILayout.Label("--- THEME & COULEURS ---", labelStyle);
            int newTheme = GUILayout.Toolbar(themeIndex, new[] { "Dark", "Neon", "Classic", "Red" });
            if (newTheme != themeIndex)
            {
                themeIndex = newTheme;
                customColorMode = false; 
                rainbowMode = false;
                ApplyTheme();
            }

            GUILayout.Space(5);
            GUILayout.BeginVertical("box");
            rainbowMode = GUILayout.Toggle(rainbowMode, " 🌈 Mode RGB (Rainbow)");
            if (rainbowMode)
            {
                GUILayout.Label($"Vitesse RGB: {rainbowSpeed:F1}");
                rainbowSpeed = GUILayout.HorizontalSlider(rainbowSpeed, 0.1f, 3f);
            }
            else
            {
                customColorMode = GUILayout.Toggle(customColorMode, " Couleur Personnalisée");
                if (customColorMode)
                {
                    GUILayout.Label("Rouge:");
                    customAccentColor.r = GUILayout.HorizontalSlider(customAccentColor.r, 0f, 1f);
                    GUILayout.Label("Vert:");
                    customAccentColor.g = GUILayout.HorizontalSlider(customAccentColor.g, 0f, 1f);
                    GUILayout.Label("Bleu:");
                    customAccentColor.b = GUILayout.HorizontalSlider(customAccentColor.b, 0f, 1f);
                    GUI.backgroundColor = customAccentColor;
                    GUILayout.Button("Aperçu Couleur");
                    GUI.backgroundColor = buttonColor;
                }
            }
            GUILayout.EndVertical();
            
            GUILayout.Space(20);
            GUILayout.Label("--- PROTECTION ---", labelStyle);
            if (GUILayout.Button("Copier Lobby ID")) ExecuteCommand("/lobby");
            if (GUILayout.Button("Block Radar")) ExecuteCommand("/block radar");
            if (GUILayout.Button("Block Enemy Aim")) ExecuteCommand("/block enemy");

            GUILayout.FlexibleSpace();
            GUI.backgroundColor = new Color(0.8f, 0.2f, 0.2f);
            if (GUILayout.Button("RESET ALL CONFIG"))
                ResetConfig();
            GUI.backgroundColor = buttonColor;
        }

        void DrawToggle(ref bool state, string label, string command)
        {
            bool newState = GUILayout.Toggle(state, label);
            if (newState != state)
            {
                state = newState;
                ExecuteCommand(command);
                Notify($"{command} {(state ? "ACTIVÉ" : "DÉSACTIVÉ")}");
            }
        }

        void ResetConfig()
        {
            godMode = infiniteStamina = noclip = unlimitedJump = esp = brightVision = false;
            stunClick = killClick = invis = hearAll = rapidUse = false;
            fakeLag = invertControls = headSpin = cameraShake = rainbowScreen = uiGlitch = timeJitter = fakeFreeze = notifSpam = drunkCamera = extremeHeadSpin = false;
            uiScale = speedHack = brightness = headSpinSpeed = 1f;
            menuAlpha = 0.95f;
            rainbowMode = false;
            customColorMode = false;
            Notify("Configuration réinitialisée.");
        }

        void Notify(string msg) { notif = msg; notifTimer = 3.0f; }

        void ExecuteCommand(string commandName)
        {
            if (!commandName.StartsWith("/")) commandName = "/" + commandName;
            Chat.ExecuteCommand(commandName);
            if (HUDManager.Instance != null) HUDManager.Instance.SubmitChat_performed(default);
        }

        void ApplyTheme()
        {
            Color baseBg = Color.black; Color baseBtn = Color.gray; Color baseText = Color.white; Color baseAccent = Color.cyan;

            switch (themeIndex)
            {
                case 0: baseBg = new Color(0.1f, 0.1f, 0.1f); baseBtn = new Color(0.2f, 0.2f, 0.2f); baseText = Color.white; baseAccent = Color.cyan; break;
                case 1: baseBg = new Color(0.05f, 0.0f, 0.1f); baseBtn = new Color(0.2f, 0.0f, 0.3f); baseText = Color.green; baseAccent = Color.magenta; break;
                case 2: baseBg = new Color(0.8f, 0.8f, 0.8f); baseBtn = Color.white; baseText = Color.black; baseAccent = Color.blue; break;
                case 3: baseBg = new Color(0.1f, 0.0f, 0.0f); baseBtn = new Color(0.3f, 0.0f, 0.0f); baseText = Color.yellow; baseAccent = Color.red; break;
            }

            bgColor = new Color(baseBg.r, baseBg.g, baseBg.b, menuAlpha);
            buttonColor = baseBtn; textColor = baseText;

            if (rainbowMode || customColorMode) accentColor = customAccentColor;
            else { accentColor = baseAccent; customAccentColor = baseAccent; }
        }

        void InitStyles()
        {
            if (tabStyle != null) return;
            windowStyle = new GUIStyle(GUI.skin.window);
            windowStyle.fontStyle = FontStyle.Bold;
            windowStyle.alignment = TextAnchor.UpperCenter;
            Texture2D tex = new Texture2D(1, 1);
            tex.SetPixel(0, 0, new Color(0,0,0,0.5f));
            tex.Apply();
            windowStyle.onNormal.background = tex;
            tabStyle = new GUIStyle(GUI.skin.button);
            tabStyle.fontSize = 12;
            buttonStyle = new GUIStyle(GUI.skin.button);
            buttonStyle.fontSize = 12; buttonStyle.fontStyle = FontStyle.Bold; buttonStyle.alignment = TextAnchor.MiddleCenter; buttonStyle.normal.textColor = accentColor;
            activeTab = new GUIStyle(tabStyle); activeTab.normal.textColor = accentColor; activeTab.fontStyle = FontStyle.Bold;
            labelStyle = new GUIStyle(GUI.skin.label); labelStyle.fontStyle = FontStyle.Bold; labelStyle.alignment = TextAnchor.MiddleCenter; labelStyle.normal.textColor = accentColor;
            watermarkStyle = new GUIStyle(GUI.skin.label); watermarkStyle.fontStyle = FontStyle.BoldAndItalic; watermarkStyle.fontSize = 14; watermarkStyle.alignment = TextAnchor.UpperLeft;
            boxStyle = new GUIStyle(GUI.skin.box); textFieldStyle = new GUIStyle(GUI.skin.textField); textFieldStyle.alignment = TextAnchor.MiddleCenter;
        }

        async void RunSpammWear(CancellationToken token)
        {
            string[] suits = new string[] { "armor", "hazmat", "spacesuit", "riot", "stealth" };
            try { while (!token.IsCancellationRequested) { foreach (var suit in suits) { if (token.IsCancellationRequested) break; ExecuteCommand($"/suit {suit}"); await System.Threading.Tasks.Task.Delay(100, token); } } }
            catch (OperationCanceledException) { }
        }
    }
}