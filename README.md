# WukongTheLostPower

3D Action-Adventure game built with Unity, using Git LFS to manage large assets and (optional) Firebase for Cloud Save.

# Requirements
Git + Git LFS

# Unity Hub + Unity correct version
Open Unity and Add project form dick. If the correct version for the project is not available, Unity will ask to install the version.

# (Optional) Firebase Unity SDK
(Auth + Firestore)

# ⬇️ Clone project
git clone https://github.com/TienHoangAnh/WukongTheLostPower.git
cd WukongTheLostPower

# Enable Git LFS and download real assets
git lfs install
git lfs pull

Use PowerShell instead of wc -l
In PowerShell, you can count the number of lines as follows:
git lfs ls-files | Measure-Object -Line

Or use find /c /v "" in CMD
git lfs ls-files | find /c /v ""

If you want to use wc -l properly on Windows
Install Git Bash (included with Git for Windows).

Open Git Bash and run again:
git lfs ls-files | wc -l

Note: If the clone is finished and the asset in Assets/ is just a small text file (pointer) → you have not run "git lfs pull".

# Open in Unity
Open Unity Hub → Open Project → select the repo folder.
Unity automatically detects the version from ProjectSettings/ProjectVersion.txt.

The first time you open it, it will take a few minutes to rebuild Library/.

☁️ Firebase (Cloud Save) — optional

By default, the project runs using Local Save.
To enable Cloud Save with Firebase:
If the firebase_appconfig.json file is not provided, you need to:

Download the Firebase Unity SDK and import the package: Auth + Firestore.
Create Assets/StreamingAssets/firebase_appconfig.json file with content:
{
"apiKey": "YOUR_API_KEY",
"appId": "YOUR_APP_ID",
"projectId": "YOUR_PROJECT_ID",
"storageBucket": "YOUR_PROJECT.appspot.com",
"messagingSenderId": "YOUR_SENDER_ID"
}

⚠️ This file is in .gitignore → do not commit to GitHub.

# In Unity:
Edit → Project Settings → Player → Other Settings → Scripting Define Symbols
Add:
FIREBASE_ENABLED

# 🛠️ Code update process
# Get new code (do not create redundant merge commits)
git pull --rebase origin main
# Download new LFS assets
git lfs pull
# Commit changes
git add .
git commit -m "feat: add new boss"
# Push
git push origin main

# Commit Convention Suggestions:
feat: add feature
fix: fix bug
docs: documentation
chore: configuration/cleanup
refactor: change code without affecting logic
perf: optimize performance

# ⚠️ Never Commit
Build/cache folder: Library/, Temp/, Build/, …
Firebase SDK: Assets/Firebase/Plugins/, Assets/ExternalSDKs/
File secret:
Assets/StreamingAssets/firebase_appconfig.json
google-services.json / GoogleService-Info.plist

🐞 Troubleshooting

LFS asset is missing → need to run "git lfs pull"
Push is rejected due to file >100MB → check .gitignore, do not commit heavy SDK/asset
Firebase reports missing config → check firebase_appconfig.json is in StreamingAssets/
