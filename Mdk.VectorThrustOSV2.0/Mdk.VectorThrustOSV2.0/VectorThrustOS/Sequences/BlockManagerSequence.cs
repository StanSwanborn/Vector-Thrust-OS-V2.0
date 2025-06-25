using IngameScript.VectorThrustOS.Configuration;
using IngameScript.VectorThrustOS.Extensions;
using IngameScript.VectorThrustOS.Runtime;
using System.Collections.Generic;
using Sandbox.ModAPI.Ingame;
using System.Linq;
using System.Text;
using System;

namespace IngameScript.VectorThrustOS.Sequences
{
    internal class BlockManagerSequence
    {
        List<IMyBatteryBlock> batteries = new();
        List<IMyBatteryBlock> backupBatteries = new();
        bool SetThrottle = false;
        bool donescan = false;
        bool changedruntime = false;

        private readonly MyGridProgram _baseProgram;
        private readonly SequenceAssigner _baseSequenceAssigner;

        private readonly ThrustOSConfig _thrustOSConfig;
        private readonly RuntimeTracker _runtimeTracker;
        private readonly RuntimeProperties _runtimeProperties;
        private readonly RuntimeCollections _runtimeCollections;

        public BlockManagerSequence(
            MyGridProgram baseProgram, 
            ThrustOSConfig thrustOSConfig, 
            RuntimeTracker runtimeTracker, 
            RuntimeProperties runtimeProperties, 
            RuntimeCollections runtimeCollections,
            SequenceAssigner baseSequenceAssigner
        )
            
        {
            _baseProgram = baseProgram;
            _thrustOSConfig = thrustOSConfig;
            _runtimeTracker = runtimeTracker;
            _runtimeProperties = runtimeProperties;
            _runtimeCollections = runtimeCollections;
            _baseSequenceAssigner = baseSequenceAssigner;
        }
        
        bool EndBatteryManagement(bool scanned, bool changedruntime)
        {
            if (scanned && _runtimeProperties.Parked && !_baseSequenceAssigner.Doneloop)
            {
                _baseSequenceAssigner.Doneloop = true;
                _runtimeTracker.ChangeRuntime(_thrustOSConfig.PerformanceWhilePark && wgv == 0 && !changedruntime ? 2 : 1);
            }
            else if (!_runtimeProperties.Parked)
            {
                _runtimeProperties.ParkedWithConnector = _runtimeProperties.AlreadyParked = false;
            }

            if (_runtimeProperties.Check && !this.changedruntime && _runtimeProperties.ParkedCompletely)
            {
                _runtimeTracker.ChangeRuntime();
                this.changedruntime = true;
            }
            else if (this.changedruntime && !_runtimeProperties.Check && _runtimeProperties.ParkedCompletely)
            {
                _runtimeTracker.ChangeRuntime(_thrustOSConfig.PerformanceWhilePark && wgv == 0 ? 2 : 1);
                this.changedruntime = false;
            }

            return true;
        }

        public IEnumerable<int> Process()
        {
            while (true)
            {
                // Determine whether to turn off thrusters based on configuration and presence of normal thrusters
                bool TurnOffThrottle = _thrustOSConfig.TurnOffThrustersOnPark && !_runtimeCollections.NormalThrusters.Empty();

                if (TurnOffThrottle && (!SetThrottle || !_runtimeProperties.Parked))
                { // We either haven't set the throttle yet or the ship isn't currently parked
                    _runtimeCollections.NormalThrusters.ForEach(x => x.Enabled = !_runtimeProperties.Parked); // Disable thrusters if parked, enable if not parked
                    SetThrottle = true;
                }

                if ((_runtimeCollections.NormalBatteries.Count + _runtimeCollections.TaggedBatteries.Count < 2) || !_thrustOSConfig.RechargeOnPark || !_runtimeProperties.ParkedWithConnector)
                {
                    changedruntime = EndBatteryManagement(true, changedruntime);
                    yield return FramesBetweenActions;
                    continue;
                }

                if (batteries.Empty() && backupBatteries.Empty())
                {
                    List<IMyBatteryBlock> allBatteries = new List<IMyBatteryBlock>(_runtimeCollections.TaggedBatteries)
                        .Concat(_runtimeCollections.NormalBatteries)
                        .ToList();

                    if (_runtimeProperties.Parked) yield return FramesBetweenActions;

                    backupBatteries = allBatteries.FindAll(x => 
                        x.CustomName.Contains(_thrustOSConfig.BackupSubstring) || 
                        (_runtimeProperties.Greedy && _thrustOSConfig.RenameBackupSubstring && x.BlockDefinition.SubtypeId.Contains("SmallBattery"))
                    );
                    
                    if (_runtimeProperties.Parked) yield return FramesBetweenActions;

                    if (!backupBatteries.Empty() && allBatteries.SequenceEqual(backupBatteries))
                    {
                        batteries = new List<IMyBatteryBlock>(backupBatteries);
                        backupBatteries = new List<IMyBatteryBlock> { batteries[0] };
                        batteries.RemoveAt(0);
                    }
                    else if (!backupBatteries.Empty())
                    {
                        batteries = allBatteries.Except(backupBatteries).ToList();
                    }

                    else if (backupBatteries.Empty() && _runtimeCollections.TaggedBatteries.Count > _runtimeCollections.NormalBatteries.Count)
                    {
                        backupBatteries = new List<IMyBatteryBlock>(_runtimeCollections.NormalBatteries);

                        if (_runtimeCollections.NormalBatteries.Empty())
                        {
                            backupBatteries = new List<IMyBatteryBlock> { _runtimeCollections.TaggedBatteries.First() };
                        }

                        batteries = new List<IMyBatteryBlock>(_runtimeCollections.TaggedBatteries).Except(backupBatteries).ToList();
                    }
                    else if (backupBatteries.Empty())
                    {
                        backupBatteries.Add(_runtimeCollections.NormalBatteries.Empty() ? 
                            _runtimeCollections.TaggedBatteries[0] : _runtimeCollections.NormalBatteries[0]
                        );

                        batteries = batteries.Concat(_runtimeCollections.NormalBatteries)
                            .Concat(_runtimeCollections.TaggedBatteries)
                            .Except(backupBatteries)
                            .ToList();
                    }

                    //Getting at least 1 bat/tank to handle thrusters for a bit
                    if (!_runtimeProperties.Parked)
                    {
                        if (!batteries.Empty()) 
                            batteries[0].ChargeMode = ChargeMode.Auto;

                        if (!_runtimeCollections.TankBlocks.Empty()) 
                            _runtimeCollections.TankBlocks.First().Stockpile = false;
                    }

                    yield return FramesBetweenActions;
                }

                List<IMyPowerProducer> powerProducers = new List<IMyPowerProducer>();
                _baseProgram.GridTerminalSystem.GetBlocksOfType(powerProducers, x => !batteries.Contains(x) && !backupBatteries.Contains(x));
                yield return FramesBetweenActions;

                List<double> statsBBATS = new List<double>();
                List<double> statsPW = new List<double>();
                yield return FramesBetweenActions;

                if (!donescan || (BlockManager.Doneloop && _runtimeProperties.Parked))
                {
                    if (_runtimeProperties.Parked)
                    { //temporary fix
                        batsseq = new List<IMyTerminalBlock>(powerProducers);
                        while (!BatteryStats.Doneloop)
                        {
                            BatteryStats.Run();
                            yield return FramesBetweenActions;
                        }
                        BatteryStats.Doneloop = false;

                        donescan = true;
                        statsPW = new List<double>(outputbatsseq);
                    }
                    yield return FramesBetweenActions;

                    batsseq = new List<IMyTerminalBlock>(backupBatteries);
                    while (!BatteryStats.Doneloop)
                    {
                        BatteryStats.Run();
                        yield return FramesBetweenActions;
                    }
                    BatteryStats.Doneloop = false;

                    statsBBATS = new List<double>(outputbatsseq);
                    yield return FramesBetweenActions;
                }

                bool lowbackupbat = !statsBBATS.Empty() && statsBBATS[2] < 2.5;
                bool comebackupbat = !statsBBATS.Empty() && statsBBATS[2] > 25;
                yield return FramesBetweenActions;

                bool charging = batteries.All(x => x.ChargeMode == ChargeMode.Recharge);
                rechargecancelled = (statsPW.Empty() && donescan) || (!statsPW.Empty() && statsPW[1] == 0) || lowbackupbat;
                bool notcharged = parked && parkedwithcn && donescan && ((!charging && !rechargecancelled) || (charging && rechargecancelled)) && !batteries.Empty();
                bool reassign = rechargecancelled && !statsPW.Empty() && statsPW[1] != 0 && comebackupbat;
                yield return FramesBetweenActions;

                if (!((!parked && parkedwithcn) || notcharged || reassign))
                {
                    changedruntime = EndBatteryManagement(donescan, changedruntime);
                    yield return FramesBetweenActions;
                    continue;
                }

                foreach (IMyGasTank t in tankblocks)
                {
                    t.Stockpile = !rechargecancelled && parked && t.FilledRatio != 1;
                    yield return FramesBetweenActions;
                }

                if (parked && !rechargecancelled)
                { //If I don't do this the ship will shut off
                    foreach (IMyBatteryBlock b in backupBatteries)
                    {
                        b.ChargeMode = parked && !rechargecancelled ? ChargeMode.Auto : ChargeMode.Recharge;
                        yield return FramesBetweenActions;
                    }
                    foreach (IMyBatteryBlock b in batteries)
                    {
                        b.ChargeMode = parked && !rechargecancelled ? ChargeMode.Recharge : ChargeMode.Auto;
                        yield return FramesBetweenActions;
                    }
                }
                else if (!parked || rechargecancelled)
                {
                    foreach (IMyBatteryBlock b in batteries)
                    {
                        b.ChargeMode = parked && !rechargecancelled ? ChargeMode.Recharge : ChargeMode.Auto;
                        yield return FramesBetweenActions;
                    }
                    foreach (IMyBatteryBlock b in backupBatteries)
                    {
                        b.ChargeMode = parked && !rechargecancelled ? ChargeMode.Auto : ChargeMode.Recharge;
                        yield return FramesBetweenActions;
                    }
                }

                changedruntime = EndBatteryManagement(donescan, changedruntime);
                yield return FramesBetweenActions;
            }
        }
    }
}
