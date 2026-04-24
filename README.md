# WebKit — Unity HTTP Client Library

---

## English

### Overview

WebKit is a lightweight Unity HTTP client built on top of `UnityWebRequest` with:
- **Async/await** support via [UniTask](https://github.com/Cysharp/UniTask)
- **Fluent builder** API for constructing requests
- **Decorator chain** for composable request augmentation (auth headers, HTTPS certificates)
- **Retry policies** — pluggable, async-capable retry logic
- **JSON** serialization/deserialization via Newtonsoft.Json

---

### Installation

Add to your project's `Packages/manifest.json`:

```json
{
  "dependencies": {
    "com.cgk.webkit": "https://github.com/YOUR_REPO.git?path=Assets/Scripts/WebKit",
    "com.cysharp.unitask": "https://github.com/Cysharp/UniTask.git?path=src/UniTask/Assets/Plugins/UniTask",
    "com.unity.nuget.newtonsoft-json": "3.2.1",
    "com.suvorov.common": "https://github.com/Suvorovm/CGK.git#0.0.60"
  }
}
```

---

### Quick Start

#### 1. Initialize the sender

```csharp
using WebKit.Controller;
using WebKit.Decorator;

var sender = new UnityWebRequestSender();
sender.Init("https://api.example.com", decorator: null);
```

#### 2. Build and send a GET request

```csharp
using WebKit.Model;

var request = RequestBuilder.Create()
    .SetPath("/users")
    .SetRequestType(RequestType.Get)
    .SetTimeOut(10)
    .AddParam("page", "1")
    .AddParam("limit", "20")
    .Build();

var response = await sender.MakeRequest<UsersResponse>(request);
```

#### 3. POST request with a body

```csharp
var loginRequest = RequestBuilder.Create()
    .SetPath("/auth/login")
    .SetRequestType(RequestType.Post)
    .SetBody(new { username = "player1", password = "secret" })
    .Build();

var loginResponse = await sender.MakeRequest<LoginResponse>(loginRequest);
```

---

### RequestBuilder API

| Method | Description | Default |
|---|---|---|
| `SetPath(string)` | Endpoint path appended to root host | — |
| `SetRequestType(RequestType)` | `Get`, `Post`, `Put`, `Patch`, `Delete` | — |
| `SetBody(object)` | Request body, serialized to JSON | `null` |
| `AddParam(string, string)` | Appends a query parameter | — |
| `SetParams(Dictionary<string,string>)` | Adds multiple query parameters at once | — |
| `SetTimeOut(int)` | Timeout in seconds | `30` |
| `SetMaxRetryCount(int)` | Override max retry attempts | `3` |
| `Build()` | Returns the `RequestModel` | — |

---

### Decorators

Decorators wrap `UnityWebRequest` before it is sent. They are composable via constructor chaining.

#### AuthDecorator — Bearer token

```csharp
using WebKit.Decorator;

var decorator = new AuthDecorator(innerRequestDecorator: null, token: "eyJhbGci...");
sender.Init("https://api.example.com", decorator);
```

#### HttpsRequestDecorator — custom certificate handler

```csharp
using WebKit.Certificate;
using WebKit.Decorator;

var httpsDecorator = new HttpsRequestDecorator(new CustomCertificateHandler());
sender.Init("https://api.example.com", httpsDecorator);
```

#### Chaining decorators (HTTPS + Auth)

```csharp
var httpsDecorator = new HttpsRequestDecorator(new CustomCertificateHandler());
var authDecorator  = new AuthDecorator(innerRequestDecorator: httpsDecorator, token: userToken);

sender.Init("https://api.example.com", authDecorator);
// Both certificate and Authorization header will be applied on every request.
```

#### Per-request decorator override

You can pass a decorator directly to `MakeRequest` to override the global one for a single call:

```csharp
var tempDecorator = new AuthDecorator(null, token: refreshToken);
var result = await sender.MakeRequest<TokenResponse>(request, decorator: tempDecorator);
```

---

### Retry Policies

Implement `IRetryPolicy` to define when and how to retry a failed request.

```csharp
using Cysharp.Threading.Tasks;
using UnityEngine.Networking;
using WebKit.Controller;

public class TokenRefreshRetryPolicy : IRetryPolicy
{
    private readonly ITokenService _tokenService;
    private readonly UnityWebRequestSender _sender;

    public TokenRefreshRetryPolicy(ITokenService tokenService, UnityWebRequestSender sender)
    {
        _tokenService = tokenService;
        _sender = sender;
    }

    // Trigger only on 401 Unauthorized
    public bool ShouldHandle(UnityWebRequest failedRequest) =>
        failedRequest.responseCode == 401;

    // Refresh token, then update the decorator so the retry uses the new token
    public async UniTask<bool> PrepareRetryAsync()
    {
        string newToken = await _tokenService.RefreshAsync();
        if (string.IsNullOrEmpty(newToken))
            return false;

        _sender.SetDecorator(new AuthDecorator(null, newToken));
        return true;
    }
}
```

Register the policy:

```csharp
sender.AddRetryPolicy(new TokenRefreshRetryPolicy(tokenService, sender));
```

---

### Error Handling

All HTTP errors throw `WebRequestError` which carries the HTTP status code.

```csharp
using WebKit.Error;

try
{
    var data = await sender.MakeRequest<UserProfile>(request);
}
catch (WebRequestError e)
{
    Debug.LogError($"HTTP {e.HttpStatusCode}: {e.Message}");

    if (e.HttpStatusCode == 404)
    {
        // handle not found
    }
}
```

---

### Full Example

```csharp
using Cysharp.Threading.Tasks;
using UnityEngine;
using WebKit.Certificate;
using WebKit.Controller;
using WebKit.Decorator;
using WebKit.Error;
using WebKit.Model;

public class ApiService : MonoBehaviour
{
    private UnityWebRequestSender _sender;

    private void Awake()
    {
        _sender = new UnityWebRequestSender();

        var decorator = new HttpsRequestDecorator(new CustomCertificateHandler());
        _sender.Init("https://api.example.com", decorator);
        _sender.AddRetryPolicy(new TokenRefreshRetryPolicy(tokenService, _sender));
    }

    public async UniTask<UserProfile> GetProfileAsync(string userId)
    {
        var request = RequestBuilder.Create()
            .SetPath($"/users/{userId}")
            .SetRequestType(RequestType.Get)
            .SetTimeOut(15)
            .Build();

        try
        {
            return await _sender.MakeRequest<UserProfile>(request);
        }
        catch (WebRequestError e)
        {
            Debug.LogError($"Failed to load profile: {e.HttpStatusCode} — {e.Message}");
            throw;
        }
    }

    public async UniTask UpdateScoreAsync(string userId, int score)
    {
        var request = RequestBuilder.Create()
            .SetPath("/scores")
            .SetRequestType(RequestType.Post)
            .SetBody(new { userId, score })
            .SetMaxRetryCount(1)
            .Build();

        await _sender.MakeRequest(request);
    }
}
```

---
---

## Русский

### Обзор

WebKit — лёгкий HTTP-клиент для Unity поверх `UnityWebRequest` с поддержкой:
- **async/await** через [UniTask](https://github.com/Cysharp/UniTask)
- **Fluent-builder** для построения запросов
- **Цепочки декораторов** для гибкого оформления запросов (заголовки авторизации, HTTPS-сертификаты)
- **Политик повтора (Retry Policy)** — подключаемая асинхронная логика повторных попыток
- **JSON** сериализация/десериализация через Newtonsoft.Json

---

### Установка

Добавьте в `Packages/manifest.json` вашего проекта:

```json
{
  "dependencies": {
    "com.cgk.webkit": "https://github.com/ВАШ_РЕПО.git?path=Assets/Scripts/WebKit",
    "com.cysharp.unitask": "https://github.com/Cysharp/UniTask.git?path=src/UniTask/Assets/Plugins/UniTask",
    "com.unity.nuget.newtonsoft-json": "3.2.1",
    "com.suvorov.common": "https://github.com/Suvorovm/CGK.git#0.0.60"
  }
}
```

---

### Быстрый старт

#### 1. Инициализация отправителя

```csharp
using WebKit.Controller;
using WebKit.Decorator;

var sender = new UnityWebRequestSender();
sender.Init("https://api.example.com", decorator: null);
```

#### 2. GET-запрос

```csharp
using WebKit.Model;

var request = RequestBuilder.Create()
    .SetPath("/users")
    .SetRequestType(RequestType.Get)
    .SetTimeOut(10)
    .AddParam("page", "1")
    .AddParam("limit", "20")
    .Build();

var response = await sender.MakeRequest<UsersResponse>(request);
```

#### 3. POST-запрос с телом

```csharp
var loginRequest = RequestBuilder.Create()
    .SetPath("/auth/login")
    .SetRequestType(RequestType.Post)
    .SetBody(new { username = "player1", password = "secret" })
    .Build();

var loginResponse = await sender.MakeRequest<LoginResponse>(loginRequest);
```

---

### API RequestBuilder

| Метод | Описание | По умолчанию |
|---|---|---|
| `SetPath(string)` | Путь эндпоинта, добавляется к корневому хосту | — |
| `SetRequestType(RequestType)` | `Get`, `Post`, `Put`, `Patch`, `Delete` | — |
| `SetBody(object)` | Тело запроса, сериализуется в JSON | `null` |
| `AddParam(string, string)` | Добавляет query-параметр | — |
| `SetParams(Dictionary<string,string>)` | Добавляет несколько query-параметров сразу | — |
| `SetTimeOut(int)` | Таймаут в секундах | `30` |
| `SetMaxRetryCount(int)` | Переопределяет максимальное число попыток | `3` |
| `Build()` | Возвращает `RequestModel` | — |

---

### Декораторы

Декораторы оборачивают `UnityWebRequest` перед отправкой. Они компонуются через цепочку конструкторов.

#### AuthDecorator — Bearer-токен

```csharp
using WebKit.Decorator;

var decorator = new AuthDecorator(innerRequestDecorator: null, token: "eyJhbGci...");
sender.Init("https://api.example.com", decorator);
```

#### HttpsRequestDecorator — собственный обработчик сертификата

```csharp
using WebKit.Certificate;
using WebKit.Decorator;

var httpsDecorator = new HttpsRequestDecorator(new CustomCertificateHandler());
sender.Init("https://api.example.com", httpsDecorator);
```

#### Цепочка декораторов (HTTPS + Auth)

```csharp
// Внутренний декоратор применяется первым
var httpsDecorator = new HttpsRequestDecorator(new CustomCertificateHandler());
var authDecorator  = new AuthDecorator(innerRequestDecorator: httpsDecorator, token: userToken);

sender.Init("https://api.example.com", authDecorator);
// К каждому запросу будут применены и сертификат, и заголовок Authorization.
```

#### Переопределение декоратора для одного запроса

```csharp
var tempDecorator = new AuthDecorator(null, token: refreshToken);
var result = await sender.MakeRequest<TokenResponse>(request, decorator: tempDecorator);
```

---

### Политики повтора (Retry Policy)

Реализуйте `IRetryPolicy`, чтобы определить, когда и как повторять неудавшийся запрос.

```csharp
using Cysharp.Threading.Tasks;
using UnityEngine.Networking;
using WebKit.Controller;

public class TokenRefreshRetryPolicy : IRetryPolicy
{
    private readonly ITokenService _tokenService;
    private readonly UnityWebRequestSender _sender;

    public TokenRefreshRetryPolicy(ITokenService tokenService, UnityWebRequestSender sender)
    {
        _tokenService = tokenService;
        _sender = sender;
    }

    // Срабатывает только при 401 Unauthorized
    public bool ShouldHandle(UnityWebRequest failedRequest) =>
        failedRequest.responseCode == 401;

    // Обновляем токен и устанавливаем новый декоратор перед повторной попыткой
    public async UniTask<bool> PrepareRetryAsync()
    {
        string newToken = await _tokenService.RefreshAsync();
        if (string.IsNullOrEmpty(newToken))
            return false;

        _sender.SetDecorator(new AuthDecorator(null, newToken));
        return true;
    }
}
```

Регистрация политики:

```csharp
sender.AddRetryPolicy(new TokenRefreshRetryPolicy(tokenService, sender));
```

---

### Обработка ошибок

Все HTTP-ошибки выбрасывают `WebRequestError`, которое содержит HTTP-статус код.

```csharp
using WebKit.Error;

try
{
    var data = await sender.MakeRequest<UserProfile>(request);
}
catch (WebRequestError e)
{
    Debug.LogError($"HTTP {e.HttpStatusCode}: {e.Message}");

    if (e.HttpStatusCode == 404)
    {
        // обработка "не найдено"
    }
}
```

---

### Полный пример

```csharp
using Cysharp.Threading.Tasks;
using UnityEngine;
using WebKit.Certificate;
using WebKit.Controller;
using WebKit.Decorator;
using WebKit.Error;
using WebKit.Model;

public class ApiService : MonoBehaviour
{
    private UnityWebRequestSender _sender;

    private void Awake()
    {
        _sender = new UnityWebRequestSender();

        var decorator = new HttpsRequestDecorator(new CustomCertificateHandler());
        _sender.Init("https://api.example.com", decorator);
        _sender.AddRetryPolicy(new TokenRefreshRetryPolicy(tokenService, _sender));
    }

    public async UniTask<UserProfile> GetProfileAsync(string userId)
    {
        var request = RequestBuilder.Create()
            .SetPath($"/users/{userId}")
            .SetRequestType(RequestType.Get)
            .SetTimeOut(15)
            .Build();

        try
        {
            return await _sender.MakeRequest<UserProfile>(request);
        }
        catch (WebRequestError e)
        {
            Debug.LogError($"Не удалось загрузить профиль: {e.HttpStatusCode} — {e.Message}");
            throw;
        }
    }

    public async UniTask UpdateScoreAsync(string userId, int score)
    {
        var request = RequestBuilder.Create()
            .SetPath("/scores")
            .SetRequestType(RequestType.Post)
            .SetBody(new { userId, score })
            .SetMaxRetryCount(1)
            .Build();

        await _sender.MakeRequest(request);
    }
}
```

---

### Структура пакета

```
WebKit/
├── Certificate/
│   └── CustomCertificateHandler.cs    # Обработчик HTTPS-сертификата
├── Controller/
│   ├── IRequestSender.cs              # Интерфейс отправителя
│   ├── IRetryPolicy.cs                # Интерфейс политики повтора
│   └── UnityWebRequestSender.cs       # Основная реализация
├── Decorator/
│   ├── RequestDecorator.cs            # Абстрактный базовый класс
│   ├── AuthDecorator.cs               # Добавляет Bearer-токен
│   └── HttpsRequestDecorator.cs       # Добавляет сертификат
├── Model/
│   ├── RequestType.cs                 # Enum: Get, Post, Put, Patch, Delete
│   ├── IRequestModel.cs               # Интерфейс модели
│   ├── RequestModel.cs                # Конкретная модель
│   └── RequestBuilder.cs             # Fluent-builder
├── Error/
│   └── WebRequestError.cs            # Исключение с HTTP-статус кодом
├── WebKit.asmdef
└── package.json
```
