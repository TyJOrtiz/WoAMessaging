using Windows.UI;
using Windows.UI.Xaml;

namespace Messaging_for_Windows_on_ARM
{
    public class Message
    {
        public string Body { get; set; }
        public HorizontalAlignment Alignment { get; set; }
        public Color Color { get; set; }
        public string DateTime { get; set; }
        public Thickness Margin { get; set; }
    }
}