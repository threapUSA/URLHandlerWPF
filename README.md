Problem: I prefer to browse using Firefox, but in a corporate environment, many intranet sites fail to render properly (or perform passthrough authentication) unless using IE (and now, Edge). Having to copy and paste URLs into the appropriate browser is awkward.

Several years ago, to resolve the above problem, I wrote a simple C# Winforms tool that I named URLHandler. It registered itself in Windows as the default handler for http/https links, and then displayed a small window whenever such a link was clicked (e.g. in an Outlook e-mail).
The original URLHandler had several configuration options - a whitelist of domains/URL regexes to always open in Edge, configurable between Firefox and Chrome, and so on.

Recently I realised that it's overkill and daunting for most users, and this URLHandlerWPF (yeah, I suck at naming things) is the result. Think of it as the earlier tool but reimplemented in WPF to drag it into the 21st Century. Click on a link in Outlook or whereever and this applet pops up under the mouse cursor with two instantly-recognizable icons for Firefox and Edge. Left-click one or the other to open the URL using that browser.
Right-click either icon to open a setup dialog.
In the setup dialog, you can
* choose which browser to use as the alternative to Edge - currently Firefox and Chrome are detected and offered if present.
* choose to have the applet automatically close after a short period.
* enter a list of text patterns; if the clicked link contains any of these, then Edge will be used as the default browser for that link.

If Edge is the default browser - see above - the applet will pulse the Edge icon to show this.
And if the applet is configured to automatically close, Edge will be used to open the link without further interaction.

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
