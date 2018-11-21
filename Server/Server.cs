using CitizenFX.Core;
using System;
using System.Runtime.InteropServices;

namespace Server
{
    public class Server : BaseScript
    {
        private int _baitCar = 0;
        private Player _operator;
        private bool _baitcarArmed = false;

        public Server()
        {
            Debug.WriteLine("Baitcar loaded");

            EventHandlers["chatMessage"] += new Action<int, string, string>(HandleChatMessage);
            EventHandlers["Baitcar.Installed"] += new Action<int, int>(baitcarInstalled);
            EventHandlers["Baitcar.EnteredCar"] += new Action<int, int>(checkEnteredCar);
            EventHandlers["Baitcar.ExitedBaitcar"] += new Action<int>(exitedBaitcar);
            EventHandlers["Baitcar.ID"] += new Action<int>(checkIn);
            EventHandlers["Baitcar.Blip"] += new Action<int, float, float, float>(updateBlip);
        }

        private void HandleChatMessage(int src, string color, string message)
        {
            string[] args = message.Split(' ');
            var player = new PlayerList()[src];
            if (args[0].ToLower() == "/baitcar")
            {
                switch (args[1].ToLower())
                {
                    case "install":
                        player.TriggerEvent("Baitcar.Install");
                        break;
                    case "arm":
                        _baitcarArmed = true;
                        player.TriggerEvent("Baitcar.VehicleActivity", "armed");
                        break;
                    case "lock":
                        break;
                    case "unlock":
                        TriggerClientEvent(null, "Baitcar.Unlocked");
                        break;
                    case "reset":
                        player.TriggerEvent("Baitcar.Reset");
                        break;
                    case "remove":
                        break;
                    default:
                        player.TriggerEvent("chat:addMessage", "[Baitcar]", new[] { 255, 0, 0 }, " Unknown option. Valid options are: install, arm, lock, unlock, reset, remove");
                        break;
                }
            }
        }

        private void baitcarInstalled(int src, int vehicle)
        {
            _operator = new PlayerList()[src];
            _baitCar = vehicle;
            TriggerClientEvent("Baitcar.ID", _baitCar);
        }

        private void checkEnteredCar(int src, int vehicle)
        {
            Player player = new PlayerList()[src];
            if (vehicle == _baitCar)
            {
                _operator.TriggerEvent("Baitcar.VehicleActivity", "entry");
                player.TriggerEvent("Baitcar.YouAreInBaitcar");
            }
        }

        private void exitedBaitcar(int src)
        {
            _operator.TriggerEvent("Baitcar.VehicleActivity", "entry");
        }

        private void checkIn(int src)
        {
            Player p = new PlayerList()[src];
            p.TriggerEvent("Baitcar.ID", _baitCar);
        }

        private void updateBlip(int src, float x, float y, float z)
        {
            TriggerClientEvent(null, "Baitcar.UpdateBlip", x, y, z);
        }
    }
}
