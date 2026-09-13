from pathlib import Path

root = Path(__file__).resolve().parents[1]

# Static WinForms designer captions. The original English key is retained in the
# localization call so the source remains easy to compare with upstream.
installer = {
    "Install": "安装",
    "Comment": "说明",
    "Extra Files": "额外文件",
    "Manual": "手动",
    "Auto": "自动",
    "Folder": "文件夹",
    "Game": "游戏",
    "Installation method": "安装方式",
    "Restore original files": "恢复原始文件",
    "Home Page": "主页",
    "Uninstall": "卸载",
    "Select": "选择",
    "Ingame Version:": "游戏内版本：",
    "Current Version:": "当前版本：",
    "Mods": "模组",
    "Name": "名称",
    "Version": "版本",
    "Manager Version": "管理器版本",
    "Status": "状态",
    "Update": "更新",
    "Revert": "还原",
    "Remove": "移除",
    "Check Update": "检查更新",
    "Open Folder": "打开文件夹",
    "Check Updates": "检查更新",
    "Install Mod": "安装模组",
    "Log": "日志",
    "Settings": "设置",
    "Reset firewall rules": "重置防火墙规则",
    "For the Game": "游戏",
    "For the Installer": "安装器",
    "Get API key": "获取 API 密钥",
    "API key": "API 密钥",
    "Ready": "就绪",
    "UnityModManager Installer": "UnityModManager 安装器",
    "Updates": "更新",
    "Downloading...": "正在下载……",
    "Downloading": "正在下载",
    "Mod": "模组",
    "Change": "更改",
    "Game folder": "游戏文件夹",
    "Check updates": "检查更新",
}

# Replace only Text assignments in generated designer files. This keeps control
# names and program logic untouched.
for rel in [
    "UnityModManagerApp/Form.Designer.cs",
    "UnityModManagerApp/DownloadExtraFiles.Designer.cs",
    "UnityModManagerApp/DownloadMod.Designer.cs",
    "UnityModManagerApp/SetFolder.Designer.cs",
    "Updater/Form.Designer.cs",
]:
    path = root / rel
    text = path.read_text(encoding="utf-8")
    for source, _ in installer.items():
        text = text.replace(f'.Text = "{source}";', f'.Text = UnityModManagerNet.Localization.Get("{source}");')
    # Handle the one wrapped explanatory label separately below.
    path.write_text(text, encoding="utf-8")

# Main in-game interface. These are UI captions only; mod-provided names and
# descriptions are deliberately not translated.
ui = {
    '"Mod Manager " + version': 'Localization.Get("Mod Manager") + " " + version',
    '"Filter:"': 'Localization.Get("Filter:")',
    '"Close"': 'Localization.Get("Close")',
    '"Save"': 'Localization.Get("Save")',
    '"Debug"': 'Localization.Get("Debug")',
    '"Reload"': 'Localization.Get("Reload")',
    '"Options"': 'Localization.Get("Options")',
    '"Home page"': 'Localization.Get("Home page")',
    '"Available update"': 'Localization.Get("Available update")',
    '"Active"': 'Localization.Get("Active")',
    '"Inactive"': 'Localization.Get("Inactive")',
    '"Need restart"': 'Localization.Get("Need restart")',
    '"Errors"': 'Localization.Get("Errors")',
    '"Drag window"': 'Localization.Get("Drag window")',
    '"Clear"': 'Localization.Get("Clear")',
    '"Open detailed log"': 'Localization.Get("Open detailed log")',
    '"Hotkey (default Ctrl+F10)"': 'Localization.Get("Hotkey (default Ctrl+F10)")',
    '"UMM Hotkey"': 'Localization.Get("UMM Hotkey")',
    '"Check updates"': 'Localization.Get("Check updates")',
    '"Show this window on startup"': 'Localization.Get("Show this window on startup")',
    '"Window size"': 'Localization.Get("Window size")',
    '"Width "': 'Localization.Get("Width") + " "',
    '"Height"': 'Localization.Get("Height")',
    '"Apply"': 'Localization.Get("Apply")',
    '"UI"': 'Localization.Get("UI")',
    '"Font"': 'Localization.Get("Font")',
    '"Scale"': 'Localization.Get("Scale")',
    '"Mods Hotkeys"': 'Localization.Get("Mods Hotkeys")',
    '"Hotkey"': 'Localization.Get("Hotkey")',
    '"!!!"': '"!!!"',
}
path = root / "UnityModManager/UI.cs"
text = path.read_text(encoding="utf-8")
for source, replacement in ui.items():
    text = text.replace(source, replacement)

# Translate the fixed arrays and tab/column presentation without changing the
# internal tab identifiers used by the switch statement.
text = text.replace('tab = GUILayout.Toolbar(tab, tabs, button, GUILayout.ExpandWidth(false));',
                    'tab = GUILayout.Toolbar(tab, tabs.Select(x => Localization.Get(x)).ToArray(), button, GUILayout.ExpandWidth(false));')
text = text.replace('private static string[] mCheckUpdateStrings = { "Disabled", "Once a day", "Everytime" };',
                    'private static string[] mCheckUpdateStrings = { Localization.Get("Disabled"), Localization.Get("Once a day"), Localization.Get("Everytime") };')
text = text.replace('private static string[] mShowOnStartStrings = { "No", "Yes" };',
                    'private static string[] mShowOnStartStrings = { Localization.Get("No"), Localization.Get("Yes") };')
text = text.replace('GUILayout.Label(mColumns[i].name, colWidth[i]);',
                    'GUILayout.Label(Localization.Get(mColumns[i].name), colWidth[i]);')
path.write_text(text, encoding="utf-8")

# Localize the explanatory label in the installer (preserve its line wrapping).
form = root / "UnityModManagerApp/Form.Designer.cs"
text = form.read_text(encoding="utf-8")
text = text.replace('this.label4.Text = "Get API key on nexusmods, this will allow you to receive notifications about new " +\n                "mod versions.";',
                    'this.label4.Text = UnityModManagerNet.Localization.Get("Get API key on nexusmods, this will allow you to receive notifications about new mod versions.");')
form.write_text(text, encoding="utf-8")

print("Applied localization call sites")
