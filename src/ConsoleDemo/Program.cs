using ConsoleDemo.EventBus;

var handler = new MessageHandler();

Console.WriteLine("1、未注册时发布消息：");
handler.Publish();
Console.WriteLine();

Console.WriteLine("2、手动注册后发布消息：");
handler.ManuSubscribe();
handler.Publish();

Console.WriteLine("3、取消手动注册后发布消息：");
handler.ManuUnsubscribe();
handler.Publish();
Console.WriteLine();

Console.WriteLine("4、自动注册后发布消息：");
handler.AutoSubscribe();
handler.Publish();

Console.WriteLine("5、取消自动注册后发布消息：");
handler.AutoUnsubscribe();
handler.Publish();

Console.ReadKey();