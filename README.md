# 🛠️ SFramework For Unity

<p align="left">
  <img src="https://img.shields.io/badge/Unity-2021.3+-blue.svg?style=flat-square&logo=unity" alt="Unity Version">
  <img src="https://img.shields.io/badge/Platform-Mac%20%7C%20Windows%20%7C%20Linux%20%7C%20Android%20%7C%20Web-green.svg?style=flat-square" alt="Platform">
  <img src="https://img.shields.io/badge/.NET-Framework-red.svg?style=flat-square&logo=.net" alt=".NET Framework">
</p>

SFramework 是一个为 Unity 开发者设计的轻量化、模块化开发框架，涵盖网络通讯、硬件 IO、AI 接入、任务流与架构模式等能力。各模块提供 **Mono 单例**（`SfMonoSingleton<T>`），可在 Inspector 中快速配置，运行时通常一行代码即可调用。

---

## 📦 核心模块说明

### 🌐 SFNet | 网络通讯
> **功能描述：** 底层网络通讯，提供稳定的数据传输与可视化调试。

| 能力 | 说明 |
| :--- | :--- |
| **TCP / UDP** | 服务端 / 客户端 Mono 开箱即用 |
| **HTTP** | 内置 HTTP 服务器 |
| **KCP** | 低延迟可靠 UDP（`SfKcpServerMono`） |

| 支持协议 | 可视化预览 |
| :--- | :--- |
| TCP、UDP、HTTP、KCP | <img src="./SFNet/Editor/Data/NetView.png" width="450" /> |

---

### 🤖 SAI | AI 工作台
> **功能描述：** 多运营商大模型统一接入（Kimi、Qwen、DeepSeek、MiMo 等），OpenAI 兼容协议，支持流式与思考过程展示。

| 能力 | 说明 |
| :--- | :--- |
| **编辑器** | 菜单 `SFramework → AI 工作台`，配置运营商、API Key、模型列表、系统提示词 |
| **运行时** | `SfAiMono` 单例：选运营商、填 Key 与模型名即可对话 |
| **存档** | 可选 `configJson` 字符串，便于读档 / 服务端下发 |

**编辑器：** `SFramework → AI 工作台`

**Mono 一行调用：**

```csharp
SfAiMono.Instance.Ask("你好", reply => Debug.Log(reply));
```

**Inspector 字段：** `provider` · `apiKey` · `model` · `systemPrompt`（无需填写内部 Id）

**读档 / 导出配置：**

```csharp
SfAiMono.Instance.SetConfigJson(savedJson);
string json = SfAiMono.Instance.ExportConfigJson();
```

---

### 📷 SFIO | IO 模块
> **功能描述：** 硬件交互、文件操作、串口与音频处理。

* **硬件交互**：相机画面采集；麦克风录音与 VAD。
* **文件操作**：StreamingAssets、文本、JSON、图片、音频等。
* **串口通讯**：自动主线程分发与协议解析。
* **音频处理**：WAV 加载与保存。

<p align="center">
  <img src="./SFIO/Editor/Data/CameraView.png" width="48%" />
  <img src="./SFIO/Editor/Data/MicrophoneView.png" width="48%" />
</p>

---

### 🗄️ SFDB | 数据库管理
> **功能描述：** 可视化 SQLite 工具，支持在 Editor 中直接操作数据。

* **特性**：开箱即用的 **SQLite** 模块（`SfSqliteMono`）。
* **编辑器**：Unity Editor 内表结构与数据编辑。

<img src="./SFDatabase/Editor/Data/View.png" width="600" />

---

### 📝 SFTask | 任务流系统
> **功能描述：** 模块化、可视化任务编辑器。

* **灵活性**：任务配置可导出为 `.sftask` 文件。
* **动态化**：运行时加载，无需重新编译即可调整逻辑。

<img src="./SFTask/Editor/Data/view.png" width="600" />

---

### 🏗️ SFArchitecture | 架构模式
> **功能描述：** **MVC / MVVM** 与事件总线，便于模块解耦。

* **MVC**：`SfControllerBase`、`SfModelBase`、`SfViewBase`。
* **事件总线**：`SfEventBus` 跨模块通信。

---

### 📄 SFOffice | 办公文档处理
> **功能描述：** Excel、Word、PDF 等文档读写与 Editor 可视化操作。

<p align="center">
  <img src="./SFOffice/Editor/Data/Excel.png" width="30%" />
  <img src="./SFOffice/Editor/Data/Word.png" width="30%" />
  <img src="./SFOffice/Editor/Data/PDF.png" width="30%" />
</p>

---

### 🔄 SFState | 状态机系统
> **功能描述：** 流程 / 状态机，支持完整生命周期（Enter / Update / Exit）。

* **Mono**：`SfProcessMono` 在 Inspector 选择起始流程类型。

---

### 🎨 SFUI | UI 模块
> **功能描述：** UI 工具与序列帧动画组件。

* **序列帧**：`SfSequence` 支持 Sprite / Image / RawImage，可设范围、循环与帧率。

---

## 🧩 Mono 单例（通用约定）

各业务模块的运行时入口继承 `SfMonoSingleton<T>`（`Core/Mono/SfMonoSingleton.cs`）：

| 约定 | 说明 |
| :--- | :--- |
| **Inspector** | 拖组件或自动创建单例 GameObject，字段可视化配置 |
| **调用** | `XxxMono.Instance.方法(...)`，尽量保持一行完成主流程 |
| **自定义 Inspector** | 使用 `SfTitleEditor` 显示模块标题（如 `SFramework AI 模块`） |

| Mono | 模块 |
| :--- | :--- |
| `SfUdpServerMono` / `SfTcpServerMono` / `SfHttpServerMono` / `SfKcpServerMono` | SFNet |
| `SfAiMono` | SAI |
| `SfCameraMono` / `SfMicrophoneMono` / `SfSerialPortMono` | SFIO |
| `SfSqliteMono` | SFDB |
| `SfTaskMono` | SFTask |
| `SfProcessMono` | SFState |

---

## 🛠️ 模块索引 (Quick Link)

| 模块 | 图标 | 状态 |
| :--- | :---: | :--- |
| **SAI** | — | ✅ 稳定 |
| **SFArchitecture** | <img src="./Core/Editor/Config/Architecture.png" width="25" /> | ✅ 稳定 |
| **SFDB** | <img src="./Core/Editor/Config/Database.png" width="25" /> | ✅ 稳定 |
| **SFIO** | <img src="./Core/Editor/Config/IO.png" width="25" /> | ✅ 稳定 |
| **SFNet** | <img src="./Core/Editor/Config/Net.png" width="25" /> | ✅ 稳定 |
| **SFOffice** | <img src="./Core/Editor/Config/Office.png" width="25" /> | ✅ 稳定 |
| **SFState** | <img src="./Core/Editor/Config/State.png" width="25" /> | ✅ 稳定 |
| **SFTask** | <img src="./Core/Editor/Config/Task.png" width="25" /> | ✅ 稳定 |
| **SFUI** | <img src="./Core/Editor/Config/UI.png" width="25" /> | ✅ 稳定 |

> **说明：** 原 **SFMultiple** 中的 KCP 能力已并入 **SFNet**（`SFNet/Module/Kcp`）。
