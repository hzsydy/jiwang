# README

本项目已经开源。[本项目的Github](https://github.com/hzsydy/jiwang)

### 文件结构

-   根目录

    -   /jiwang.sln

        VS 2012 解决方案文件。

    -   /App.config,  /jiwang.csproj

        和项目相关的工程文件。

    -   /.gitignore

        指示版本管理（SVN）程序git的忽略项。

    -   /README.md

        作业要求的README。

    -   /Program.cs

        C# Winform程序的默认入口。


-   /model/

    存储模型（底层）类的文件夹。各文件详细介绍参见报告。

    -   /model/Chatlink.cs

        P2P类。

    -   /model/common.cs

        公用头类。

    -   /model/Listener.cs

        网关类。

    -   /model/ServerLink.cs

        服务器通信类。

-   /view/

    存储界面（顶层）类的文件夹。各文件详细介绍参见报告。

    -   /view/FormMain.cs

        主窗体类。

    -   /view/FormAddFriend.cs

        添加好友窗体类。

    -   /view/FormAddGroup.cs

        添加群聊窗体类。

-   /bin/Debug/

    -   C#工程的生成文件。其中jiwang.exe即为生成的可执行文件。

-   /Properties/

    -   所有文件都是C#自动生成的配置文件，用于记录对工程的配置。