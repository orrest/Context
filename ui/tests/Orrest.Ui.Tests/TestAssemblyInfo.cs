using Xunit;

// WPF pack:// URI 的方案注册非线程安全：xunit 默认并行运行测试类，
// 首次解析 pack:// 的并发竞态会随机产生 "Invalid URI: Invalid port specified" /
// "The URI prefix is not recognized"，禁用并行后注册只发生一次。
[assembly: CollectionBehavior(DisableTestParallelization = true)]
