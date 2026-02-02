#if using_firebase_message
using Firebase.Extensions;
#endif
using Sonat.Debugger;

namespace Sonat.FirebaseModule.Message
{
    public class SonatFirebaseMessage
    {
        private PlayerPrefInt disableNoti = new PlayerPrefInt("DisableNotify");
        public const string DefaultTopic = "game";

        public bool IsNotificationDisable() => disableNoti.BoolValue;


        public void Initialize()
        {
            if (!disableNoti.BoolValue)
            {
#if using_firebase_message
                Firebase.Messaging.FirebaseMessaging.TokenReceived += OnTokenReceived;
                Firebase.Messaging.FirebaseMessaging.MessageReceived += OnMessageReceived;
#endif
            }
        }

        public void EnableNotifications()
        {
            disableNoti.BoolValue = false;
#if using_firebase || using_firebase_message
            //init notifications
            Firebase.Messaging.FirebaseMessaging.DeleteTokenAsync().ContinueWithOnMainThread(task =>
            {
                SonatDebugType.Firebase.Log($"SonatFirebase - Deleted Token");
                Firebase.Messaging.FirebaseMessaging.GetTokenAsync().ContinueWithOnMainThread(task =>
                {
                    SonatDebugType.Firebase.Log($"SonatFirebase - Got New Token");
                    Firebase.Messaging.FirebaseMessaging.SubscribeAsync(DefaultTopic).ContinueWithOnMainThread(task =>
                    {
                        SonatDebugType.Firebase.Log($"SonatFirebase - Subscribed To Topic - {DefaultTopic}");
                    });
                });
            });
#endif
        }

        public void DisableNotification()
        {
            disableNoti.BoolValue = true;
#if using_firebase || using_firebase_message
            //init notifications
            Firebase.Messaging.FirebaseMessaging.DeleteTokenAsync().ContinueWithOnMainThread(task =>
            {
                SonatDebugType.Firebase.Log($"SonatFirebase - Deleted Token");
                Firebase.Messaging.FirebaseMessaging.GetTokenAsync().ContinueWithOnMainThread(task =>
                {
                    SonatDebugType.Firebase.Log($"SonatFirebase - Got New Token");
                    Firebase.Messaging.FirebaseMessaging.UnsubscribeAsync(DefaultTopic).ContinueWithOnMainThread(task =>
                    {
                        SonatDebugType.Firebase.Log($"SonatFirebase - UnSubscribed To Topic - {DefaultTopic}");
                    });
                });
            });
#endif
        }


#if using_firebase || using_firebase_message
        public void OnTokenReceived(object sender, Firebase.Messaging.TokenReceivedEventArgs token)
        {
            SonatDebugType.Firebase.Log("Received Registration Token: " + token.Token);
            if (disableNoti.BoolValue)
                DisableNotification();
        }

        public void OnMessageReceived(object sender, Firebase.Messaging.MessageReceivedEventArgs e)
        {
            SonatDebugType.Firebase.Log("Received a new message from: " + e.Message.From);
        }
#endif
    }
}