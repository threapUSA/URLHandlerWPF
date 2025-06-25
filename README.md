Problem: I prefer to browse using Firefox, but in a corporate environment, many intranet sites fail to render properly (or perform passthrough authentication) unless using IE/Edge. Having to copy and paste URLs into the appropriate browser is awkward.

Several years ago, to resolve the above problem, I wrote a simple C# Winforms tool that I named URLHandler. It registered itself in Windows as the default handler for http/https links, and then displayed a small window whenever such a link was clicked (e.g. in an Outlook e-mail).
The original URLHandler had several configuration options - a whitelist of domains/URL regexes to always open in Edge, configurable between Firefox and Chrome, and so on.

Earlier today I realised that it's overkill for most users, and this URLHandlerWPF (yeah, I suck at naming things) is the result. Think of it as the earlier tool but stripped down to the bare minimum. Click on a link in Outlook or whereever and this applet pops up under the mouse cursor with two instantly-recognizable icons for Firefox and Edge. Left-click one or the other to open the URL using that browser.
Right-click either icon to close without opening the URL (i.e., cancel).

Features:
1) Uses WPF framework, which in return for making me pull my hair out wrestling with XAML, has made the UI a little slicker and more modern, with zoom/fade effects etc.
2) Can be cancelled by right-clicking either button if you decide you don't, in fact, want to open the clicked URL.
3) If no URL is passed to it (e.g. "URLHandlerWPF https://www.github.com") it grays out both buttons, and exits after two seconds.
4) can now be registered as a browser (run it without passing it a URL; if it's not running with UAC elevated permissions it'll ask to restart)
5) 
Still not implemented:
1) ~~no mechanism to register it as a browser in Windows. Can only be done at present with Registry hackery.~~
2) Hard-coded to Firefox x64 and Edge (Chromium). No mechanism to select another browser.
3) Minimal UI. Practically no UI, in fact. It's a browser chooser; what do you want, 4K video?
