using CitizenFX.Core;
using CitizenFX.Core.Native;
using static CitizenFX.Core.Native.API;
using System;
using System.Linq;
using System.Threading.Tasks;
using CitizenFX.Core.UI;
using Newtonsoft.Json;

namespace Baitcar
{
    public class Client : BaseScript
    {
        private bool hasControlJob = false;
        private Config config;
        private int _baitCar = 0;
        private bool wasInCar = false;
        private bool IAmInBaitcar = false;
        private bool CarLockeddown = false;
        private Blip bcBlip;

        public Client()
        {
            String data = Function.Call<string>(Hash.LOAD_RESOURCE_FILE, "baitcar", "config.json");
            config = JsonConvert.DeserializeObject<Config>(data);

            Tick += onTick;
            Tick += blip;
            Tick += vehicleTrack;

            EventHandlers["esx:setJob"] += new Action<string>(setJob);
            EventHandlers["Baitcar.Install"] += new Action(bc_install);
            EventHandlers["Baitcar.VehicleActivity"] += new Action<string>(vehicleActivity);
            EventHandlers["Baitcar.YouAreInBaitcar"] += new Action(inBaitcar);
            EventHandlers["Baitcar.Lock"] += new Action(lockCar);
            EventHandlers["Baitcar.ID"] += new Action<int>(checkIn);
            EventHandlers["Baitcar.UpdateBlip"] += new Action<float, float, float>(blipUpdate);
            EventHandlers["Baitcar.Unlocked"] += new Action(unlockCar);
            EventHandlers["Baitcar.Reset"] += new Action(resetCar);

            TriggerServerEvent("Baitcar.checkIn");
        }

        public async Task onTick()
        {
            if (IAmInBaitcar && CarLockeddown)
            {
                Game.DisableControlThisFrame(0, Control.VehicleExit);

                int _ped = GetPlayerPed(-1);
                int _veh = GetVehiclePedIsIn(_ped, false);

                if (GetPedInVehicleSeat(GetVehiclePedIsIn(GetPlayerPed(-1), false), -1) == GetPlayerPed(-1))
                {
                    SetVehicleEngineOn(_veh, false, true, false);
                    SetVehicleAlarm(_veh, true);
                    SetVehicleAlarmTimeLeft(_veh, 1000);
                    SetVehicleDoorsLockedForAllPlayers(_veh, true);
                    SetVehicleIndicatorLights(_veh, 0, true);
                    SetVehicleIndicatorLights(_veh, 1, true);
                }
            }

            await Delay(0);
        }

        public async Task blip()
        {
            if (IAmInBaitcar && GetPedInVehicleSeat(GetVehiclePedIsIn(GetPlayerPed(-1), false), -1) == GetPlayerPed(-1) && CarLockeddown)
            {
                Ped player = Game.PlayerPed;
                Vector3 position = player.Position;
                TriggerServerEvent("Baitcar.Blip", position.X, position.Y, position.Z);
            }
            await Delay(3000);
        }

        public async Task vehicleTrack()
        {
            int _veh = GetVehiclePedIsIn(GetPlayerPed(-1), false);
            if (_veh != 0 && !wasInCar)
            {
                TriggerServerEvent("Baitcar.EnteredCar", _veh);
                wasInCar = true;
            } else if (_veh == 0 && wasInCar && IAmInBaitcar)
            {
                TriggerServerEvent("Baitcar.ExitedBaitcar");
                wasInCar = IAmInBaitcar = false;
            }

            await Delay(0);
        }

        private void bc_install()
        {
            if (!hasControlJob) return;

            if (_baitCar != 0)
            {
                TriggerEvent("chat:addMessage", "[Baitcar]", new[] {255, 0, 0},
                    " There can only have 1 baitcar at a time. Please remove the equipment from the other baitcar.");
                return;
            }

            int _ped = GetPlayerPed(-1);
            int _veh = GetVehiclePedIsIn(_ped, false);
            if (_veh == 0)
            {
                TriggerEvent("chat:addMessage", "[Baitcar]", new[] { 255, 0, 0}, " Must be in vehicle to install the baitcar equipment.");
            }
            if (GetPedInVehicleSeat(_veh, -1) != _ped)
            {
                TriggerEvent("chat:addMessage", "[Baitcar]", new[] {255, 0, 0}, " Must be in driver's seaet.");
            }
            _baitCar = _veh;
            Screen.DisplayHelpTextThisFrame("Baitcar equipment has been installed.");
            TriggerServerEvent("Baitcar.Install", _veh);

        }

        private void resetCar()
        {
            if (!hasControlJob) return;

            int _veh = GetVehiclePedIsIn(GetPlayerPed(-1), false);

            if (GetPedInVehicleSeat(GetVehiclePedIsIn(GetPlayerPed(-1), false), -1) == GetPlayerPed(-1))
            {
                SetVehicleEngineOn(_veh, true, true, false);
                SetVehicleAlarm(_veh, false);
                SetVehicleAlarmTimeLeft(_veh, 0);
                SetVehicleDoorsLockedForAllPlayers(_veh, false);
                SetVehicleIndicatorLights(_veh, 0, false);
                SetVehicleIndicatorLights(_veh, 1, false);
            }
        }

        private void setJob(string job)
        {
            hasControlJob = config.jobs.Contains(job);
            Debug.WriteLine("[Baitcar] Has control job? {0}", hasControlJob.ToString());
        }

        private void vehicleActivity(string activity)
        {
            if (!hasControlJob) return;

            switch (activity)
            {
                case "entry":
                    Screen.ShowNotification("Baitcar door has been opened.");
                    break;
                case "movement":
                    Screen.ShowNotification("Baitcar is moving.");
                    break;
                case "armed":
                    Screen.ShowNotification("Baitcar is armed.");
                    break;
            }
        }

        private void inBaitcar()
        {
            IAmInBaitcar = true;
        }

        private void lockCar()
        {
            if (!IAmInBaitcar) return;

            CarLockeddown = true;
            // Only do engine stuff if in driver seat.

        }

        private void unlockCar()
        {
            if (!IAmInBaitcar) return;

            CarLockeddown = false;

            int _veh = GetVehiclePedIsIn(GetPlayerPed(-1), false);
            SetVehicleDoorsLockedForAllPlayers(_veh, true);
        }

        private void checkIn(int id)
        {
            Debug.WriteLine("[Baitcar]  Got response to check in, baitcar ID: {0}.", id.ToString());
        }

        private void blipUpdate(float x, float y, float z)
        {
            if (!hasControlJob) return;
            if (bcBlip != null) bcBlip.Delete();

            Vector3 pos = new Vector3(x, y, z);
            const BlipColor bc = BlipColor.Red;
            bcBlip = new Blip(World.CreateBlip(pos).Handle)
            {
                Sprite = BlipSprite.Handcuffs,
                Color = bc,
                IsShortRange = true,
                Name = "Baitcar",
                IsFlashing = false
            };
        }
    }

    sealed class Config
    {
        public string[] jobs;
    }
}
