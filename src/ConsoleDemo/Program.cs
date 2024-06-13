using ConsoleDemo.EventBus;

var handler = new MessageHandler();

Console.WriteLine("Post messages when not registered");
handler.Publish();
Console.WriteLine();

Console.WriteLine("---------------------------------------------------------");

Console.WriteLine("Post messages after manual registration");
handler.ManuSubscribe();
handler.Publish();

Console.WriteLine("---------------------------------------------------------");

Console.WriteLine("Post messages after canceling manual registration");
handler.ManuUnsubscribe();
handler.Publish();
Console.WriteLine();

Console.WriteLine("---------------------------------------------------------");

Console.WriteLine("Post messages after automatic registration");
handler.AutoSubscribe();
handler.Publish();

Console.WriteLine("---------------------------------------------------------");

Console.WriteLine("Post a message after canceling automatic registration");
handler.AutoUnsubscribe();
handler.Publish();

Console.ReadKey();