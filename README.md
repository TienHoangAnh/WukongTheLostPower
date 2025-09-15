# WukongTheLostPower

3D Action-Adventure game built with Unity, using Git LFS to manage large assets and (optional) Firebase for Cloud Save.

# Requirements
Git + Git LFS
Unity Hub + Unity correct version
Open Unity and Add project form dick. If the correct version for the project is not available, Unity will ask to install the version.
(Optional) Firebase Unity SDK
(Auth + Firestore)

# ⬇️ Clone project
git clone https://github.com/TienHoangAnh/WukongTheLostPower.git <br>
cd WukongTheLostPower

# Enable Git LFS and download real assets
git lfs install <br>
git lfs pull
<br>
Use PowerShell instead of wc -l <br> 
In PowerShell, you can count the number of lines as follows: <br>
git lfs ls-files | Measure-Object -Line
<br>
Or use find /c /v "" in CMD <br>
git lfs ls-files | find /c /v ""
<br>
If you want to use wc -l properly on Windows <br>
Install Git Bash (included with Git for Windows).
<br>
Open Git Bash and run again: <br>
git lfs ls-files | wc -l
<br>

# Note: <br>
If the clone is finished and the asset in Assets/ is just a small text file (pointer) → you have not run "git lfs pull".

# Open in Unity
Open Unity Hub → Open Project → select the repo folder.
Unity automatically detects the version from ProjectSettings/ProjectVersion.txt.
<br>
The first time you open it, it will take a few minutes to rebuild Library/.

# ☁️ Firebase (Cloud Save) — optional

By default, the project runs using Local Save. <br>
To enable Cloud Save with Firebase: <br>
If the firebase_appconfig.json file is not provided, you need to:

Download the Firebase Unity SDK and import the package: Auth + Firestore. <br>
Create Assets/StreamingAssets/firebase_appconfig.json file with content: <br> 
{<br>
"apiKey": "YOUR_API_KEY",<br>
"appId": "YOUR_APP_ID",<br>
"projectId": "YOUR_PROJECT_ID",<br>
"storageBucket": "YOUR_PROJECT.appspot.com",<br>
"messagingSenderId": "YOUR_SENDER_ID"<br>
}
<br>
⚠️ This file is in .gitignore → do not commit to GitHub.

# In Unity:
Edit → Project Settings → Player → Other Settings → Scripting Define Symbols <br>
Add: <br>
FIREBASE_ENABLED

# 🛠️ Code update process
Get new code (do not create redundant merge commits) <br>
git pull --rebase origin main
Download new LFS assets <br>
git lfs pull
Commit changes <br>
git add .
git commit -m "feat: add new boss"
Push <br>
git push origin main

# Commit Convention Suggestions:
feat: add feature <br>
fix: fix bug <br>
docs: documentation <br>
chore: configuration/cleanup <br>
refactor: change code without affecting logic <br>
perf: optimize performance <br>

# ⚠️ Never Commit
Build/cache folder: Library/, Temp/, Build/, … <br>
Firebase SDK: Assets/Firebase/Plugins/, Assets/ExternalSDKs/ <br>
File secret: <br>
Assets/StreamingAssets/firebase_appconfig.json <br>
google-services.json / GoogleService-Info.plist <br>

# 🐞 Troubleshooting

LFS asset is missing → need to run "git lfs pull" <br>
Push is rejected due to file >100MB → check .gitignore, do not commit heavy SDK/asset <br>
Firebase reports missing config → check firebase_appconfig.json is in StreamingAssets/
