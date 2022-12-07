# unity_nvim

Unity on nvim-qt


## Install Package

Window -> Package Manager -> Add package from git URL...
```
https://github.com/nagaohiroki/unity_nvim.git?path=unity_nvim/Assets
```

## Setup

Preference... ->  External Tools -> External Script Edtior -> nvim-qt  
```
Windows:
execute:D:\nvim-win64\Neovim\bin\nvim-qt.exe
arguments:$(File) +$(Line)
visual studio:C:\Program Files (x86)\Microsoft Visual Studio\2019\Community\Common7\IDE\devenv.exe

Mac:
execute:/opt/homebrew/bin/nvim-qt
arguments:$(File) +$(Line)
visual studio:/Applications/Visual Studio.app
```
