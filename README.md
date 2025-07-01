__Summary:__ I prefer to browse using Firefox, but in a corporate environment, many intranet sites fail to render properly (usually because they can't perform passthrough authentication) unless using Edge. Sharepoint's a particular offender for this. Having to copy and paste URLs from other applications such as Outlook into the appropriate browser is awkward.

Several years ago, to resolve the above problem, I wrote a simple C# Winforms tool that I named URLHandler. It registered itself in Windows as the default handler for http/https links, and then displayed a small window whenever such a link was clicked (e.g. in an Outlook e-mail).
The original URLHandler had several configuration options - a whitelist of domains/URL regexes to always open in Edge, configurable between Firefox and Chrome, and so on.

Recently I realised that it's ~~overkill and daunting for most users~~ too complicated in its initial appearance for most users, a limitation of the Winforms framework, and this URLHandlerWPF (yeah, I suck at naming things) is the result. Think of it as the earlier tool but reimplemented in WPF to drag it into the 21st Century. Click on a link in Outlook or whereever and this applet pops up under the mouse cursor with just two instantly-recognizable icons for Firefox (or Chrome) and Edge. Left-click one or the other to open the URL using that browser; right-click either icon to open a setup dialog.

__NOTE:__ I wrote this for my own purposes, primarily as a learning exercise in WPF, and it's the first project I've made public on GitHub. Please be forgiving of any n00b mistakes!

__Requires:__ Windows 10 or 11 (tested on both).
__Build Environment:__ Visual Studio 2019 on Windows 10 22H2; requires NuGet package __Microsoft.Windows.SDK.Contracts__ for some WinUI stuff (primarily, getting the Accent color).

__To get started:__
Copy the .exe to a suitable location (e.g. Downloads) and run it by double-clicking its icon.
It'll detect that a) it's not being used to open a link and b) not installed, and offer to register itself as a browser. Because this process needs access to the HKLM tree of the Registry, the app needs to be running with Admin rights to do this; if it isn't, it'll prompt you to re-start it with appropriate rights. Click the "register as browser" button and it'll create the necessary Registry keys and display the Settings -> Apps -> Defaults page for you to select it as the default Browser. From then on it should appear, in the form of two icons, when you click a link in another application.

__Setup Options__
In the setup dialog - displayed on first run to allow registering/installation, and subsequently by right-clicking one of the two browser icons - you can:
* choose which browser to use as the alternative to Edge - currently Firefox and Chrome are detected and offered if present,
* choose to have the applet automatically close ("timeout") after a short period,
* choose to be warned if a URL includes non-ANSI characters,
* enter a list of text patterns; if the clicked link contains any of these, then Edge will be used as the default browser for that link.
* Register the application as a browser, so that it handles links.

Any settings chosen here are saved automatically for the next time the application's used.
(For those who need to know: Settings are saved to the user's appdata-local folder as a .config file.)

If Edge is the default browser - see above - the applet will pulse the Edge icon to show this.
And if the applet is configured to automatically close, Edge will be used to open the link without further interaction.
Hotkeys - press 1 to use the leftmost browser (i.e. Firefox or Chrome), press 2 to use Edge, press Return to select the default if it's set, and press Escape to exit without opening the link.

New feature: If the clicked link contains non-ANSI characters and the appropriate Setup option has been set, both icons will be outlined in a yellow warning glow; clicking once will change this to a RED glow ("last chance!"), and clicking a second time will open the link. This is intended to protect against lookalike glyphs that might be intended to confuse.

__Version History Highlights:__
version 2025.182: implemented new version numbering scheme (year.dayofyear *.hour* ); now displays explanation if it's run from the location from where it's already been installed & registered. Refactored code. Cleaned up the Setup dialog layout, correctly implementing the Grid container.
version 063025: finished implementing Edge whitelisting, with highlighting of the clicked URL in the setup dialog to show when a match has been made. Animated the Edge icon when it's the default.
version 062725: fully supports Dark/Light mode

Features:
1) Uses WPF framework, which in return for making me pull my hair out wrestling with XAML, has made the UI a little slicker and more modern, with zoom/fade effects etc.
2) Can be cancelled by pressing Escape if you decide you don't, in fact, want to open the clicked URL.
3) can now be registered as a browser (run it without passing it a URL; if it's not running with UAC elevated permissions it'll ask to restart)
4) Saves its settings automatically.


Still not implemented:
1) ~~no mechanism to register it as a browser in Windows. Can only be done at present with Registry hackery.~~
2) Hard-coded to Firefox x64 and Edge (Chromium). No mechanism to select another browser.
3) ~~Minimal UI. Practically no UI, in fact. It's a browser chooser; what do you want, 4K video?~~
