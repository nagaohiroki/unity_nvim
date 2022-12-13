# unity_nvim

Unity on nvim-qt


## Install Package

Window -> Package Manager -> Add package from git URL...

```
https://github.com/nagaohiroki/unity_nvim.git?path=unity_nvim/Assets
```

## Setup

Preference... ->  External Tools -> External Script Edtior -> choose "nvim-qt"  

```
Windows:
Execute:D:\nvim-win64\Neovim\bin\nvim-qt.exe
Arguments:"$(File)" +$(Line)

Mac:
Execute:/opt/homebrew/bin/nvim-qt
Arguments:"$(File)" +$(Line)
```
