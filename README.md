# FumenDownloader

## 简介
FumenDownloader是一个Monomod mod模块, 可以将[自制谱网站](https://next.nageki-net.com/net/custom-fumen-list)上的谱面同步并加载到游戏中。

## 功能/流程
- 启动时自动拉取远程谱面缓存清单并与本地缓存比对。
- 按差异执行文件下载、更新、删除，避免每次全量覆盖。
- 自动维护本地缓存文件 `fileCacheList.json`。
- 自动创建/更新 `assets.bytes`，将新增 `ui_jacket_*` 资源注册到资源索引。
- 将自制谱资源目录追加到 DataStudio 加载目录（`<DataFolder>/opt`）。
- 可按发布状态过滤曲目显示（仅显示 `publishState == 2` 的谱面）。
- 运行过程与异常写入日志 `dpFumenDownload.log`。

## 使用方法
1. 将编译产物 `Assembly-CSharp.FumenDownloader.mm.dll` 放到游戏对应的 MonoMod/BepInEx 补丁目录中。
2. 在游戏根目录的 `mu3.ini` 添加或修改 `[FumenDownloader]` 配置节。
3. 启动游戏，模块会在数据加载前自动执行同步。

## 注意
请勿修改`DataFolder`指向的文件夹（如`dpFumenData`）里面的内容，这个文件夹只能由程序自己维护，但你可以随时删除这个文件夹，mod会自己重新同步和下载

### 插件配置 `mu3.ini` 示例
```ini
[FumenDownloader]
Enable=true
DataFolder=dpFumenData
APIUriBase=http://nageki-net.com/fumen/
OnlyPublished=true
```

## 配置选项
| 选项 | 类型 | 默认值 | 说明 |
|---|---|---|---|
| `Enable` | `bool` | `true` | 是否启用 FumenDownloader。关闭后不执行下载与过滤逻辑。 |
| `DataFolder` | `string` | `dpFumenData` | 本地谱面数据根目录（会转为绝对路径）。缓存文件、`opt` 与资源文件都存放在此目录。 |
| `APIUriBase` | `string` | `http://nageki-net.com/fumen/` | 远程 API 根地址。程序会自动补齐末尾 `/`。 |
| `OnlyPublished` | `bool` | `true` | 是否只显示已发布自制谱。 |
