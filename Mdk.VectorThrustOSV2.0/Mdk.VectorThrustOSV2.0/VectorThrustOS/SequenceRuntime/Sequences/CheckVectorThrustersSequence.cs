using IngameScript.VectorThrustOS.Architecture.Abstractions;
using IngameScript.VectorThrustOS.Configuration;
using IngameScript.VectorThrustOS.Controllers;
using IngameScript.VectorThrustOS.Extensions;
using IngameScript.VectorThrustOS.Runtime;
using Sandbox.ModAPI.Ingame;
using SpaceEngineers.Game.ModAPI.Ingame;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml.Linq;

namespace IngameScript.VectorThrustOS.SequenceRuntime.Sequences
{
    internal class CheckVectorThrustersSequence : BaseSequence<CheckVectorThrustersSequence>
    {
        private readonly SequenceAssigner<> _getControllersSequenceAssigner;
        private readonly SequenceAssigner<> _getScreenSequenceAssigner;
        private readonly SequenceAssigner<> _checkParkBlocksSequenceAssigner;

        public CheckVectorThrustersSequence(
            MyGridProgram baseProgram,
            ThrustOSConfig thrustOSConfig,
            RuntimeTracker runtimeTracker,
            RuntimeProperties runtimeProperties,
            RuntimeCollections runtimeCollections,
            SequenceAssigner<> getControllersSequenceAssigner,
            SequenceAssigner<> getScreenSequenceAssigner,
            SequenceAssigner<> checkParkBlocksSequenceAssigner,
            SequenceAssigner<CheckVectorThrustersSequence> parentSequenceAssigner
        ) : base(baseProgram, thrustOSConfig, runtimeTracker, runtimeProperties, runtimeCollections, parentSequenceAssigner)
        {
            _getControllersSequenceAssigner = getControllersSequenceAssigner;
            _getScreenSequenceAssigner = getScreenSequenceAssigner;
            _checkParkBlocksSequenceAssigner = checkParkBlocksSequenceAssigner;
        }

        public override IEnumerable<int> Process()
        {
            while (true)
            {
                RuntimeProperties.PauseSequence = ((!RuntimeProperties.JustCompiled || (RuntimeProperties.JustCompiled && RuntimeProperties.Error)) && !RuntimeProperties.ApplyTags);
                
                if (RuntimeProperties.PauseSequence) 
                    yield return ThrustOSConfig.FramesBetweenActions;
                
                if (!RuntimeProperties.Check)
                {
                    if (GetControllers.Loop(RuntimeProperties.PauseSequence) || GetScreen.Loop(RuntimeProperties.PauseSequence) || CheckParkBlocks.Loop(RuntimeProperties.PauseSequence)) 
                        yield return ThrustOSConfig.FramesBetweenActions;

                    BaseProgram.Echo(" -Everything seems normal.");
                    continue;
                }

                if (!RuntimeProperties.JustCompiled) 
                    BaseProgram.Echo("  -Mass is different\n");

                List<IMyTerminalBlock> NewBlocks = new List<IMyTerminalBlock>();
                BaseProgram.GridTerminalSystem.GetBlocks(NewBlocks);

                if (!RuntimeProperties.ApplyTags && NewBlocks.Count.Equals(RuntimeProperties.BlockCount))
                {
                    RuntimeProperties.Check = false;
                    yield return ThrustOSConfig.FramesBetweenActions;
                    continue;
                }

                if (!RuntimeProperties.JustCompiled) BaseProgram.Echo("  -New blocks detected\n");

                List<IMyTerminalBlock> vtblocks = new List<IMyTerminalBlock>(RuntimeCollections.VTThrusters)
                    .Concat(RuntimeCollections.NormalThrusters).Concat(RuntimeCollections.VTRotors).Concat(RuntimeCollections.ControllerBlocks).ToList();

                bool search = vtblocks.Any(x => !BaseProgram.GridTerminalSystem.CanAccess(x));

                if (search)
                {
                    foreach (IMyTerminalBlock b in vtblocks)
                    {
                        if (!BaseProgram.GridTerminalSystem.CanAccess(b))
                        {
                            RuntimeCollections.AllBlocks.Remove(b);
                            RuntimeProperties.BlockCount--;

                            if (b is IMyShipController co)
                            {
                                // TODO: What the heck are CControllerBlocks and ControllerBlocks and ControlledControllers and Controllers?
                                RuntimeCollections.CControllerBlocks.Remove(co);
                                RuntimeCollections.ControllerBlocks.Remove(co);

                                RemoveSurfaceProvider(co);

                                RuntimeCollections.ControlledControllers.Remove(RuntimeCollections.ControlledControllers.Find(x => x.TheBlock.Equals(co)));
                                RuntimeCollections.Controllers.Remove(RuntimeCollections.Controllers.Find(x => x.TheBlock.Equals(co)));

                                if (RuntimeCollections.ControlledControllers.Empty())
                                {
                                    BaseProgram.Echo($"ERROR -> Any Usable Controller Found, Shutting Down");
                                    ManageTag(true);
                                    RuntimeProperties.Error = true;
                                    yield return ThrustOSConfig.FramesBetweenActions;
                                }

                                RuntimeProperties.MainController = RuntimeCollections.ControlledControllers[0];
                            }
                            else if (b is IMyThrust)
                            {
                                IMyThrust tr = (IMyThrust)b;

                                RuntimeCollections.AbandonedThrusters.Remove(tr);
                                RuntimeCollections.CruiseThrottleCollection.Remove(tr);
                                bool oldnthr = !RuntimeCollections.NormalThrusters.Empty();
                                RuntimeCollections.NormalThrusters.Remove(tr);

                                if (RuntimeCollections.NormalThrusters.Empty() && oldnthr)
                                {
                                    RuntimeProperties.Dampeners = true; //Put dampeners back on if normalthrusters got removed entirely
                                }
                                RuntimeCollections.VTThrusters.Remove(tr);
                            }
                            else
                            {
                                RuntimeCollections.VTRotors.Remove((IMyMotorStator)b);
                                RuntimeCollections.AbandonedRotors.Remove((IMyMotorStator)b);
                            }
                        }
                    }
                }


                List<IMyTerminalBlock> newvtblocks = new List<IMyTerminalBlock>(NewBlocks)
                    .FindAll(x => x is IMyThrust || x is IMyMotorStator || x is IMyShipController)
                    .Except(RuntimeCollections.NormalThrusters).Except(RuntimeCollections.VTRotors).Except(RuntimeCollections.VTThrusters).Except(RuntimeCollections.ControllerBlocks).ToList();

                if (RuntimeProperties.ApplyTags || !newvtblocks.Empty())
                {
                    foreach (IMyTerminalBlock b in newvtblocks)
                    {
                        if (b is IMyShipController controller)
                        {
                            AddBlock(b, RuntimeCollections.ControllerBlocks);
                            RuntimeCollections.ControllersInput.Add(new ShipController(controller, BaseProgram));
                        }
                        else if (b is IMyThrust)
                        {
                            if (!FilterThis(b)) AddBlock(b, RuntimeCollections.ThrustersInput);
                            else
                            {
                                IMyThrust tr = (IMyThrust)b;
                                AddBlock(b, RuntimeCollections.NormalThrusters);
                                if (b.Orientation.Forward == RuntimeProperties.MainController.TheBlock.Orientation.Forward) //changing
                                {
                                    RuntimeCollections.CruiseThrottleCollection.Add(tr);
                                }
                                if (!RuntimeProperties.JustCompiled && ThrustOSConfig.StockValues) (b as IMyFunctionalBlock).Enabled = true;
                            }
                        }
                        else
                        {
                            AddBlock(b, RuntimeCollections.RotorsInput);
                        }
                    }

                    GetControllers.Loop(false);
                    GetVectorThrusters.Loop(false);

                    //if (GetControllers.Loop(RuntimeProperties.PauseSequence)) yield return timepause;
                    //if (GetVectorThrusters.Loop(RuntimeProperties.PauseSequence)) yield return timepause;
                }

                if (RuntimeProperties.PauseSequence) yield return ThrustOSConfig.FramesBetweenActions;

                foreach (VectorThrust vt in RuntimeCollections.VectorThrusters)
                {
                    vt.thrusters.RemoveAll(x => !RuntimeCollections.VTThrusters.Contains(x.TheBlock));
                    vt.activeThrusters.RemoveAll(x => !vt.thrusters.Contains(x));
                    vt.availableThrusters.RemoveAll(x => !vt.thrusters.Contains(x));
                    if (RuntimeProperties.PauseSequence) yield return ThrustOSConfig.FramesBetweenActions;
                }


                for (int i = RuntimeCollections.VectorThrusters.Count - 1; i >= 0; i--)
                {
                    VectorThrust vt = RuntimeCollections.VectorThrusters[i];
                    IMyMotorStator rt = vt.rotor.TheBlock;

                    if (!RuntimeCollections.VTRotors.Contains(rt) || rt.Top == null || vt.thrusters.Empty())
                    {
                        rt.Brake();
                        RuntimeCollections.VectorThrusters.RemoveAt(i);
                        if (!RuntimeCollections.AbandonedRotors.Contains(rt)) RuntimeCollections.AbandonedRotors.Add(rt);
                    }
                    if (RuntimeProperties.PauseSequence) yield return ThrustOSConfig.FramesBetweenActions;
                }

                foreach (List<VectorThrust> group in RuntimeCollections.VTThrottleGroups)
                {
                    group.RemoveAll(x => !RuntimeCollections.VectorThrusters.Contains(x) || x.thrusters.Count < 1);
                    if (RuntimeProperties.PauseSequence) yield return ThrustOSConfig.FramesBetweenActions;
                }

                RuntimeCollections.VTThrottleGroups.RemoveAll(x => x.Count < 1);
                if (RuntimeProperties.PauseSequence) yield return ThrustOSConfig.FramesBetweenActions;

                for (int i = RuntimeCollections.AllBlocks.Count - 1; i >= 0; i--)
                {
                    IMyTerminalBlock curBlock = RuntimeCollections.AllBlocks[i];

                    bool tagallcond = RuntimeProperties.TagAll && (curBlock is IMyBatteryBlock || curBlock is IMyGasTank || curBlock is IMyLandingGear || curBlock is IMyShipConnector);
                    bool tagcond = curBlock is IMyShipController || RuntimeCollections.VTThrusters.Contains(curBlock) || curBlock is IMyMotorStator;

                    if (!BaseProgram.GridTerminalSystem.CanAccess(curBlock))
                    {
                        RuntimeCollections.AllBlocks.RemoveAt(i);

                        if (curBlock is IMyLandingGear landingGear)
                        {
                            RuntimeCollections.LandingGearBlocks.Remove(landingGear);
                            RuntimeCollections.LandingGears.Remove(landingGear);
                        }
                        else if (curBlock is IMyShipConnector shipConnector)
                        {
                            RuntimeCollections.ConnectorBlocks.Remove(shipConnector);
                            RuntimeCollections.Connectors.Remove(shipConnector);
                        }
                        else if (curBlock is IMyGasTank gasTank)
                        {
                            RuntimeCollections.TankBlocks.Remove(gasTank);
                        }
                        else if (curBlock is IMyBatteryBlock)
                        {
                            RuntimeCollections.BatteriesBlocks.Remove((IMyBatteryBlock)curBlock);
                            RuntimeCollections.TaggedBatteries.Remove((IMyBatteryBlock)curBlock);
                            RuntimeCollections.NormalBatteries.Remove((IMyBatteryBlock)curBlock);
                        }
                        else if (curBlock is IMyTextPanel)
                        {
                            RuntimeCollections.Screens.Remove((IMyTextPanel)curBlock);
                            RemoveSurface((IMyTextPanel)curBlock);
                        }
                        else if (curBlock is IMySoundBlock)
                        {
                            RuntimeCollections.SoundBlocks.Remove((IMySoundBlock)curBlock);
                        }

                    }
                    else if (RuntimeProperties.ApplyTags && (tagallcond || tagcond))
                    {
                        AddTag(curBlock);
                        if (ThrustOSConfig.RenameBackupSubstring && curBlock.BlockDefinition.SubtypeId.Contains("SmallBattery") && !curBlock.CustomName.Contains(ThrustOSConfig.BackupSubstring))
                        {
                            curBlock.CustomName += " " + ThrustOSConfig.BackupSubstring;
                        }
                    }

                    if (RuntimeProperties.PauseSequence) yield return ThrustOSConfig.FramesBetweenActions;
                }

                List<IMyTerminalBlock> blocks = new List<IMyTerminalBlock>(NewBlocks).Except(RuntimeCollections.AllBlocks).ToList();
                if (RuntimeProperties.PauseSequence) yield return ThrustOSConfig.FramesBetweenActions;

                foreach (IMyTerminalBlock b in blocks)
                {
                    if (BaseProgram.GridTerminalSystem.CanAccess(b))
                    {
                        bool island = b is IMyLandingGear;
                        bool iscon = b is IMyShipConnector;
                        bool samegrid = FilterThis(b);
                        bool hastag = HasTag(b);
                        bool xor = samegrid || hastag;

                        if (b is IMyTextPanel)
                        {
                            AddBlock(b, RuntimeCollections.InputScreens);
                        }
                        else if (iscon)
                        {
                            if (RuntimeProperties.TagAll) AddTag(b);
                            bool cond1 = AutoAddGridConnectors && xor;
                            bool cond2 = !AutoAddGridConnectors && hastag;

                            bool cncond = cond1 || cond2;

                            AddBlock(b, ref RuntimeCollections.ConnectorBlocks);
                            if (cncond && !RuntimeCollections.Connectors.Contains(b)) RuntimeCollections.Connectors.Add((IMyShipConnector)b);
                        }
                        else if (island)
                        {
                            if (RuntimeProperties.TagAll) AddTag(b);
                            bool cond3 = AutoAddGridLandingGears && xor;
                            bool cond4 = !AutoAddGridLandingGears && hastag;

                            bool lgcond = cond3 || cond4;

                            AddBlock(b, RuntimeCollections.LandingGearBlocks);
                            if (lgcond && !RuntimeCollections.LandingGears.Contains(b)) RuntimeCollections.LandingGears.Add((IMyLandingGear)b);
                        }
                        else if (b is IMyGasTank && (hastag || RuntimeProperties.TagAll || samegrid))
                        {
                            if (RuntimeProperties.TagAll) AddTag(b);
                            AddBlock(b, RuntimeCollections.TankBlocks);
                            if (hastag && ThrustOSConfig.StockValues) (b as IMyGasTank).Stockpile = false;
                        }
                        else if (b is IMyBatteryBlock)
                        {
                            IMyBatteryBlock bat = (IMyBatteryBlock)b;

                            if (RuntimeProperties.TagAll)
                            {
                                AddTag(b);
                                //log.AppendNR("Cacoide1"); TODO, THIS SEEMS TO NOT HAVE ANY EFFECT
                                if (ThrustOSConfig.RenameBackupSubstring && b.BlockDefinition.SubtypeId.Contains("SmallBattery") && !b.CustomName.Contains(ThrustOSConfig.BackupSubstring))
                                {
                                    b.CustomName += " " + ThrustOSConfig.BackupSubstring;
                                    //log.AppendNR("Cacoide");
                                }
                            }
                            if (RuntimeProperties.JustCompiled && (hastag || samegrid) && ThrustOSConfig.StockValues) bat.ChargeMode = ChargeMode.Auto;

                            if (hastag) AddBlock(b, RuntimeCollections.TaggedBatteries);
                            else if (samegrid) AddBlock(b, RuntimeCollections.NormalBatteries);
                            else AddBlock(b, RuntimeCollections.BatteriesBlocks);
                        }
                        else if (b is IMySoundBlock && hastag)
                        {
                            IMySoundBlock sb = (IMySoundBlock)b;
                            sb.LoopPeriod = 1;
                            sb.SelectedSound = "Alert 2";
                            AddBlock(b, RuntimeCollections.SoundBlocks);
                        }
                    }
                    if (RuntimeProperties.PauseSequence) yield return ThrustOSConfig.FramesBetweenActions;
                }

                if (!RuntimeProperties.JustCompiled && CheckParkBlocks.Loop(RuntimeProperties.PauseSequence)) yield return ThrustOSConfig.FramesBetweenActions; //Causes script too complex if done in one run

                if (GetScreen.Loop(RuntimeProperties.PauseSequence)) yield return ThrustOSConfig.FramesBetweenActions;

                // TODO: Investigate if this is necessary
                LND(ref RuntimeCollections.ControllerBlocks);
                if (RuntimeProperties.PauseSequence) yield return ThrustOSConfig.FramesBetweenActions;

                LND(ref RuntimeCollections.VectorThrusters);
                if (RuntimeProperties.PauseSequence) yield return ThrustOSConfig.FramesBetweenActions;

                LND(ref RuntimeCollections.NormalThrusters);
                if (RuntimeProperties.PauseSequence) yield return ThrustOSConfig.FramesBetweenActions;

                LND(ref RuntimeCollections.VTThrusters);
                if (RuntimeProperties.PauseSequence) yield return ThrustOSConfig.FramesBetweenActions;

                LND(ref RuntimeCollections.VTRotors);
                if (RuntimeProperties.PauseSequence) yield return ThrustOSConfig.FramesBetweenActions;

                LND(ref RuntimeCollections.CControllerBlocks);
                if (RuntimeProperties.PauseSequence) yield return ThrustOSConfig.FramesBetweenActions;

                LND(ref RuntimeCollections.ControlledControllers);
                if (RuntimeProperties.PauseSequence) yield return ThrustOSConfig.FramesBetweenActions;

                RuntimeProperties.Check = false;
                RuntimeProperties.BlockCount = NewBlocks.Count;
                yield return ThrustOSConfig.FramesBetweenActions;
            }
        }
    }
}
