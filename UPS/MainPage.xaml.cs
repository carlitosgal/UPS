using Microsoft.Maker.RemoteWiring;
using Microsoft.Maker.Serial;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;

// La plantilla de elemento Página en blanco está documentada en http://go.microsoft.com/fwlink/?LinkId=402352&clcid=0x409

namespace UPS
{
    /// <summary>
    /// Página vacía que se puede usar de forma independiente o a la que se puede navegar dentro de un objeto Frame.
    /// </summary>
    public sealed partial class MainPage : Page
    {

        RemoteDevice arduino;
        UsbSerial connection;
        String Source ="Panel";
        String ChangeMode="Auto";
        Stopwatch cron = new Stopwatch();
        Stopwatch second = new Stopwatch();
        TimeSpan diez = new TimeSpan (10,0,0);
        TimeSpan stap = new TimeSpan(0, 0, 1);
        UInt16 flag1 = 0;
        UInt16 flag2 = 0;
        UInt16 vsys =14;
        UInt16 vcom = 14;


        public MainPage()
        {
            this.InitializeComponent();
            connection = new UsbSerial("VID_2341", "PID_0043");
            arduino = new RemoteDevice(connection);

            arduino.DeviceReady += OnDeviceReady; 

            connection.begin(57600, SerialConfig.SERIAL_8N1);
            
        }

        private void OnDeviceReady()
        {
            var accion = Dispatcher.RunAsync(Windows.UI.Core.CoreDispatcherPriority.Normal, new Windows.UI.Core.DispatchedHandler(() =>
          {
              Auto.IsEnabled = true;
              Manual.IsEnabled = true;
              Panel.IsEnabled = true;
              Commercial.IsEnabled = true;
              relayON.IsEnabled = true;
              relayOFF.IsEnabled = true;
          }));

            //set digital pin to OUTPUT
            arduino.pinMode(13, PinMode.OUTPUT);
            arduino.pinMode(6, PinMode.OUTPUT);

            //set analog pin to ANALOG INPUT
            arduino.pinMode("A0", PinMode.ANALOG);
            arduino.pinMode("A1", PinMode.ANALOG);

            arduino.AnalogPinUpdated += MyAnalogPinUpdatedCallBack;
        }

        private void MyAnalogPinUpdatedCallBack(string pin, ushort value)
        {
            vsys = arduino.analogRead("A0");
            vsys *= 50;
            vsys /= 1023;

            vcom = arduino.analogRead("A1");
            vcom *= 50;
            vcom /= 1023;

            if (flag2 == 0)
            {
                refresh();
            }
        }

        private void refresh()
        {
            flag2 = 1;
            second.Start();
            flag1 = 0;
            while (flag1==0)
            {
                if (second.Elapsed>=stap)
                {
                    //loop here

                    if (textBlock6.Dispatcher.HasThreadAccess)
                    {
                        textBlock6.Text = "Change of source";
                    }
                    else {
                        textBlock6.Dispatcher.RunAsync(Windows.UI.Core.CoreDispatcherPriority.Normal, () => { textBlock6.Text = "Change of source"; });
                    }

                    if (textBlock7.Dispatcher.HasThreadAccess)
                    {
                        textBlock7.Text = "Energy source";
                    }
                    else {
                        textBlock7.Dispatcher.RunAsync(Windows.UI.Core.CoreDispatcherPriority.Normal, () => { textBlock7.Text = "Energy source"; });
                    }

                    if (textBlock8.Dispatcher.HasThreadAccess)
                    {
                        textBlock8.Text = "Relay (OFF)";
                    }
                    else {
                        textBlock8.Dispatcher.RunAsync(Windows.UI.Core.CoreDispatcherPriority.Normal, () => { textBlock8.Text = "Relay (OFF)"; });
                    }

                    if (textBlock2.Dispatcher.HasThreadAccess)
                    {
                        textBlock2.Text = "Change of source is: " + ChangeMode;
                    }
                    else {
                        textBlock2.Dispatcher.RunAsync(Windows.UI.Core.CoreDispatcherPriority.Normal, () => { textBlock2.Text = "Change of source is: " + ChangeMode; });
                    }

                    if (textBlock1.Dispatcher.HasThreadAccess)
                    {
                        textBlock1.Text = "Actual source: " + Source;
                    }
                    else {
                        textBlock1.Dispatcher.RunAsync(Windows.UI.Core.CoreDispatcherPriority.Normal, () => { textBlock1.Text = "Actual source: " + Source; });
                    }

                    if (textBlock5.Dispatcher.HasThreadAccess)
                    {
                        textBlock5.Text = "External charging time: " + cron.ToString();
                    }
                    else {
                        textBlock5.Dispatcher.RunAsync(Windows.UI.Core.CoreDispatcherPriority.Normal, () => { textBlock5.Text = "External charging time: " + cron.ToString(); });
                    }

                    if (textBlock3.Dispatcher.HasThreadAccess)
                    {
                        textBlock3.Text = "Voltage in of the system: " + vsys;
                    }
                    else {
                        textBlock3.Dispatcher.RunAsync(Windows.UI.Core.CoreDispatcherPriority.Normal, () => { textBlock3.Text = "Voltage in of the system: " + vsys; });
                    }

                    if (textBlock4.Dispatcher.HasThreadAccess)
                    {
                        textBlock4.Text = "Voltage of the commercial current: " + vcom;
                    }
                    else {
                        textBlock4.Dispatcher.RunAsync(Windows.UI.Core.CoreDispatcherPriority.Normal, () => { textBlock4.Text = "Voltage of the commercial current: " + vcom; });
                    }
                    


                    if (vsys <= 12 && ChangeMode == "Auto")
                    {
                        arduino.digitalWrite(13, PinState.HIGH);
                        Source = "Commercial current";
                        cron.Start();
                    }
                    if (Source == "Commercial current" && cron.Elapsed >= diez)
                    {
                        arduino.digitalWrite(13, PinState.LOW);
                        Source = "Panel";
                        cron.Stop();
                        cron.Reset();
                    }
                    flag1 = 1;
                    second.Stop();
                    second.Reset();
                    flag2 = 0;
                }
            }
        }

        private void Auto_Click (object sender, RoutedEventArgs e)
        {
            ChangeMode = "Auto";
        }

        private void Manual_Click(object sender, RoutedEventArgs e)
        {
            ChangeMode = "Manual";
        }

        private void Panel_Click(object sender, RoutedEventArgs e)
        {
            arduino.digitalWrite(13, PinState.LOW);
            Source = "Panel";
            cron.Stop();
            cron.Reset();
        }
        private void Commercial_Click(object sender, RoutedEventArgs e)
        {
            arduino.digitalWrite(13, PinState.HIGH);
            Source = "Commercial current";
            cron.Start();
        }
        private void relayON_Click(object sender, RoutedEventArgs e)
        {
            arduino.digitalWrite(6, PinState.HIGH);

            if (textBlock8.Dispatcher.HasThreadAccess)
            {
                textBlock8.Text = "Relay (ON)";
            }
            else {
                textBlock8.Dispatcher.RunAsync(Windows.UI.Core.CoreDispatcherPriority.Normal, () => { textBlock8.Text = "Relay (ON)"; });
            }
        }
        private void relayOFF_Click(object sender, RoutedEventArgs e)
        {
            arduino.digitalWrite(6, PinState.LOW);

            if (textBlock8.Dispatcher.HasThreadAccess)
            {
                textBlock8.Text = "Relay (OFF)";
            }
            else {
                textBlock8.Dispatcher.RunAsync(Windows.UI.Core.CoreDispatcherPriority.Normal, () => { textBlock8.Text = "Relay (OFF)"; });
            }
        }
    }
}
