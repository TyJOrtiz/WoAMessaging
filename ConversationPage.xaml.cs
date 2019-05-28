using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.ApplicationModel.Calls;
using Windows.ApplicationModel.Chat;
using Windows.Devices.Sms;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.System;
using Windows.UI.Popups;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;

// The Blank Page item template is documented at https://go.microsoft.com/fwlink/?LinkId=234238

namespace Messaging_for_Windows_on_ARM
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class ConversationPage : Page
    {
        private ObservableCollection<Message> Conversation = new ObservableCollection<Message>();
        private MessageThread messagethread;
        private SmsDevice2 device;
        private ChatMessageReader reader;
        private IReadOnlyList<ChatMessage> list;

        public ConversationPage()
        {
            this.InitializeComponent();
            ChatBoxShadow.Receivers.Add(ChatBoxShadowCatcher);
            ConversationList.ItemsSource = Conversation;
        }
        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);
            messagethread = e.Parameter as MessageThread;
            GetConvo(messagethread);

        }

        private async void GetConvo(MessageThread messagethread)
        {
            reader = messagethread.ChatConversation.GetMessageReader();
            list = await reader.ReadBatchAsync();
            foreach (var item in list)
            {
                if (item.IsIncoming)
                {
                    var mesage = new Message
                    {
                        Body = item.Body,
                        Alignment = HorizontalAlignment.Left,
                        Color = (Windows.UI.Color)Application.Current.Resources["SystemAccentColorDark1"],
                        DateTime = item.LocalTimestamp.DateTime.ToString("MM/dd, HH:mm"),
                        Margin = new Thickness(10, 10, 60, 10)
                    };
                    Conversation.Add(mesage);
                }
                else
                {
                    var mesage = new Message
                    {
                        Body = item.Body,
                        Alignment = HorizontalAlignment.Left,
                        Color = (Windows.UI.Color)Application.Current.Resources["SystemAccentColorLight1"],
                        DateTime = item.LocalTimestamp.DateTime.ToString("MM/dd, HH:mm"),
                        Margin = new Thickness(60, 10, 10, 10)
                    };
                    Conversation.Add(mesage);
                }
            }
            var ordered = Conversation.OrderBy(x => x.DateTime);
            ConversationList.ItemsSource = ordered;
            ConversationList.ScrollIntoView(ordered.Last());
            if (messagethread.ChatConversation.Participants.Count > 1)
            {
                var list = new List<MenuFlyoutItem>();
                var flyout = new MenuFlyout();
                for (int i = 0; i > messagethread.ChatConversation.Participants.Count; i++)
                {
                    var flyitem = new MenuFlyoutItem
                    {
                        Text = messagethread.ChatConversation.Participants[i],
                        Tag = messagethread.ChatConversation.Participants[i]
                    };
                    flyitem.Tapped += Flyitem_Tapped;
                    flyout.Items.Add(flyitem);
                }
                CallButton.Flyout = flyout;
            }
            else
            {
                CallButton.Tag = messagethread.ChatConversation.Participants.FirstOrDefault();
                CallButton.Click += CallButton_Click;
            }
        }

        private async void CallButton_Click(object sender, RoutedEventArgs e)
        {
            var number = (sender as AppBarButton).Tag.ToString();
            PlaceCall(number);
        }

        private async void PlaceCall(string number)
        {
            try
            {
                PhoneCallStore PhoneCallStore = await PhoneCallManager.RequestStoreAsync();
                Guid LineGuid = await PhoneCallStore.GetDefaultLineAsync();

                var phoneLine = await PhoneLine.FromIdAsync(LineGuid);
                phoneLine.Dial(number, "New Call");
            }
            catch
            {

                var uri = $"callto:{number}";
                var launch = await Launcher.LaunchUriAsync(new Uri(uri));
            }
        }

        private async void Flyitem_Tapped(object sender, TappedRoutedEventArgs e)
        {
            var number = (sender as MenuFlyoutItem).Tag.ToString();
            PlaceCall(number);
        }

        private async void SendButton_Click(object sender, RoutedEventArgs e)
        {
            ChatMessageStore store = await ChatMessageManager.RequestStoreAsync();
            var mess = new ChatMessage
            {
                Body = MessageTB.Text,
                TransportId = "0"
            };
            foreach (var rec in messagethread.ChatConversation.Participants)
            {
                mess.Recipients.Add(rec);
            }
            if (MessageTB.Text.Length > 0)
            {
                try
                {
                    foreach (var recipient in messagethread.ChatConversation.Participants)
                    {
                        sendtext(MessageTB.Text, recipient);
                    }
                    try
                    {
                        
                        await store.SendMessageAsync(mess);
                    }
                    catch (Exception ex)
                    {
                        var nsg = new MessageDialog(ex.Message);
                        await nsg.ShowAsync();
                        //try
                        //{
                        //    await Windows.ApplicationModel.Chat.ChatMessageManager.ShowComposeSmsMessageAsync(mess);
                        //}
                        //catch (Exception ex2)
                        //{
                        //    var nsg2 = new MessageDialog(ex2.Message);
                        //    await nsg2.ShowAsync();
                        //}
                    }
                    var mesage = new Message
                    {
                        Body = MessageTB.Text,
                        Alignment = HorizontalAlignment.Right,
                        Color = (Windows.UI.Color)Application.Current.Resources["SystemAccentColorLight1"],
                        DateTime = DateTime.Now.ToString("MM/dd, HH:mm"),
                        Margin = new Thickness(60, 10, 10, 10)
                    };
                    Conversation.Add(mesage);
                }
                catch
                {

                }
            }
        }

        private async void sendtext(string text, string recipient)
        {
            if (device == null)

            {

                try

                {

                    Debug.WriteLine("Getting default SMS device ...");

                    device = SmsDevice2.GetDefault();

                }

                catch (Exception ex)

                {

                    Debug.WriteLine(ex.Message);

                    return;

                }



            }



            string msgStr = "";

            if (device != null)

            {

                try

                {

                    // Create a text message - set the entered destination number and message text.

                    SmsTextMessage2 msg = new SmsTextMessage2();

                    msg.To = recipient;

                    msg.Body = MessageTB.Text;

                    // Send the message asynchronously

                    Debug.WriteLine("Sending message ...");

                    SmsSendMessageResult result = await device.SendMessageAndGetResultAsync(msg);



                    if (result.IsSuccessful)

                    {

                        msgStr = "";

                        msgStr += "Text message sent, cellularClass: " + result.CellularClass.ToString();

                        IReadOnlyList<Int32> messageReferenceNumbers = result.MessageReferenceNumbers;



                        for (int i = 0; i < messageReferenceNumbers.Count; i++)

                        {

                            msgStr += "\n\t\tMessageReferenceNumber[" + i.ToString() + "]: " + messageReferenceNumbers[i].ToString();

                        }

                        Debug.WriteLine(msgStr);

                    }

                    else

                    {

                        msgStr = "";

                        msgStr += "ModemErrorCode: " + result.ModemErrorCode.ToString();

                        msgStr += "\nIsErrorTransient: " + result.IsErrorTransient.ToString();

                        if (result.ModemErrorCode == SmsModemErrorCode.MessagingNetworkError)

                        {

                            msgStr += "\n\tNetworkCauseCode: " + result.NetworkCauseCode.ToString();



                            if (result.CellularClass == CellularClass.Cdma)

                            {

                                msgStr += "\n\tTransportFailureCause: " + result.TransportFailureCause.ToString();

                            }

                            Debug.WriteLine(msgStr);

                        }

                    }

                }

                catch (Exception ex)

                {

                    Debug.WriteLine(ex.Message);

                }

            }

            else

            {

                Debug.WriteLine("Failed to find SMS device");

            }

        }

        private void AddMessage(string text)
        {
            var mesage = new Message
            {
                Body = text,
                Alignment = HorizontalAlignment.Right,
                Color = (Windows.UI.Color)Application.Current.Resources["SystemAccentColorLight1"],
                DateTime = DateTime.Now.ToString("MM/dd, HH:mm"),
                Margin = new Thickness(60, 10, 10, 10)
            };
            Conversation.Add(mesage);
            ConversationList.ScrollIntoView(mesage);
            MessageTB.Text = "";
        }

        private async void Randon_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                var uri = new Uri("https://baconipsum.com/api/?type=meat-and-filler&paras=5&format=text");
                var client = new HttpClient();
                var response = await client.GetAsync(uri);
                var respoonsestring = await response.Content.ReadAsStringAsync();
                var mesage = new Message
                {
                    Body = respoonsestring,
                    Alignment = HorizontalAlignment.Left,
                    Color = (Windows.UI.Color)Application.Current.Resources["SystemAccentColorDark1"],
                    DateTime = DateTime.Now.ToString("MM/dd, HH:mm"),
                    Margin = new Thickness(10, 10, 60, 10)
                };
                Conversation.Add(mesage);
                ConversationList.ScrollIntoView(mesage);
            }
            catch
            {

            }
        }

        private void SendButton_RightTapped(object sender, RightTappedRoutedEventArgs e)
        {
            var mesage = new Message
            {
                Body = MessageTB.Text,
                Alignment = HorizontalAlignment.Left,
                Color = (Windows.UI.Color)Application.Current.Resources["SystemAccentColorDark1"],
                DateTime = DateTime.Now.ToString("MM/dd, HH:mm"),
                Margin = new Thickness(10, 10, 60, 10)
            };
            Conversation.Add(mesage);
            ConversationList.ScrollIntoView(mesage);
            MessageTB.Text = "";
        }

        private void DeviceStates_CurrentStateChanged(object sender, VisualStateChangedEventArgs e)
        {
            if (e.NewState == Medium)
            {
                Frame.GoBack();
            }
        }
    }
}
