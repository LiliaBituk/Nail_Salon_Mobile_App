using Android.App;
using Android.Content.PM;
using Android.Runtime;
using Android.OS;
using Microsoft.Maui;

namespace Nail_Salon_Mobile_App
{
    [Activity(Label = "Nail_Salon_Mobile_App", Icon = "@mipmap/favicon", Theme = "@style/MainTheme", MainLauncher = true, ConfigurationChanges = ConfigChanges.ScreenSize | ConfigChanges.Orientation)]
    public class MainActivity : MauiAppCompatActivity
    {
        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);
        }
    }
}