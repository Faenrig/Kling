using Adb_gui_Apkbox_plugin;
using Components;
using Gma.System.MouseKeyHook;
using Loamen.KeyMouseHook;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Forms;

namespace Kling
{
    public class Context : ApplicationContext
    {
        private readonly KeyMouseFactory _EventHookFactory = new KeyMouseFactory(Hook.GlobalEvents());
        private readonly KeyboardWatcher _KeyboardWatcher;
        
        private ContextMenuStrip _ContextMenustrip;
        private IKeyboardMouseEvents _GlobalHook;
        private System.Drawing.Point _Location;
        private List<MacroEvent> _MacroEvents;
        private SettingsUI _UiSettings;
        private IContainer _Components;
        private NotifyIcon _NotifyIcon;
        private Window _HiddenWindow;
        private Display _KeyUi;
        private Timer _Timer;

        private bool _IsAboutShowing = false;
        private bool _SpecialKeys = false;
        private bool _SuppressKey = false;
        private bool _SuppressKey2 = false;
        private bool _SettingsHowing = false;
        private bool _Notify = true;
        private bool _StdKeys = true;
        private bool _Record = true; 
        private bool _LogKeys = false;
        private int _DisplayTime = 2; 

        public Context()
        {
            _Components = new Container();

            // Load Settings, but first create if not exist
            if (!File.Exists(@"config.ini"))
            {
                var height = SystemInformation.VirtualScreen.Height;
                File.WriteAllText(@"config.ini",
                    "[Settings]" + Environment.NewLine +
                    "displayindex=2" + Environment.NewLine +
                    "xaxis=20" + Environment.NewLine +
                    "yaxis=" + (height - new Display().Height - 60) + Environment.NewLine +
                    "displaytime=2" + Environment.NewLine +
                    "notify=True" + Environment.NewLine +
                    "logkeys=True" + Environment.NewLine +
                    "stdkeys=True" + Environment.NewLine);
            }

            if (File.Exists(@"config.ini"))
            {
                var myini = new IniFile(@"config.ini");

                _Location = new System.Drawing.Point(
                    Convert.ToInt16(myini.Read("xaxis", "Settings")),
                    Convert.ToInt16(myini.Read("yaxis", "Settings"))
                    );

                _DisplayTime = Convert.ToInt16(myini.Read("displaytime", "Settings"));
                _Notify = Convert.ToBoolean(myini.Read("notify", "Settings"));
                _StdKeys = Convert.ToBoolean(myini.Read("stdkeys", "Settings"));
                _LogKeys = Convert.ToBoolean(myini.Read("logkeys", "Settings"));
            }

            _KeyUi = new Display();
            _KeyUi.Location = _Location;

            _ContextMenustrip = new ContextMenuStrip();
            _ContextMenustrip.Items.Add(NewToolStripItem("Stop recording", (o, s) =>
            {
                var firstItem = _ContextMenustrip.Items[0];

                if (_Record)
                {
                    // Stop Recording
                    _Record = false;
                    firstItem.Text = "Start recording";
                    DisplayStatusMessage("Kling : Service stopped");
                }
                else
                {
                    // Start Recording
                    _Record = true;
                    firstItem.Text = "Stop recording";
                    DisplayStatusMessage("Kling : Service started");
                }
            }));

            _ContextMenustrip.Items.Add(new ToolStripSeparator());
            _ContextMenustrip.Items.Add(NewToolStripItem("Settings", ShowSettings));
            _ContextMenustrip.Items.Add(NewToolStripItem("Restart", (o, s) => { System.Windows.Forms.Application.Restart(); }));
            _ContextMenustrip.Items.Add(NewToolStripItem("About", (o, s) =>
            {
                // About screen dialog
                if (!_IsAboutShowing)
                {
                    _IsAboutShowing = true;
                    AboutUI ui = new AboutUI();
                    ui.Closing += (obj, ex) => { _IsAboutShowing = false; };
                    ui.ShowDialog();
                }
            }));

            _ContextMenustrip.Items.Add(new ToolStripSeparator());
            _ContextMenustrip.Items.Add(NewToolStripItem("Exit", (o, s) =>
            {
                // Exit Button
                if (_EventHookFactory != null)
                    _EventHookFactory.Dispose();
                System.Windows.Forms.Application.Exit();
            }));

            _NotifyIcon = new NotifyIcon(_Components)
            {
                ContextMenuStrip = _ContextMenustrip,
                Icon = Properties.Resources.icon,
                Text = "Kling",
                Visible = true,
            };

            _NotifyIcon.DoubleClick += ShowSettings;

            _HiddenWindow = new Window();
            _HiddenWindow.Hide();
            DisplayStatusMessage(_NotifyIcon.Text + ": Start pressing keys");

            _KeyboardWatcher = _EventHookFactory.GetKeyboardWatcher();
            _KeyboardWatcher.OnKeyboardInput += (s, e) =>
            {
                if (!_Record)
                    return;

                if (_MacroEvents != null)
                    _MacroEvents.Add(e);

                if (e.KeyMouseEventType == MacroEventType.KeyPress)
                {
                    var keyEvent = (KeyPressEventArgs)e.EventArgs;

                    if (e.KeyMouseEventType.ToString().Contains("KeyUp"))
                    {
                        // This will also show a form
                        DisplayKeys(getKeys(keyEvent.KeyChar.ToString()));
                    }
                }
                else
                {
                    var keyEvent = (KeyEventArgs)e.EventArgs;

                    if (e.KeyMouseEventType.ToString().Contains("KeyUp"))
                    {
                        // This will show a form
                        var keys = keyEvent.KeyCode;

                        // Suppress next event
                        if (_SuppressKey)
                        {
                            _SuppressKey = false;
                            return;
                        }

                        if (_SuppressKey2)
                        {
                            _SuppressKey2 = false;
                            return;
                        }
                        
                        if (isControlPressed(keyEvent, keys))
                        {
                            _SpecialKeys = true;
                            if (isShiftPressed(keyEvent, keys))
                            {
                                if (isAltPressed(keyEvent, keys))
                                {
                                    _SuppressKey2 = true;
                                    showKey("Ctrl + Shift + Alt + ", keys);
                                }
                                else showKey("Ctrl + Shift + ", keys);
                            }
                            else if (isAltPressed(keyEvent, keys))
                            {
                                if (isShiftPressed(keyEvent, keys))
                                {
                                    _SuppressKey2 = true;
                                    showKey("Ctrl + Alt + Shift + ", keys);
                                }
                                else showKey("Ctrl + Alt + ", keys);
                            }
                            else DisplayKeys("Ctrl + " + getKeys(keys.ToString()));
                        }
                        else if (isAltPressed(keyEvent, keys))
                        {
                            _SpecialKeys = true;
                            if (isShiftPressed(keyEvent, keys))
                            {
                                if (isControlPressed(keyEvent, keys))
                                {
                                    _SuppressKey2 = true;
                                    showKey("Alt + Shift + Ctrl + ", keys);
                                }
                                else showKey("Alt + Shift + ", keys);
                            }
                            else if (isControlPressed(keyEvent, keys)) 
                            {
                                if (isShiftPressed(keyEvent, keys))
                                {
                                    _SuppressKey2 = true;
                                    showKey("Alt + Ctrl + Shift + ", keys);
                                }else 
                                    showKey("Alt + Ctrl + ", keys);
                            }
                            else DisplayKeys("Alt + " + getKeys(keys.ToString()));
                        }
                        else if (isShiftPressed(keyEvent, keys))
                        {
                            _SpecialKeys = true;
                            if (isControlPressed(keyEvent, keys)) 
                            {
                                if (isAltPressed(keyEvent, keys))
                                {
                                    _SuppressKey2 = true;
                                    showKey("Shift + Ctrl + Alt + ", keys);
                                }else showKey("Shift + Ctrl + ", keys);
                            }
                            else if (isAltPressed(keyEvent, keys))
                            {
                                if (isControlPressed(keyEvent, keys))
                                {
                                    _SuppressKey2 = true;
                                    showKey("Shift + Alt + Ctrl + ", keys);
                                }else showKey("Shift + Alt + ", keys);
                            }
                            else DisplayKeys("Shift + " + getKeys(keys.ToString()));
                        }
                        else
                        {
                            if (!_SpecialKeys)
                            {
                                DisplayKeys(getKeys(keys.ToString()));
                            }
                            else _SpecialKeys = false;
                        }
                    }
                }
            };

            _MacroEvents = new List<MacroEvent>();
            _KeyboardWatcher.Start(Hook.GlobalEvents());

            _Timer = new Timer();
            _Timer.Interval = _DisplayTime * 1000;
            _Timer.Tick += async (o, e) =>
            {
                _Timer.Stop();
                while (_KeyUi.Opacity > 0.0)
                {
                    await Task.Delay(20);
                    _KeyUi.Opacity -= 0.05;
                }
                _KeyUi.Opacity = 0;
                _KeyUi.Hide();
                _KeyUi.Opacity = 0.7;
            };
        }

        private List<CodeUI> windows = new List<CodeUI>();

        public void Subscribe()
        {
            _GlobalHook = Hook.GlobalEvents();

            _GlobalHook.MouseDownExt += GlobalHookMouseDownExt;
            _GlobalHook.KeyPress += GlobalHookKeyPress;
            _GlobalHook.KeyDown += M_GlobalHook_KeyDown;
        }

        private void M_GlobalHook_KeyDown(object sender, KeyEventArgs e)
        {
            foreach (var ui in windows)
                ui.PushUp();

            var codeUI = new CodeUI(_DisplayTime).SetText(e.KeyCode.ToString());

            codeUI.Closing += (o, ex) => { windows.Remove(codeUI); };
            codeUI.Show();
            windows.Add(codeUI);
            Debug.WriteLine("KeyDown: " + e.KeyCode.ToString());
        }

        private void GlobalHookKeyPress(object sender, KeyPressEventArgs e)
        {
            Debug.WriteLine("KeyPress: \t{0}", e.KeyChar);
        }

        private void GlobalHookMouseDownExt(object sender, MouseEventExtArgs e)
        {
            Debug.WriteLine("MouseDown: \t{0}; \t System Timestamp: \t{1}", e.Button, e.Timestamp);

            // uncommenting the following line will suppress the middle mouse button click
            // if (e.Buttons == MouseButtons.Middle) { e.Handled = true; }
        }

        public void Unsubscribe()
        {
            _GlobalHook.MouseDownExt -= GlobalHookMouseDownExt;
            _GlobalHook.KeyPress -= GlobalHookKeyPress;

            // It is recommened to dispose it
            _GlobalHook.Dispose();
        }

        private void showKey(string Text, Keys keys)
        {
            _SuppressKey = true;
            DisplayKeys(Text + getKeys(keys.ToString()));
        }

        private bool isControlPressed(KeyEventArgs keyEvent, Keys keys)
        {
            return keyEvent.Control && keys != Keys.RControlKey && keys != Keys.LControlKey &&
                        keys != Keys.Control && keys != Keys.ControlKey;
        }

        private bool isAltPressed(KeyEventArgs keyEvent, Keys keys)
        {
            return keyEvent.Alt && keys != Keys.RMenu && keys != Keys.LMenu &&
                      keys != Keys.Alt;
        }

        private bool isShiftPressed(KeyEventArgs keyEvent, Keys keys)
        {
            return keyEvent.Shift && keys != Keys.Shift && keys != Keys.LShiftKey &&
                    keys != Keys.RShiftKey && keys != Keys.ShiftKey;
        }

        private void DisplayKeys(string Text)
        {
            _KeyUi.Hide();
            _Timer.Stop();

            _KeyUi.SetText(Text);

            if (_LogKeys)
                File.AppendAllText("app.log", $"[{DateTime.Now}] {Text}{Environment.NewLine}");

            _Timer.Start();
            _KeyUi.Show();
        }

        public string getKeys(string Text)
        {
            if (!_StdKeys)
                return Text;

            if (Text.Length == 2)
            {
                if (Text.StartsWith("D"))
                    return Text.Substring(1);
            }
            else if (Text.StartsWith("NumPad"))
            {
                return Text.Substring(6);
            }

            switch (Text)
            {
                case "LMenu":
                    return "Alt";
                case "RMenu":
                    return "Alt";
                case "RControlKey":
                    return "Ctrl";
                case "LControlKey":
                    return "Ctrl";
                case "LShiftKey":
                    return "Shift";
                case "RShiftKey":
                    return "Shift";
                case "LWin":
                    return "Win";
                case "RWin":
                    return "Win";
                case "Add":
                    return "Num +";
                case "Subtract":
                    return "Num -";
                case "Divide":
                    return "Num /";
                case "Multiply":
                    return "Num *";
                // Oem Keys
                case "OemMinus":
                    return "-";
                case "Oemplus":
                    return "=";
                case "Oemtilde":
                    return "`";
                case "Oem5":
                    return "\\";
                case "Oem6":
                    return "]";
                case "OemOpenBrackets":
                    return "[";
                case "Oem1":
                    return ";";
                case "Oem7":
                    return "'";
                case "Oemcomma":
                    return ",";
                case "OemPeriod":
                    return ".";
                case "OemQuestion":
                    return "/";
            }
            return Text;
        }

        private void ShowSettings(object sender, EventArgs e)
        {
            // Show Settings
            if (!_SettingsHowing)
            {
                _UiSettings = new SettingsUI(_KeyUi.Height, _KeyUi.Width);
                _UiSettings.Closing += (o, ex) => { _SettingsHowing = false; };
                _UiSettings.ShowDialog();
            }
        }

        private ToolStripMenuItem NewToolStripItem(string Text, EventHandler handler)
        {
            var item = new ToolStripMenuItem(Text);

            if (handler != null)
                item.Click += handler;

            return item;
        }

        private void DisplayStatusMessage(string text, string message = null)
        {
            _HiddenWindow.Dispatcher.Invoke(delegate
            {
                if (_Notify)
                {
                    _NotifyIcon.BalloonTipText = text;

                    if (message != null)
                        _NotifyIcon.Text = message;

                    // The timeout is ignored on recent Windows
                    _NotifyIcon.ShowBalloonTip(3000);
                }
            });
        }
    }
}
