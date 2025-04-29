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
    }
}
