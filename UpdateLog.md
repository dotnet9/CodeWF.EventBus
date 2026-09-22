# 更新日志

## 3.4.5.24 (2026-09-22)

- build: enable nullable reference types in the core package.
- fix: annotate nullable internal state and reflection results explicitly.

## 3.4.5.23 (2026-09-22)

- docs: align supported target frameworks and package output paths with the build configuration.

## 3.4.5.22 (2026-09-22)

- fix: continue scanning loadable types when an assembly has type-load failures.
- fix: validate assembly scanning callback and collection arguments.
- test: cover scanner argument validation.

## 3.4.5.21 (2026-09-22)

- fix: propagate the original exception from synchronous handlers.
- test: verify handler exception identity and type are preserved.

## 3.4.5.20 (2026-09-22)

- fix: reject synchronous publishing when asynchronous handlers are registered.
- docs: clarify when to use synchronous versus asynchronous APIs.
- test: cover synchronous publishing with asynchronous handlers.

## 3.4.5.19 (2026-09-22)

- fix: reject null delegate subscriptions and tolerate null tasks from async handlers.
- test: cover null delegate validation and null task completion.

## 3.4.5.18 (2026-09-22)

- fix: validate scanned handler signatures consistently across all discovery paths.
- fix: honor handler order across manual and assembly-discovered subscriptions.
- test: cover cross-source ordering and invalid automatic handler signatures.

## 3.4.5.17 (2026-09-22)

- fix: use the application entry assembly for default IOC event-handler discovery.
- docs: clarify explicit assembly registration for plugin and test scenarios.

## 3.4.5.16 (2026-09-20)

- 🚀[新增]-NuGet 包统一支持 `net8.0;net10.0;net11.0`，并发布新版本。

## 3.4.5.8 (2026-06-08)

- 🎨[优化]-重新设计根目录 `logo.svg`、`logo.png`、`logo.ico`，使用中心总线节点、事件端点和消息流向表达 NuGet 包的事件总线定位，并保持小尺寸图标可辨识。

## 3.4.5.7 (2026-06-08)

- 🔨[优化]-补齐根目录 logo.svg、logo.png、logo.ico 三件套，子工程通过 MSBuild Link 引用根 logo，避免维护多份图标副本。
- 🔨[优化]-统一目标框架：NuGet 包项目支持 `net8.0;net10.0`，Demo、App、测试与内部应用项目升级到 `net11.0` / `net11.0-windows`。
- 🔨[优化]-将旧 WinForms 示例项目迁移到 SDK 风格 `net11.0-windows`，避免继续依赖 .NET Framework 4.8。
- 🔨[优化]-保留运行时帮助、Markdown 示例、内置备忘录和业务设计文档，仅收敛仓库级重复文档入口。

## 3.4.5.6 (2026-06-08)

- 统一版本号维护入口，只在仓库根目录 `Directory.Build.props` 中定义 `<Version>`。
- 清理英文/双语文档入口，后续仅维护简体中文文档。
- 完善 NuGet 发布配置，补充 Source Link、符号包和标签格式规范。
## 2026-06-08 仓库规范整理

- 统一文档维护入口：每个仓库只保留根目录 `README.md` 和根目录 `UpdateLog.md`，清理重复日志、英文文档和语言切换入口。
- 统一版本维护入口：包版本只在仓库根目录 `Directory.Build.props` 的 `<Version>` 节点维护，移除散落的程序集版本配置。
- 不再维护 `global.json`，SDK 选择交给本机或 CI 环境；NuGet 包和应用的目标框架在项目文件中明确声明。
- 统一 NuGet 包文档入口：包 README 统一引用仓库根 `README.md`，更新日志统一引用仓库根 `UpdateLog.md`。
