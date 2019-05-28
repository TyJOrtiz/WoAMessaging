using Windows.ApplicationModel.Chat;
using Windows.UI.Xaml.Media.Imaging;

namespace Messaging_for_Windows_on_ARM
{
    public class MessageThread
    {
        public ChatConversation ChatConversation { get; set; }
        public string Participant { get; set; }
        public BitmapImage ParticipantPhoto { get; set; }
        public string RecentMessage { get; set; }
        public string DateTime { get; set; }
    }
}