# BeatmapExporter International

A fork of [BeatmapExporter](https://github.com/kabiiQ/BeatmapExporter) with multi-language support, maintained by [SG-Workstation](https://github.com/SG-Workstation).

**Additional features:**
- Multi-language UI (English / 简体中文 / 日本語)
- Switch language on the fly — no restart required
- Language preference persisted across sessions

### Support the Developer (original)

[![ko-fi](https://ko-fi.com/img/githubbutton_sm.svg)](https://ko-fi.com/E1E5AF13X)

For issues or if an update is required, you can create an issue or discussion on GitHub. Alternatively, I can be found through Discord via by [my bot's support server](https://discord.com/invite/ucVhtnh). Though it is not for BeatmapExporter specifically, I do not mind it being used for my other utilities such as this one.

<hr />

# Purpose/Functionality

BeatmapExporter is a program/tool that can mass-export your osu! beatmap library from the modern osu! Lazer storage format.

osu! Lazer does not have a "Songs/" folder as "stable" osu! does. Lazer's files are stored under hashed filenames and other information about the beatmap is contained in a local "Realm" database on your PC.

# Beatmap Export

Exporting beatmaps with a tag in the GUI:

![](https://i.imgur.com/A6SFsR6.png)

# Language Selection

The language can be changed at any time from the **Settings** page → **Language** dropdown:

![](https://i.imgur.com/A6SFsR6.png)

No restart required — the UI updates immediately.

# Download/Usage

Executables are available from the [Releases section here on GitHub](https://github.com/SG-Workstation/BeatmapExporter/releases), also found on the right of the main page (below About). 

If your Lazer database is in the default location (%appdata%\osu), you should be able to simply run the application. If you changed the database location when installing osu! (Lazer), the program will allow you to locate your database.

The directory needed in the Lazer storage contains another directory named "files". This folder can also be opened from in-game if you moved it and are unsure where it is located. 
