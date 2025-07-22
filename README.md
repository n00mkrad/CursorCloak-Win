# CursorCloak-Win
Simple standalone Windows executable to show or hide the mouse cursor. ~~May or may not contain Romulan cloaking technology~~

## Usage

Call CursorCloak.exe from a terminal or script. Append either `hide` or `show` as the first argument.

`CursorCloak hide` - Hide mouse cursor  
`CursorCloak show` - Restore mouse cursor  

### Additional arguments (after first arg, not positional):

`q` - Quiet; do not print messages (except error messages)  
`qq` - Completely quiet  

## Credits

- Mostly based on this AutoHotKey script: https://github.com/steveseguin/hide-cursor
- Disclaimer: AHK2 code was translated into C# by GPT o4-mini-high. It was then reviewed and tested by myself.
