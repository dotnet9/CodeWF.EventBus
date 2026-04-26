using CodeWF.EventBus;
using System;
using System.Windows.Forms;

namespace WindowsFormsApp1_4_8
{
    public class TimeCommand : Command
    {
        public int Id { get; set; }
    }
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();

            // 无 IOC 场景下，窗体自己订阅自己的事件处理方法。
            EventBus.Default.Subscribe(this);

            var btn = new Button();
            btn.Text = "publish";
            btn.Click += (s, e) => EventBus.Default.Publish(new TimeCommand() { Id = DateTime.Now.Millisecond });
            this.Controls.Add(btn);
        }

        [EventHandler]
        private void ReceiveCommand(TimeCommand command)
        {
            MessageBox.Show($"收到ID：{command.Id}");
        }

        protected override void OnFormClosed(FormClosedEventArgs e)
        {
            // 窗体关闭时主动取消订阅，避免重复打开窗体后处理器累积。
            EventBus.Default.Unsubscribe(this);
            base.OnFormClosed(e);
        }
    }
}
