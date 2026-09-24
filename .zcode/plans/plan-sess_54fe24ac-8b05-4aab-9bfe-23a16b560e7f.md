# Context 解决方案 — WPF + .NET 10 Clean Architecture 骨架（slnx 格式）

在 `C:\Users\OR\source\Context`（当前空目录）下创建解决方案 `Context.slnx`（新版 XML 解决方案格式），四层单向依赖 + 单元测试，SQLite + Dapper，CommunityToolkit.Mvvm，仅做最小垂直切片骨架（一个 Product 实体贯穿各层，不做完整 CRUD 页面）。

## 0. 前置检查
- 运行 `dotnet --list-sdks` 验证已装 .NET 10 SDK（slnx 需要 SDK 9.0.200+，.NET 10 原生支持）。**若未安装则停止并向你报告**，不擅自装 SDK。

## 1. 项目结构与依赖方向（单向，无环）

```
Context.slnx                        # dotnet new sln -n Context --format slnx
├── Directory.Packages.props        # NuGet 包版本集中管理 (CPM)
├── .gitignore                      # 标准 VS .gitignore
├── src/
│   ├── Context.Domain/             # net10.0 类库 —— 实体，零依赖
│   │   └── Entities/Product.cs                     (Id, Name)
│   ├── Context.Application/        # → Domain —— 用例层，只定义接口
│   │   ├── Abstractions/IProductRepository.cs      (GetAllAsync / AddAsync)
│   │   ├── Services/ProductService.cs              (构造注入接口，做名字 Trim/非空校验——给单测留可验证逻辑)
│   │   └── DependencyInjection.cs                  (AddApplication 扩展)
│   ├── Context.Infrastructure/     # → Application —— Dapper + SQLite 实现
│   │   ├── Persistence/IDbConnectionFactory.cs     (基础设施内部接口)
│   │   ├── Persistence/SqliteConnectionFactory.cs
│   │   ├── Persistence/DatabaseInitializer.cs      (CREATE TABLE IF NOT EXISTS Products; 自动建 app.db)
│   │   ├── Persistence/ProductRepository.cs        (Dapper QueryAsync/ExecuteAsync)
│   │   └── DependencyInjection.cs                  (AddInfrastructure 扩展)
│   ├── Context.Presentation/       # → Application —— ViewModel 层
│   │   ├── ViewModels/MainViewModel.cs             ([ObservableProperty] 新品名, [RelayCommand] Load/Add)
│   │   └── DependencyInjection.cs                  (AddPresentation 扩展)
│   └── Context.App/                # WPF (net10.0-windows)，组合根 → Presentation + Infrastructure
│       ├── App.xaml / App.xaml.cs                  (OnStartup 组装 ServiceCollection，OnExit 释放)
│       └── MainWindow.xaml / .cs                   (ctor 注入 MainViewModel；ListBox + TextBox + 两个按钮)
└── tests/
    └── Context.Tests/              # xUnit → Application
        ├── ProductServiceTests.cs  (NSubstitute 模拟 IProductRepository)
        └── MainViewModelTests.cs   (模拟 IProductService，验证命令行为)
```

依赖链：`Domain ← Application ← Infrastructure`；`Application ← Presentation`；WPF App 是组合根（唯一允许知道所有层的项目）。

## 2. NuGet 包（实施时用最新稳定版，写入 Directory.Packages.props）
| 位置 | 包 |
|---|---|
| Context.App | Microsoft.Extensions.DependencyInjection |
| Context.Application | Microsoft.Extensions.DependencyInjection.Abstractions（用于 AddApplication 扩展） |
| Context.Infrastructure | Dapper、Microsoft.Data.Sqlite、DI.Abstractions |
| Context.Presentation | CommunityToolkit.Mvvm、DI.Abstractions |
| Context.Tests | NSubstitute（xUnit 用模板自带） |

## 3. 实施步骤
1. `dotnet new sln -n Context --format slnx`；6 个项目：4×`classlib -f net10.0`、1×`wpf`、1×`xunit`，全部 `dotnet sln Context.slnx add ...`
2. 按上文设置 `dotnet add ... reference` 项目引用；删除模板生成的 Class1.cs
3. `dotnet add package` 安装各包（SDK 8+ 自动同步 CPM）
4. 写入上述源码：DI 扩展方法、Dapper 仓储（GetAllAsync/AddAsync 两个方法）、数据库初始化器、MainViewModel（Load + Add 命令）、MainWindow 绑定、两个测试类
5. SQLite 连接串默认 `Data Source=app.db`（AddInfrastructure 可选参数），首次启动自动建表

## 4. 验证
- `dotnet build Context.slnx` 零警告零错误
- `dotnet test` 全部通过
- 说明：WPF 窗口不在自动化环境启动，构建+测试通过后由你本地 `dotnet run --project src/Context.App` 验证界面
