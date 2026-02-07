# Quest Deployment Troubleshooting

## Quick Diagnosis

**What exactly happens when you try to run it?**
- [ ] App launches but shows on laptop screen (not in headset)
- [ ] App crashes immediately on Quest
- [ ] App doesn't launch at all
- [ ] App launches but black screen in headset
- [ ] App launches but no tracking/controllers

## Fix 1: Verify XR Settings in Unity

**In Unity Editor:**
1. Go to **Edit > Project Settings > XR Plug-in Management**
2. Click **Android** tab (not Standalone)
3. Make sure **Oculus** is checked ✅
4. Click **Oculus** to expand settings
5. Verify:
   - ✅ **Initialize XR on Startup** is checked
   - ✅ **Target Devices** includes Quest 3 and Quest Pro

**If Initialize XR on Startup is NOT checked:**
- Check it and rebuild

## Fix 2: Check Quest Connection

**On your Quest:**
1. Put on headset
2. Go to **Settings > Developer**
3. Make sure **Developer Mode** is ON
4. Make sure **USB Connection Dialog** is enabled

**On your computer:**
1. Connect Quest via USB
2. Put on headset - you should see "Allow USB debugging?" popup
3. Click **Allow** and check "Always allow from this computer"
4. In Unity, go to **Edit > Project Settings > XR Plug-in Management > Oculus**
5. You should see your Quest listed if connected

## Fix 3: Verify Build Settings

**In Unity:**
1. **File > Build Settings**
2. Platform: **Android** (should be selected)
3. **Player Settings** button
4. Under **Other Settings**:
   - ✅ **Minimum API Level**: Android 7.0 (API 24) or higher
   - ✅ **Target API Level**: Automatic (or API 32)
5. Under **XR Settings**:
   - ✅ **Virtual Reality Supported** should be checked
   - ✅ **Oculus** should be in the list

## Fix 4: Clean Build

**If app crashes or doesn't launch:**

1. **Close Unity**
2. Delete these folders:
   - `Library/BuildProfiles`
   - `Library/BuildProfileContext.asset`
   - `Temp` folder (if exists)
3. **Reopen Unity**
4. **File > Build Settings**
5. Click **Build** (not Build and Run)
6. Install APK manually:
   - Use `adb install -r YourApp.apk` in command prompt
   - Or drag APK to Quest and install via file manager

## Fix 5: Check Logs

**To see what's happening:**

1. Connect Quest via USB
2. Open Command Prompt/Terminal
3. Run: `adb logcat -s Unity`
4. Launch app on Quest
5. Watch for errors in the log

**Common errors to look for:**
- `XR initialization failed`
- `OVRPlugin not found`
- `Permission denied`
- `ClassNotFoundException`

## Fix 6: Force VR Mode

**If app shows on laptop screen instead of headset:**

This usually means XR didn't initialize. Try:

1. **In Unity, add this script** to a GameObject in your scene:

```csharp
using UnityEngine;
using UnityEngine.XR;

public class ForceVRMode : MonoBehaviour
{
    void Start()
    {
        StartCoroutine(LoadDevice("OpenXR"));
    }

    System.Collections.IEnumerator LoadDevice(string newDevice)
    {
        XRSettings.LoadDeviceByName(newDevice);
        yield return null;
        XRSettings.enabled = true;
    }
}
```

2. **Or manually enable in code:**
   - Add to MosaicGazeManager1 Start() method:
   ```csharp
   if (!XRSettings.enabled)
   {
       XRSettings.enabled = true;
   }
   ```

## Fix 7: Verify Scene Has OVRCameraRig

**In MosaicGazeScene:**
1. Make sure **OVRCameraRig** exists in hierarchy
2. It should be at root level or under a parent
3. It should have:
   - LeftEyeAnchor
   - RightEyeAnchor  
   - CenterEyeAnchor
   - OVRManager component

**If OVRCameraRig is missing:**
- Go to **GameObject > XR > OVR Camera Rig**
- Add it to scene
- Make sure it's at position (0, 0, 0)

## Fix 8: Check Android Manifest

**The manifest looks correct, but verify:**
- File: `Assets/Plugins/Android/AndroidManifest.xml`
- Should have: `<category android:name="com.oculus.intent.category.VR" />`
- ✅ Already present - this is correct

## Most Common Issue: App Shows on Laptop Screen

**This means XR isn't initializing. Try:**

1. **Check XR Settings** (Fix 1 above)
2. **Add ForceVRMode script** (Fix 6 above)
3. **Rebuild completely** (Fix 4 above)
4. **Check logs** for XR initialization errors

## Still Not Working?

**Check these:**
- [ ] Quest firmware is up to date
- [ ] Unity version is compatible (Unity 6 should work)
- [ ] Meta XR SDK version matches (83.0.4)
- [ ] No other apps running on Quest
- [ ] Quest has enough storage space
- [ ] Quest is not in sleep mode

**Get detailed logs:**
```bash
adb logcat > quest_logs.txt
```
Then launch app and check the file for errors.
