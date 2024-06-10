namespace WebAPIDemo.Services
{
    public class TimeService : ITimeService
    {
        public string GetTime()
        {
            return DateTime.Now.ToString("HH:mm:ss fff");
        }
    }
}