using System;
using System.Diagnostics;
using System.IO;
using System.Runtime.InteropServices;
using System.Text;
using System.Text.Json;
using System.Windows;
using Microsoft.Win32;

namespace Project_zhamo
{
    public class LauncherSettings
    {
        public string Username { get; set; } = "ZhamoPlayer";
        public string FortniteDirectory { get; set; } = string.Empty;
        public string ConsoleFilePath { get; set; } = string.Empty;
        public string RedirectFilePath { get; set; } = string.Empty;
        public string MemoryPatchFilePath { get; set; } = string.Empty;
        public string InjectFilePath { get; set; } = string.Empty;
    }

    public partial class MainWindow : Window
    {
        private LauncherSettings settings = new LauncherSettings();
        private string settingsFilePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "launcher_settings.json");

        [DllImport("kernel32.dll")]
        private static extern IntPtr OpenProcess(uint dwDesiredAccess, bool bInheritHandle, int dwProcessId);

        [DllImport("kernel32.dll", SetLastError = true)]
        private static extern IntPtr VirtualAllocEx(IntPtr hProcess, IntPtr lpAddress, uint dwSize, uint flAllocationType, uint flProtect);

        [DllImport("kernel32.dll", SetLastError = true)]
        private static extern bool WriteProcessMemory(IntPtr hProcess, IntPtr lpBaseAddress, byte[] lpBuffer, uint nSize, out int lpNumberOfBytesWritten);

        [DllImport("kernel32.dll")]
        private static extern IntPtr CreateRemoteThread(IntPtr hProcess, IntPtr lpThreadAttributes, uint dwStackSize, IntPtr lpStartAddress, IntPtr lpParameter, uint dwCreationFlags, IntPtr lpThreadId);

        [DllImport("kernel32.dll", SetLastError = true)]
        private static extern IntPtr GetProcAddress(IntPtr hModule, string procName);

        [DllImport("kernel32.dll", SetLastError = true)]
        private static extern IntPtr GetModuleHandle(string lpModuleName);

        [DllImport("kernel32.dll")]
        private static extern bool CloseHandle(IntPtr hObject);

        private const uint PROCESS_ALL_ACCESS = 0x001F0FFF;
        private const uint MEM_COMMIT = 0x1000;
        private const uint MEM_RESERVE = 0x2000;
        private const uint PAGE_READWRITE = 0x04;

        public MainWindow()
        {
            InitializeComponent();
            LoadSettings();
        }

        private void LoadSettings()
        {
            try
            {
                if (File.Exists(settingsFilePath))
                {
                    string json = File.ReadAllText(settingsFilePath);
                    settings = JsonSerializer.Deserialize<LauncherSettings>(json) ?? new LauncherSettings();

                    TxtUsername.Text = settings.Username;
                    if (!string.IsNullOrEmpty(settings.FortniteDirectory))
                        TxtFolderPath.Text = "Folder: " + settings.FortniteDirectory;
                    if (!string.IsNullOrEmpty(settings.ConsoleFilePath))
                        TxtConsoleFile.Text = Path.GetFileName(settings.ConsoleFilePath);
                    if (!string.IsNullOrEmpty(settings.RedirectFilePath))
                        TxtRedirectFile.Text = Path.GetFileName(settings.RedirectFilePath);
                    if (!string.IsNullOrEmpty(settings.MemoryPatchFilePath))
                        TxtMemoryPatchFile.Text = Path.GetFileName(settings.MemoryPatchFilePath);
                    if (!string.IsNullOrEmpty(settings.InjectFilePath))
                        TxtInjectFile.Text = Path.GetFileName(settings.InjectFilePath);

                    TxtStatus.Text = "Status: Loaded saved settings!";
                }
            }
            catch (Exception) { }
        }

        private void SaveSettings()
        {
            try
            {
                settings.Username = TxtUsername.Text.Trim();
                string json = JsonSerializer.Serialize(settings, new JsonSerializerOptions { WriteIndented = true });
                File.WriteAllText(settingsFilePath, json);
            }
            catch (Exception) { }
        }

        private void BtnSelectFolder_Click(object sender, RoutedEventArgs e)
        {
            OpenFolderDialog folderDialog = new OpenFolderDialog { Title = "Select Fortnite Root Directory (v24.20)" };
            if (folderDialog.ShowDialog() == true)
            {
                settings.FortniteDirectory = folderDialog.FolderName;
                TxtFolderPath.Text = "Folder: " + settings.FortniteDirectory;
                SaveSettings();
                TxtStatus.Text = "Status: Fortnite folder saved!";
            }
        }

        private void BtnBrowseConsole_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog dlg = new OpenFileDialog { Filter = "DLL Files (*.dll)|*.dll|All Files (*.*)|*.*" };
            if (dlg.ShowDialog() == true)
            {
                settings.ConsoleFilePath = dlg.FileName;
                TxtConsoleFile.Text = Path.GetFileName(settings.ConsoleFilePath);
                SaveSettings();
            }
        }

        private void BtnBrowseRedirect_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog dlg = new OpenFileDialog { Filter = "DLL Files (*.dll)|*.dll|All Files (*.*)|*.*" };
            if (dlg.ShowDialog() == true)
            {
                settings.RedirectFilePath = dlg.FileName;
                TxtRedirectFile.Text = Path.GetFileName(settings.RedirectFilePath);
                SaveSettings();
            }
        }

        private void BtnBrowseMemoryPatch_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog dlg = new OpenFileDialog { Filter = "DLL Files (*.dll)|*.dll|All Files (*.*)|*.*" };
            if (dlg.ShowDialog() == true)
            {
                settings.MemoryPatchFilePath = dlg.FileName;
                TxtMemoryPatchFile.Text = Path.GetFileName(settings.MemoryPatchFilePath);
                SaveSettings();
            }
        }

        private void BtnBrowseInject_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog dlg = new OpenFileDialog { Filter = "All Files (*.*)|*.*" };
            if (dlg.ShowDialog() == true)
            {
                settings.InjectFilePath = dlg.FileName;
                TxtInjectFile.Text = Path.GetFileName(settings.InjectFilePath);
                SaveSettings();
            }
        }

        private void BtnHostServer_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                string backendFolder = @"C:\Users\zhamo\source\repos\Project zhamo\Project zhamo";

                ProcessStartInfo psi = new ProcessStartInfo
                {
                    FileName = "cmd.exe",
                    Arguments = $"/k node index.js",
                    WorkingDirectory = backendFolder,
                    UseShellExecute = true,
                    CreateNoWindow = false
                };

                Process.Start(psi);
                TxtStatus.Text = "Status: Backend Host Started!";
            }
            catch (Exception ex)
            {
                MessageBox.Show("Failed to start server: " + ex.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                TxtStatus.Text = "Status: Failed to start host";
            }
        }

        private void BtnLaunch_Click(object sender, RoutedEventArgs e)
        {
            SaveSettings();

            if (string.IsNullOrEmpty(settings.FortniteDirectory))
            {
                MessageBox.Show("Please select your Fortnite folder first!", "Error", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            string executablePath = Path.Combine(settings.FortniteDirectory, "FortniteGame", "Binaries", "Win64", "FortniteClient-Win64-Shipping.exe");
            if (!File.Exists(executablePath))
            {
                executablePath = Path.Combine(settings.FortniteDirectory, "FortniteClient-Win64-Shipping.exe");
            }

            if (!File.Exists(executablePath))
            {
                MessageBox.Show("Could not find 'FortniteClient-Win64-Shipping.exe' inside the selected folder.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            string username = string.IsNullOrEmpty(settings.Username) ? "ZhamoPlayer" : settings.Username;
            TxtStatus.Text = $"Status: Launching for {username}...";

            try
            {
                string arguments = $"-epicapp=Fortnite -epicenv=Prod -epiclocale=en-us -epicportal -skippatchcheck -nobe -fromfl=eac -fltoken=3db3ba5dcbd2e16703f3978d -caldera=eyJhbGciOiJFUzI1NiIsInR5cCI6IkpXVCJ9.eyJhY2NvdW50X2lkIjoiYmU5ZGE1YzJmYmVhNDQwN2IyZjQwZWJhYWQ4NTlhZDQiLCJnZW5lcmF0ZWQiOjE2Mzg3MTcyNzgsImNhbGRlcmFHdWlkIjoiMzgxMGI4NjMtMmE2NS00NDU3LTliNTgtNGRhYjNiNDgyYTg2IiwiYWNQcm92aWRlciI6IkVhc3lBbnRpQ2hlYXQiLCJub3RlcyI6IiIsImZhbGxiYWNrIjpmYWxzZX0.VAWQB67RTxhiWOxx7DBjnzDnXyyEnX7OljJm-j2d88G_WgwQ9wrE6lwMEHZHjBd1ISJdUO1UVUqkfLdU5nofBQ -AUTH_LOGIN={username}@projectreboot.dev -AUTH_PASSWORD=Rebooted -AUTH_TYPE=epic";

                ProcessStartInfo startInfo = new ProcessStartInfo
                {
                    FileName = executablePath,
                    Arguments = arguments,
                    WorkingDirectory = Path.GetDirectoryName(executablePath)
                };

                Process gameProcess = Process.Start(startInfo);
                TxtStatus.Text = "Status: Waiting for engine window...";

                System.Threading.Tasks.Task.Run(() =>
                {
                    try
                    {
                        gameProcess.WaitForInputIdle(15000);
                        System.Threading.Thread.Sleep(3000);

                        if (!gameProcess.HasExited)
                        {
                            // Inject ONLY the necessary redirector/memory patch to avoid conflicts
                            if (!string.IsNullOrEmpty(settings.MemoryPatchFilePath) && File.Exists(settings.MemoryPatchFilePath))
                            {
                                InjectDll(gameProcess.Id, settings.MemoryPatchFilePath);
                                System.Threading.Thread.Sleep(1500);
                            }

                            if (!string.IsNullOrEmpty(settings.RedirectFilePath) && File.Exists(settings.RedirectFilePath))
                            {
                                InjectDll(gameProcess.Id, settings.RedirectFilePath);
                            }
                        }
                    }
                    catch { }
                });

                TxtStatus.Text = "Status: Injected safely!";
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Failed to launch game: {ex.Message}", "Launch Error", MessageBoxButton.OK, MessageBoxImage.Error);
                TxtStatus.Text = "Status: Launch failed.";
            }
        }

        private void InjectDll(int processId, string dllPath)
        {
            // Set working directory to the DLL's folder temporarily so it can read local assets/configs without crashing
            string dllDirectory = Path.GetDirectoryName(dllPath);
            if (!string.IsNullOrEmpty(dllDirectory))
            {
                Directory.SetCurrentDirectory(dllDirectory);
            }

            IntPtr hProcess = OpenProcess(PROCESS_ALL_ACCESS, false, processId);
            if (hProcess == IntPtr.Zero) return;

            IntPtr pDllPath = VirtualAllocEx(hProcess, IntPtr.Zero, (uint)((dllPath.Length + 1) * Marshal.SizeOf(typeof(char))), MEM_COMMIT | MEM_RESERVE, PAGE_READWRITE);
            if (pDllPath == IntPtr.Zero)
            {
                CloseHandle(hProcess);
                return;
            }

            byte[] bytes = Encoding.Default.GetBytes(dllPath);
            WriteProcessMemory(hProcess, pDllPath, bytes, (uint)bytes.Length, out _);

            IntPtr hKernel32 = GetModuleHandle("kernel32.dll");
            IntPtr pLoadLibrary = GetProcAddress(hKernel32, "LoadLibraryA");

            IntPtr hThread = CreateRemoteThread(hProcess, IntPtr.Zero, 0, pLoadLibrary, pDllPath, 0, IntPtr.Zero);
            if (hThread != IntPtr.Zero)
            {
                CloseHandle(hThread);
            }

            CloseHandle(hProcess);
        }
    }
}