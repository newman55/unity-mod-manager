using System.Collections.Generic;

namespace UnityModManagerNet
{
    /// <summary>
    /// Built-in Simplified Chinese localization for the installer and the
    /// in-game UnityModManager window. Keys remain in English so upstream
    /// updates can be merged without losing the original context.
    /// </summary>
    public static class Localization
    {
        private static readonly Dictionary<string, string> Chinese = new Dictionary<string, string>
        {
            { "Install", "安装" },
            { "Comment", "说明" },
            { "Extra Files", "额外文件" },
            { "Manual", "手动" },
            { "Auto", "自动" },
            { "Folder", "文件夹" },
            { "Game", "游戏" },
            { "Installation method", "安装方式" },
            { "Restore original files", "恢复原始文件" },
            { "Home Page", "主页" },
            { "Home page", "主页" },
            { "Uninstall", "卸载" },
            { "Select", "选择" },
            { "Ingame Version:", "游戏内版本：" },
            { "Current Version:", "当前版本：" },
            { "Mods", "模组" },
            { "Name", "名称" },
            { "Version", "版本" },
            { "Manager Version", "管理器版本" },
            { "Requirements", "依赖项" },
            { "On/Off", "启用/禁用" },
            { "Status", "状态" },
            { "Update", "更新" },
            { "Revert", "还原" },
            { "Remove", "移除" },
            { "Check Update", "检查更新" },
            { "Open Folder", "打开文件夹" },
            { "Check Updates", "检查更新" },
            { "Install Mod", "安装模组" },
            { "Log", "日志" },
            { "Logs", "日志" },
            { "Settings", "设置" },
            { "Reset firewall rules", "重置防火墙规则" },
            { "For the Game", "游戏" },
            { "For the Installer", "安装器" },
            { "Get API key", "获取 API 密钥" },
            { "API key", "API 密钥" },
            { "Ready", "就绪" },
            { "Updates", "更新" },
            { "Downloading...", "正在下载……" },
            { "Downloading", "正在下载" },
            { "Mod", "模组" },
            { "Change", "更改" },
            { "Game folder", "游戏文件夹" },
            { "(Missing)", "（缺失）" },
            { "(Inactive)", "（未启用）" },
            { "(Outdated)", "（过期）" },
            { "Ready", "就绪" },
            { "Already running", "程序已在运行" },
            { "Notice", "提示" },
            { "For the game", "游戏" },
            { "For the", "用于" },
            { "Select a game.", "请选择游戏。" },
            { "Choose path to the game, for example /Steam/steamapps/common/YourGame", "请选择游戏路径，例如 /Steam/steamapps/common/YourGame" },
            { "Game path", "游戏路径" },
            { "UnityModManager Installer", "UnityModManager 安装器" },
            { "Mod Manager", "模组管理器" },
            { "Filter:", "筛选：" },
            { "Close", "关闭" },
            { "Save", "保存" },
            { "Debug", "调试" },
            { "Reload", "重新加载" },
            { "Options", "选项" },
            { "Available update", "有可用更新" },
            { "Active", "已启用" },
            { "Inactive", "未启用" },
            { "Need restart", "需要重启" },
            { "Errors", "错误" },
            { "Drag window", "拖动窗口" },
            { "Clear", "清空" },
            { "Open detailed log", "打开详细日志" },
            { "Hotkey (default Ctrl+F10)", "快捷键（默认 Ctrl+F10）" },
            { "UMM Hotkey", "UMM 快捷键" },
            { "Check updates", "检查更新" },
            { "Show this window on startup", "启动时显示此窗口" },
            { "Window size", "窗口大小" },
            { "Width", "宽度" },
            { "Height", "高度" },
            { "Apply", "应用" },
            { "UI", "界面" },
            { "Font", "字体" },
            { "Scale", "缩放" },
            { "Mods Hotkeys", "模组快捷键" },
            { "Hotkey", "快捷键" },
            { "Disabled", "禁用" },
            { "Once a day", "每天一次" },
            { "Everytime", "每次启动" },
            { "No", "否" },
            { "Yes", "是" },
            { "Missing", "缺失" },
            { "Outdated", "过期" },
            { "Get API key on nexusmods, this will allow you to receive notifications about new mod versions.", "在 Nexus Mods 获取 API 密钥后，即可接收模组新版本通知。" },
        };

        public static string Get(string key)
        {
            string value;
            return Chinese.TryGetValue(key, out value) ? value : key;
        }
    }
}
