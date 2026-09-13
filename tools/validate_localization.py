from pathlib import Path
import re

root = Path(__file__).resolve().parents[1]
localization = (root / "UnityModManager/Localization.cs").read_text(encoding="utf-8")
keys = set(re.findall(r'\{\s*"((?:\\.|[^"\\])*)"\s*,', localization))
refs = set()
for path in [root / "UnityModManager/UI.cs", root / "UnityModManagerApp/Form.cs"]:
    refs.update(re.findall(r'Localization\.Get\("((?:\\.|[^"\\])*)"\)', path.read_text(encoding="utf-8")))
missing = sorted(refs - keys)
if missing:
    raise SystemExit("Missing localization keys: " + ", ".join(missing))

for path in root.rglob("*.cs"):
    text = path.read_text(encoding="utf-8")
    if "Localization.Get(\"" in text and text.count("{") != text.count("}"):
        raise SystemExit(f"Unbalanced braces in {path}")

workflow = (root / ".github/workflows/build-cn.yml").read_text(encoding="utf-8")
required = ["windows-latest", "dotnet restore UnityModManager.sln", "dotnet build UnityModManagerApp/UnityModManagerApp.csproj", "UnityModManager.exe"]
for item in required:
    if item not in workflow:
        raise SystemExit(f"Workflow missing: {item}")

print(f"Validated {len(keys)} localization keys and {len(refs)} code references")
print("Workflow contains Windows Release build and EXE artifact checks")
