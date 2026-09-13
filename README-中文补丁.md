# UnityModManager 简体中文补丁

本工作副本优先汉化了 Windows 安装器、更新器以及游戏内 UnityModManager 主界面。

## 已完成内容

- 加入 `UnityModManager/Localization.cs`，集中维护简体中文词条。
- 汉化 Windows 安装器的选项卡、按钮、标签、模组操作菜单、设置项和常用运行时提示。
- 汉化更新器和下载窗口的主要标题与状态文本。
- 汉化游戏内主窗口的标签、筛选、保存/关闭、日志、设置、快捷键、状态说明和模组依赖状态。
- 保留 Mod 名称、Mod 作者提供的描述和 Mod 自定义设置，不对第三方内容进行强制翻译。
- 添加 `.github/workflows/build-cn.yml`，在 Windows GitHub Actions 环境中自动构建 Release EXE，并上传 `UnityModManager-Chinese.zip`。

## 自动编译

将本目录推送到 GitHub 后，可以在仓库的 **Actions** 页面手动运行 `Build Chinese UnityModManager`；向 `master` 或 `main` 分支推送代码也会自动触发构建。

构建任务会执行：

1. 在 `windows-latest` 上恢复 NuGet 依赖；
2. 编译 `UnityModManagerApp` 的 Release 配置；
3. 检查 `UnityModManager.exe` 是否生成；
4. 将 EXE、DLL、XML 和相关文件打包为 `UnityModManager-Chinese.zip`。

当前 Linux 沙盒没有 .NET SDK、Windows Desktop 构建工具和 Windows 运行环境，因此本地只完成了源码静态检查；EXE 应通过上述 Windows 工作流生成。

## 许可证

项目原许可证为 MIT。发布汉化版时请保留原项目的版权声明和 MIT License。
