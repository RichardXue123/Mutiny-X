# Windows Installer

The Unity editor build menu can package the Windows standalone build as a standard Setup EXE using Inno Setup.

## Install Inno Setup

Inno Setup 7 is recommended. On Windows, it can be installed with:

```powershell
winget install --id JRSoftware.InnoSetup.7 -e -s winget -i
```

The Unity build tool automatically searches common Inno Setup 7 and 6 install locations, the `PATH`, and the `INNO_SETUP_COMPILER` environment variable.

If auto-detection fails, use:

`Mutiny X > Build > Configure Inno Setup Compiler...`

and select `ISCC.exe`.

## Build

- `Build Windows`: normal Unity Windows standalone build only.
- `Build Windows Installer`: builds the Windows standalone player and then creates a Setup EXE.
- `Build All Platforms`: builds Windows, creates the Windows installer, then builds Android and iOS.
- `Open Build Folder`: opens the generated output directory.

Installer output:

`Builds/Installer/MutinyX-Setup-<version>+<build>.exe`

The installer includes Start Menu integration, an optional desktop shortcut, uninstall support, and a post-install launch option.
