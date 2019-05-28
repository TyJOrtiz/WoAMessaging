using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Threading.Tasks;
using Windows.ApplicationModel.Chat;
using Windows.ApplicationModel.Contacts;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Popups;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Media.Imaging;
using Windows.UI.Xaml.Navigation;

// The Blank Page item template is documented at https://go.microsoft.com/fwlink/?LinkId=402352&clcid=0x409

namespace Messaging_for_Windows_on_ARM
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class MainPage : Page
    {
        public MainPage()
        {
            this.InitializeComponent();
            GetMessages();
        }

        private ObservableCollection<MessageThread> Messagelist = new ObservableCollection<MessageThread>();

        private async void GetMessages()
        {
            try
            {
                var contactStore = await ContactManager.RequestStoreAsync();
                var contacts = await contactStore.FindContactsAsync();
                ChatMessageStore store = await ChatMessageManager.RequestStoreAsync();
                var derp = store.GetConversationReader();
                var batch = await derp.ReadBatchAsync();
                foreach (var item in batch)
                {
                    try
                    {
                        Messagelist.Add(new MessageThread
                        {
                            ChatConversation = item,
                            Participant = GetParticipants(item.Participants),
                            ParticipantPhoto = await GetPhoto(contactStore, item),
                            RecentMessage = await Getrecentmessage(item.MostRecentMessageId, item.GetMessageReader()),
                            DateTime = await GetrecentmessageTime(item.MostRecentMessageId, item.GetMessageReader()),
                        });
                    }
                    catch (Exception ex)
                    {
                        var msgderp = new MessageDialog(ex.Message);
                        await msgderp.ShowAsync();

                    }
                }
                ConvoList.ItemsSource = Messagelist;
            }

            catch (Exception ex)
            {
                Debug.WriteLine(ex.Message);
            }
        }

        private async Task<BitmapImage> GetPhoto(ContactStore contactStore, ChatConversation item)
        {
            try
            {
                var contact = await contactStore.GetContactAsync(item.ThreadingInfo.ContactId);
                var photo = await contact.LargeDisplayPicture.OpenReadAsync();
                var img = new BitmapImage();
                await img.SetSourceAsync(photo);
                return img;
            }
            catch
            {
                return null;
                //var img = new BitmapImage(new Uri("ms-appx:///Assets/StoreLogo.png"));
                //return img;
            }
        }

        private async Task<string> GetrecentmessageTime(string mostRecentMessageId, ChatMessageReader chatMessageReader)
        {
            var Recentmessage = new ChatMessage();
            var batch = await chatMessageReader.ReadBatchAsync();
            var item = batch.Where(x => x.Id == mostRecentMessageId);
            Recentmessage = item.FirstOrDefault();
            var time = Recentmessage.LocalTimestamp.LocalDateTime.GetDateTimeFormats()[0];
            return time;
        }

        private async Task<string> Getrecentmessage(string mostRecentMessageId, ChatMessageReader chatMessageReader)
        {
            var Recentmessage = new ChatMessage();
            var batch = await chatMessageReader.ReadBatchAsync();
            var item = batch.Where(x => x.Id == mostRecentMessageId);
            Recentmessage = item.FirstOrDefault();
            var body = Recentmessage.Body;
            return body;            
        }

        private async Task<BitmapImage> GetPhoto(Contact contact)
        {
            try
            {
                var photo = await contact.LargeDisplayPicture.OpenReadAsync();
                var img = new BitmapImage();
                await img.SetSourceAsync(photo);
                return img;
            }
            catch
            {
                var img = new BitmapImage(new Uri("ms-appx:///Assets/StoreLogo.png"));
                return img;
            }
        }
        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);
            if (ConvoList.SelectedItem != null)
            {
                var item = ConvoList.SelectedItem as MessageThread;
                ContentFrame.Navigate(typeof(ConversationPage), item);
            }
        }
        private string GetParticipants(IList<string> participants)
        {
            var participantlabel = "";
            if (participants.Count == 1)
            {
                return participants.FirstOrDefault();
            }
            else
            {
                for (int i = 0; i < participants.Count() - 1; i++)
                {
                    participantlabel += $"{participants[i]}; ";
                }
                participantlabel += participants.Last();
                return participantlabel;
            }
        }

        private void ConvoList_ItemClick(object sender, ItemClickEventArgs e)
        {
            var item = e.ClickedItem as MessageThread;
            if (DeviceStates.CurrentState == Normal)
            {
                Frame.Navigate(typeof(ConversationPage), item);
            }
            if (DeviceStates.CurrentState == Medium)
            {
                ContentFrame.Navigate(typeof(ConversationPage), item);
            }
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            if (DeviceStates.CurrentState == Normal)
            {
                //Frame.Navigate(typeof(ConversationPage));
            }
            if (DeviceStates.CurrentState == Medium)
            {
                //ContentFrame.Navigate(typeof(ConversationPage));
            }
        }

        private void DeviceStates_CurrentStateChanged(object sender, VisualStateChangedEventArgs e)
        {
            Debug.WriteLine(e.NewState.Name);
            if (e.NewState == Medium)
            {
                if (ConvoList.SelectedItem != null)
                {
                    var item = ConvoList.SelectedItem as MessageThread;
                    ContentFrame.Navigate(typeof(ConversationPage), item);
                }
            }
            else
            {
                if (ConvoList.SelectedItem != null)
                {
                    var item = ConvoList.SelectedItem as MessageThread;
                    Frame.Navigate(typeof(ConversationPage), item);
                }
            }
        }

        private void Button_Click_1(object sender, RoutedEventArgs e)
        {
            Frame.Navigate(typeof(NewMessage));
        }
    }
}
