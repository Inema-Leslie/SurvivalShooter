$unityPath = "C:\Program Files\Unity\Hub\Editor\6000.4.5f1\Editor\Unity.exe"
$projectPath = "c:\Users\LENOVO\OneDrive\Desktop\SurvivalShoooter"
$logPath = "c:\Users\LENOVO\OneDrive\Desktop\SurvivalShoooter\build_android.log"
$adbPath = "C:\Program Files\Unity\Hub\Editor\6000.4.5f1\Editor\Data\PlaybackEngines\AndroidPlayer\SDK\platform-tools\adb.exe"
$apkPath = "c:\Users\LENOVO\OneDrive\Desktop\SurvivalShoooter\Builds\Android\SurvivalShooter.apk"
$argsList = @("-batchmode", "-quit", "-projectPath", $projectPath, "-executeMethod", "SurvivalShooter.Editor.AndroidBuildDeploy.BuildAndroidApk", "-logFile", $logPath)

if (Test-Path $logPath) {
    Remove-Item -Force $logPath
}

Write-Host "Starting Unity Android Build with OpenGLES3..."
$proc = Start-Process -FilePath $unityPath -ArgumentList $argsList -PassThru -Wait
Write-Host "Unity exited with code: $($proc.ExitCode)"

if ($proc.ExitCode -eq 0 -and (Test-Path $apkPath)) {
    Write-Host "Build succeeded! Installing APK to connected device..."
    & $adbPath install -r $apkPath
    Write-Host "Granting CAMERA permission..."
    & $adbPath shell pm grant com.InemaLeslie.SurvivalShooter android.permission.CAMERA
    Write-Host "Waking screen & launching game..."
    & $adbPath shell input keyevent 224
    & $adbPath shell wm dismiss-keyguard
    & $adbPath shell input swipe 500 1500 500 500
    & $adbPath shell am start -n com.InemaLeslie.SurvivalShooter/com.unity3d.player.UnityPlayerGameActivity
    Write-Host "Survival Shooter successfully launched on device!"
} else {
    Write-Host "Build failed or APK missing. Last 30 lines of build log:"
    if (Test-Path $logPath) {
        Get-Content $logPath -Tail 30
    }
}
