using IngameScript.VectorThrustOS.Architecture.Abstractions;
using IngameScript.VectorThrustOS.Configuration;
using IngameScript.VectorThrustOS.Extensions;
using IngameScript.VectorThrustOS.Runtime;
using System.Collections.Generic;
using Sandbox.ModAPI.Ingame;
using System.Linq;
using System.Text;
using System;

namespace IngameScript.VectorThrustOS.SequenceRuntime.Sequences
{
    internal class BlockManagerSequence : BaseSequence<BlockManagerSequence>
    {
        List<IMyBatteryBlock> batteries = new();
        List<IMyBatteryBlock> backupBatteries = new();
        bool SetThrottle = false;
        bool donescan = false;
        bool changedruntime = false;

        private readonly SequenceAssigner<GetBatteryStatsSequence> _getBatteryStatsSequenceAssigner;

        public BlockManagerSequence(
            MyGridProgram baseProgram, 
            ThrustOSConfig thrustOSConfig, 
            RuntimeTracker runtimeTracker, 
            RuntimeProperties runtimeProperties, 
            RuntimeCollections runtimeCollections,
            SequenceAssigner<GetBatteryStatsSequence> getBatteryStatsSequenceAssigner,
            SequenceAssigner<BlockManagerSequence> parentSequenceAssigner
        ) : base(baseProgram, thrustOSConfig, runtimeTracker, runtimeProperties, runtimeCollections, parentSequenceAssigner)
        {
            _getBatteryStatsSequenceAssigner = getBatteryStatsSequenceAssigner;
        }       
        
        bool EndBatteryManagement(bool scanned, bool changedruntime)
        {
            if (scanned && RuntimeProperties.Parked && !ParentSequenceAssigner.Doneloop)
            {
                ParentSequenceAssigner.Doneloop = true;
                RuntimeTracker.ChangeRuntime(ThrustOSConfig.PerformanceWhilePark && wgv == 0 && !changedruntime ? 2 : 1);
            }
            else if (!RuntimeProperties.Parked)
            {
                RuntimeProperties.ParkedWithConnector = RuntimeProperties.AlreadyParked = false;
            }

            if (RuntimeProperties.Check && !this.changedruntime && RuntimeProperties.ParkedCompletely)
            {
                RuntimeTracker.ChangeRuntime();
                this.changedruntime = true;
            }
            else if (this.changedruntime && !RuntimeProperties.Check && RuntimeProperties.ParkedCompletely)
            {
                RuntimeTracker.ChangeRuntime(ThrustOSConfig.PerformanceWhilePark && RuntimeProperties.GravityStrength == 0 ? 2 : 1);
                this.changedruntime = false;
            }

            return true;
        }

        public override IEnumerable<int> Process()
        {
            while (true)
            {
                // Determine whether to turn off thrusters based on configuration and presence of normal thrusters
                bool TurnOffThrottle = ThrustOSConfig.TurnOffThrustersOnPark && !RuntimeCollections.NormalThrusters.Empty();

                if (TurnOffThrottle && (!SetThrottle || !RuntimeProperties.Parked))
                { // We either haven't set the throttle yet or the ship isn't currently parked
                    RuntimeCollections.NormalThrusters.ForEach(x => x.Enabled = !RuntimeProperties.Parked); // Disable thrusters if parked, enable if not parked
                    SetThrottle = true;
                }

                if (RuntimeCollections.NormalBatteries.Count + RuntimeCollections.TaggedBatteries.Count < 2 || !ThrustOSConfig.RechargeOnPark || !RuntimeProperties.ParkedWithConnector)
                {
                    changedruntime = EndBatteryManagement(true, changedruntime);
                    yield return ThrustOSConfig.FramesBetweenActions;
                    continue;
                }

                if (batteries.Empty() && backupBatteries.Empty())
                {
                    List<IMyBatteryBlock> allBatteries = new List<IMyBatteryBlock>(RuntimeCollections.TaggedBatteries)
                        .Concat(RuntimeCollections.NormalBatteries)
                        .ToList();

                    if (RuntimeProperties.Parked) yield return ThrustOSConfig.FramesBetweenActions;

                    backupBatteries = allBatteries.FindAll(x => 
                        x.CustomName.Contains(ThrustOSConfig.BackupSubstring) || 
                        RuntimeProperties.Greedy && ThrustOSConfig.RenameBackupSubstring && x.BlockDefinition.SubtypeId.Contains("SmallBattery")
                    );
                    
                    if (RuntimeProperties.Parked) yield return ThrustOSConfig.FramesBetweenActions;

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

                    else if (backupBatteries.Empty() && RuntimeCollections.TaggedBatteries.Count > RuntimeCollections.NormalBatteries.Count)
                    {
                        backupBatteries = new List<IMyBatteryBlock>(RuntimeCollections.NormalBatteries);

                        if (RuntimeCollections.NormalBatteries.Empty())
                        {
                            backupBatteries = new List<IMyBatteryBlock> { RuntimeCollections.TaggedBatteries.First() };
                        }

                        batteries = new List<IMyBatteryBlock>(RuntimeCollections.TaggedBatteries).Except(backupBatteries).ToList();
                    }
                    else if (backupBatteries.Empty())
                    {
                        backupBatteries.Add(RuntimeCollections.NormalBatteries.Empty() ? 
                            RuntimeCollections.TaggedBatteries[0] : RuntimeCollections.NormalBatteries[0]
                        );

                        batteries = batteries.Concat(RuntimeCollections.NormalBatteries)
                            .Concat(RuntimeCollections.TaggedBatteries)
                            .Except(backupBatteries)
                            .ToList();
                    }

                    //Getting at least 1 bat/tank to handle thrusters for a bit
                    if (!RuntimeProperties.Parked)
                    {
                        if (!batteries.Empty()) 
                            batteries[0].ChargeMode = ChargeMode.Auto;

                        if (!RuntimeCollections.TankBlocks.Empty()) 
                            RuntimeCollections.TankBlocks.First().Stockpile = false;
                    }

                    yield return ThrustOSConfig.FramesBetweenActions;
                }

                List<IMyPowerProducer> powerProducers = new List<IMyPowerProducer>();
                BaseProgram.GridTerminalSystem.GetBlocksOfType(powerProducers, x => !batteries.Contains(x) && !backupBatteries.Contains(x));
                yield return ThrustOSConfig.FramesBetweenActions;

                List<double> statsBBATS = new List<double>();
                List<double> statsPW = new List<double>();
                yield return ThrustOSConfig.FramesBetweenActions;

                if (!donescan || ParentSequenceAssigner.Doneloop && RuntimeProperties.Parked)
                {
                    if (RuntimeProperties.Parked)
                    { //temporary fix
                        RuntimeCollections.BatteriesSequence = new List<IMyTerminalBlock>(powerProducers);
                        while (!_getBatteryStatsSequenceAssigner.Doneloop)
                        {
                            _getBatteryStatsSequenceAssigner.Run();
                            yield return ThrustOSConfig.FramesBetweenActions;
                        }
                        _getBatteryStatsSequenceAssigner.Doneloop = false;

                        donescan = true;
                        statsPW = new List<double>(RuntimeCollections.OutputBatteriesSequence);
                    }
                    yield return ThrustOSConfig.FramesBetweenActions;

                    RuntimeCollections.BatteriesSequence = new List<IMyTerminalBlock>(backupBatteries);
                    while (!_getBatteryStatsSequenceAssigner.Doneloop)
                    {
                        _getBatteryStatsSequenceAssigner.Run();
                        yield return ThrustOSConfig.FramesBetweenActions;
                    }
                    _getBatteryStatsSequenceAssigner.Doneloop = false;

                    statsBBATS = new List<double>(RuntimeCollections.OutputBatteriesSequence);
                    yield return ThrustOSConfig.FramesBetweenActions;
                }

                bool lowbackupbat = !statsBBATS.Empty() && statsBBATS[2] < 2.5;
                bool comebackupbat = !statsBBATS.Empty() && statsBBATS[2] > 25;
                yield return ThrustOSConfig.FramesBetweenActions;

                bool charging = batteries.All(x => x.ChargeMode == ChargeMode.Recharge);
                RuntimeProperties.RechargeCancelled = statsPW.Empty() && donescan || !statsPW.Empty() && statsPW[1] == 0 || lowbackupbat;

                bool notcharged = RuntimeProperties.Parked && RuntimeProperties.ParkedWithConnector && donescan && 
                    (!charging && !RuntimeProperties.RechargeCancelled || charging && RuntimeProperties.RechargeCancelled) && !batteries.Empty();
                
                bool reassign = RuntimeProperties.RechargeCancelled && !statsPW.Empty() && statsPW[1] != 0 && comebackupbat;
                yield return ThrustOSConfig.FramesBetweenActions;

                if (!(!RuntimeProperties.Parked && RuntimeProperties.ParkedWithConnector || notcharged || reassign))
                {
                    changedruntime = EndBatteryManagement(donescan, changedruntime);
                    yield return ThrustOSConfig.FramesBetweenActions;
                    continue;
                }

                foreach (IMyGasTank curGasTank in RuntimeCollections.TankBlocks)
                {
                    curGasTank.Stockpile = !RuntimeProperties.RechargeCancelled && RuntimeProperties.Parked && curGasTank.FilledRatio != 1;
                    yield return ThrustOSConfig.FramesBetweenActions;
                }

                if (RuntimeProperties.Parked && !RuntimeProperties.RechargeCancelled)
                { //If I don't do this the ship will shut off
                    foreach (IMyBatteryBlock curBattery in backupBatteries)
                    {
                        curBattery.ChargeMode = RuntimeProperties.Parked && !RuntimeProperties.RechargeCancelled ? ChargeMode.Auto : ChargeMode.Recharge;
                        yield return ThrustOSConfig.FramesBetweenActions;
                    }
                    foreach (IMyBatteryBlock curBattery in batteries)
                    {
                        curBattery.ChargeMode = RuntimeProperties.Parked && !RuntimeProperties.RechargeCancelled ? ChargeMode.Recharge : ChargeMode.Auto;
                        yield return ThrustOSConfig.FramesBetweenActions;
                    }
                }
                else if (!RuntimeProperties.Parked || RuntimeProperties.RechargeCancelled)
                {
                    foreach (IMyBatteryBlock curBattery in batteries)
                    {
                        curBattery.ChargeMode = RuntimeProperties.Parked && !RuntimeProperties.RechargeCancelled ? ChargeMode.Recharge : ChargeMode.Auto;
                        yield return ThrustOSConfig.FramesBetweenActions;
                    }
                    foreach (IMyBatteryBlock curBattery in backupBatteries)
                    {
                        curBattery.ChargeMode = RuntimeProperties.Parked && !RuntimeProperties.RechargeCancelled ? ChargeMode.Auto : ChargeMode.Recharge;
                        yield return ThrustOSConfig.FramesBetweenActions;
                    }
                }

                changedruntime = EndBatteryManagement(donescan, changedruntime);
                yield return ThrustOSConfig.FramesBetweenActions;
            }
        }
    }
}
