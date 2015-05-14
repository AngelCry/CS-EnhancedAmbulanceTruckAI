using System;
using System.Collections.Generic;
using System.Threading;

using ICities;
using ColossalFramework;
using ColossalFramework.Math;
using ColossalFramework.Plugins;
using ColossalFramework.UI;
using UnityEngine;

namespace EnhancedAmbulanceAI
{
    public class Dispatcher : ThreadingExtensionBase
    {
        private Settings _settings;
        private Helper _helper;

        private string _collecting = ColossalFramework.Globalization.Locale.Get("VEHICLE_STATUS_AMBULANCE_EMERGENCY");

        private bool _initialized;
        private bool _baselined;
        private bool _terminated;

        private Dictionary<ushort, Clinic> _clinics;
        private Dictionary<ushort, DateTime> _master;

        protected bool IsOverwatched()
        {
            #if DEBUG

            foreach (var plugin in PluginManager.instance.GetPluginsInfo())
            {
                _helper.NotifyPlayer("Plugin: "+plugin.name+" ID: "+plugin.publishedFileID);
            }

            return true;

            #else

            foreach (var plugin in PluginManager.instance.GetPluginsInfo())
            {
                if (plugin.publishedFileID.AsUInt64 == 421028969)
                    return true;
            }

            return false;

            #endif
        }

        public override void OnCreated(IThreading threading)
        {
            _settings = Settings.Instance;
            _helper = Helper.Instance;

            _initialized = false;
            _baselined = false;
            _terminated = false;

            base.OnCreated(threading);
        }

        public override void OnBeforeSimulationTick()
        {
            if (_terminated) return;

            if (!_helper.GameLoaded)
            {
                _initialized = false;
                _baselined = false;
                return;
            }

            base.OnBeforeSimulationTick();
        }

        public override void OnUpdate(float realTimeDelta, float simulationTimeDelta)
        {
            if (_terminated) return;

            if (!_helper.GameLoaded) return;

            try
            {
                if (!_initialized)
                {
                    if (!IsOverwatched())
                    {
                        _helper.NotifyPlayer("Skylines Overwatch not found. Terminating...");
                        _terminated = true;

                        return;
                    }

                    SkylinesOverwatch.Settings.Instance.Enable.BuildingMonitor = true;
                    SkylinesOverwatch.Settings.Instance.Enable.VehicleMonitor = true;

                    _clinics = new Dictionary<ushort, Clinic>();
                    _master = new Dictionary<ushort, DateTime>();

                    _initialized = true;

                    _helper.NotifyPlayer("Initialized");
                }
                else if (!_baselined)
                {
                    CreateBaseline();
                }
                else
                {
                    ProcessNewClinics();
                    ProcessRemovedClinics();

                    ProcessNewPickups();

                    ProcessIdleAmbulanceTrucks();
                    UpdateAmbulanceTrucks();
                }
            }
            catch (Exception e)
            {
                string error = String.Format("Failed to {0}\r\n", !_initialized ? "initialize" : "update");
                error += String.Format("Error: {0}\r\n", e.Message);
                error += "\r\n";
                error += "==== STACK TRACE ====\r\n";
                error += e.StackTrace;

                _helper.Log(error);

                if (!_initialized)
                    _terminated = true;
            }

            base.OnUpdate(realTimeDelta, simulationTimeDelta);
        }

        public override void OnReleased()
        {
            _initialized = false;
            _baselined = false;
            _terminated = false;

            base.OnReleased();
        }

        private void CreateBaseline()
        {
            SkylinesOverwatch.Data data = SkylinesOverwatch.Data.Instance;

            foreach (ushort id in data.Hospitals)
                _clinics.Add(id, new Clinic(id, ref _master));

            foreach (ushort pickup in data.BuildingsWithSick)
            {
                foreach (ushort id in _clinics.Keys)
                    _clinics[id].AddPickup(pickup);
            }

            _baselined = true;
        }

        private void ProcessNewClinics()
        {
            SkylinesOverwatch.Data data = SkylinesOverwatch.Data.Instance;

            foreach (ushort x in data.BuildingsUpdated)
            {
                if (!data.IsHospital(x))
                    continue;

                if (_clinics.ContainsKey(x))
                    continue;

                _clinics.Add(x, new Clinic(x, ref _master));

                foreach (ushort pickup in data.BuildingsWithGarbage)
                    _clinics[x].AddPickup(pickup);
            }
        }

        private void ProcessRemovedClinics()
        {
            SkylinesOverwatch.Data data = SkylinesOverwatch.Data.Instance;

            foreach (ushort id in data.BuildingsRemoved)
                _clinics.Remove(id);
        }

        private void ProcessNewPickups()
        {
            SkylinesOverwatch.Data data = SkylinesOverwatch.Data.Instance;

            foreach (ushort pickup in data.BuildingsUpdated)
            {
                if (data.IsBuildingWithSick(pickup))
                {
                    foreach (ushort id in _clinics.Keys)
                        _clinics[id].AddPickup(pickup);
                }
                else
                {
                    foreach (ushort id in _clinics.Keys)
                        _clinics[id].AddCheckup(pickup);
                }
            }
        }

        private void ProcessIdleAmbulanceTrucks()
        {
            SkylinesOverwatch.Data data = SkylinesOverwatch.Data.Instance;

            foreach (ushort x in data.BuildingsUpdated)
            {
                if (!data.IsHospital(x))
                    continue;

                if (!_clinics.ContainsKey(x))
                    continue;
                #if DEBUG

                _helper.NotifyPlayer("[ARIS] send ambulance to target...");

                #endif

                _clinics[x].DispatchIdleVehicle();
            }
        }

        private void UpdateAmbulanceTrucks()
        {
            SkylinesOverwatch.Data data = SkylinesOverwatch.Data.Instance;
            Vehicle[] vehicles = Singleton<VehicleManager>.instance.m_vehicles.m_buffer;
            InstanceID instanceID = new InstanceID();

            foreach (ushort id in data.VehiclesUpdated)
            {
                if (!data.IsAmbulance(id))
                    continue;

                Vehicle v = vehicles[id];

                if (!_clinics.ContainsKey(v.m_sourceBuilding))
                    continue;

                if (v.Info.m_vehicleAI.GetLocalizedStatus(id, ref v, out instanceID) != _collecting)
                    continue;

                ushort target = _clinics[v.m_sourceBuilding].AssignTarget(v);

                if (target != 0 && target != v.m_targetBuilding)
                    v.Info.m_vehicleAI.SetTarget(id, ref vehicles[id], target);
            }
        }
    }
}

