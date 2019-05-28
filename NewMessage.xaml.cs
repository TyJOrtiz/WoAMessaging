using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.ApplicationModel.Contacts;
using Windows.Devices.Sms;
using Windows.Foundation;
using Windows.Foundation.Collections;
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
    public sealed partial class NewMessage : Page
    {
        public IList<Contact> contacts;
        private SmsDevice2 device;

        public NewMessage()
        {
            this.InitializeComponent();
            trythis();
        }

        private void trythis()
        {
           var stringderp = "15103463456; 15103463456; 15103463456; 15103463456; 15103463456";
            var wors = stringderp.Split("; ");
            foreach (var wor in wors)
            {
                Debug.WriteLine(wor);
            }
        }

        private void DeviceStates_CurrentStateChanged(object sender, VisualStateChangedEventArgs e)
        {
            if (e.NewState == Medium)
            {
                Frame.GoBack();
            }
        }

        private async void SendButton_Click(object sender, RoutedEventArgs e)
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
                var wors = recipientbox.Text.Split("; ");
                foreach (var wor in wors)
                {
                    try

                    {

                        // Create a text message - set the entered destination number and message text.

                        SmsTextMessage2 msg = new SmsTextMessage2();

                        msg.To = wor;

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

            }

            else

            {

                Debug.WriteLine("Failed to find SMS device");

            }
        }

        private async void Button_Click(object sender, RoutedEventArgs e)
        {
            ContactPicker contactPicker = new ContactPicker();

            contactPicker.DesiredFieldsWithContactFieldType.Add(ContactFieldType.PhoneNumber);
            var contacts = await contactPicker.PickContactsAsync();
            if (contacts != null && contacts.Count > 0)
            {
                foreach (Contact contact in contacts)
                {
                    recipientbox.Text += contact.Phones[0].Number + "; ";
                }
                var wors = recipientbox.Text.Split("; ");
                foreach (var wor in wors)
                {
                    Debug.WriteLine(wor);
                }
            }
        }
    }
}
