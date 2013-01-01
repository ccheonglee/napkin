﻿
//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated by the Gadgeteer Designer.
//
//     Changes to this file may cause incorrect behavior and will be lost if
//     the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

using Gadgeteer;
using GTM = Gadgeteer.Modules;

namespace hydra1
{
    public partial class Program : Gadgeteer.Program
    {
        // GTM.Module definitions
        Gadgeteer.Modules.GHIElectronics.UsbClientDP usbClientDP;
        Gadgeteer.Modules.Seeed.OledDisplay oledDisplay;
        Gadgeteer.Modules.GHIElectronics.SDCard sdCard;
        Gadgeteer.Modules.GHIElectronics.Display_HD44780 display_HD44780;
        Gadgeteer.Modules.GHIElectronics.Button button;
        Gadgeteer.Modules.GHIElectronics.Joystick joystick;
        Gadgeteer.Modules.GHIElectronics.RFID rfid;

        public static void Main()
        {
            //Important to initialize the Mainboard first
            Mainboard = new GHIElectronics.Gadgeteer.FEZHydra();			

            Program program = new Program();
            program.InitializeModules();
            program.ProgramStarted();
            program.Run(); // Starts Dispatcher
        }

        private void InitializeModules()
        {   
            // Initialize GTM.Modules and event handlers here.		
            usbClientDP = new GTM.GHIElectronics.UsbClientDP(2);
		
            oledDisplay = new GTM.Seeed.OledDisplay(4);
		
            rfid = new GTM.GHIElectronics.RFID(7);
		
            sdCard = new GTM.GHIElectronics.SDCard(8);
		
            display_HD44780 = new GTM.GHIElectronics.Display_HD44780(9);
		
            button = new GTM.GHIElectronics.Button(12);
		
            joystick = new GTM.GHIElectronics.Joystick(14);

        }
    }
}
