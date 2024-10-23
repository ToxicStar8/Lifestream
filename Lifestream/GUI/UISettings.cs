﻿using ECommons.Configuration;
using FFXIVClientStructs.FFXIV.Client.UI;
using Lifestream.Tasks.Shortcuts;
using Lumina.Excel.GeneratedSheets;
using NightmareUI;
using NightmareUI.PrimaryUI;
using Action = System.Action;

namespace Lifestream.GUI;

internal static unsafe class UISettings
{
    private static string AddNew = "";
    internal static void Draw()
    {
        NuiTools.ButtonTabs([[new("常规", () => Wrapper(DrawGeneral)), new("悬浮窗", () => Wrapper(DrawOverlay)), new("专业", () => Wrapper(DrawExpert)), new("账号", () => Wrapper(UIServiceAccount.Draw))]]);
    }

    private static void Wrapper(Action action)
    {
        ImGui.Dummy(new(5f));
        action();
    }

    private static void DrawGeneral()
    {
        new NuiBuilder()
        .Section("传送配置")
        .Widget(() =>
        {
            ImGui.SetNextItemWidth(200f);
            ImGuiEx.EnumCombo($"跨服传送水晶", ref P.Config.WorldChangeAetheryte, Lang.WorldChangeAetherytes);
            ImGuiEx.HelpMarker($"你想传送到哪里来跨服");
            ImGui.Checkbox($"访问 服务器/大区 后传送到特定的以太网目的地", ref P.Config.WorldVisitTPToAethernet);
            if(P.Config.WorldVisitTPToAethernet)
            {
                ImGui.Indent();
                ImGui.SetNextItemWidth(250f);
                ImGui.InputText("以太网目的地，就像您在“/li”命令中使用的一样", ref P.Config.WorldVisitTPTarget, 50);
                ImGui.Checkbox($"只作用于使用命令传送的情况，不作用于从悬浮窗传送的情况", ref P.Config.WorldVisitTPOnlyCmd);
                ImGui.Unindent();
            }
            ImGui.Checkbox($"将天穹街添加到伊修加德基础层以太之光中", ref P.Config.Firmament);
            ImGui.Checkbox($"跨服时自动离开非跨服队伍", ref P.Config.LeavePartyBeforeWorldChange);
            ImGui.Checkbox($"在聊天中显示传送目的地", ref P.Config.DisplayChatTeleport);
            ImGui.Checkbox($"在弹出通知中显示传送目的地", ref P.Config.DisplayPopupNotifications);
            ImGui.Checkbox("重试同服务器失败的服务器访问", ref P.Config.RetryWorldVisit);
            ImGui.Indent();
            ImGui.SetNextItemWidth(100f);
            ImGui.InputInt("重试间隔，秒##2", ref P.Config.RetryWorldVisitInterval.ValidateRange(1, 120));
            ImGui.SameLine();
            ImGuiEx.Text("+ 最多");
            ImGui.SameLine();
            ImGui.SetNextItemWidth(100f);
            ImGui.InputInt("秒##2", ref P.Config.RetryWorldVisitIntervalDelta.ValidateRange(0, 120));
            ImGuiEx.HelpMarker("为了让它看起来不那么像机器人");
            ImGui.Unindent();
            //ImGui.Checkbox("Use Return instead of Teleport when possible", ref P.Config.UseReturn);
            //ImGuiEx.HelpMarker("This includes any IPC calls");
        })

        .Section("捷径")
        .Widget(() =>
        {
            ImGui.Checkbox("当传送到你自己的公寓时，进入里面", ref P.Config.EnterMyApartment);
            ImGui.SetNextItemWidth(150f);
            ImGuiEx.EnumCombo("当传送到自己/部队房屋时，执行此操作", ref P.Config.HouseEnterMode);
            ImGui.SetNextItemWidth(150f);
            if(ImGui.BeginCombo("首选旅馆", Utils.GetInnNameFromTerritory(P.Config.PreferredInn), ImGuiComboFlags.HeightLarge))
            {
                foreach(var x in (uint[])[0, .. TaskPropertyShortcut.InnData.Keys])
                {
                    if(ImGui.Selectable(Utils.GetInnNameFromTerritory(x), x == P.Config.PreferredInn)) P.Config.PreferredInn = x;
                }
                ImGui.EndCombo();
            }
            ImGui.Separator();
            ImGuiEx.Text("\"/li auto\" 命令优先级:");
            for(var i = 0; i < P.Config.PropertyPrio.Count; i++)
            {
                var d = P.Config.PropertyPrio[i];
                ImGui.PushID($"c{i}");
                if(ImGui.ArrowButton("##up", ImGuiDir.Up) && i > 0)
                {
                    try
                    {
                        (P.Config.PropertyPrio[i - 1], P.Config.PropertyPrio[i]) = (P.Config.PropertyPrio[i], P.Config.PropertyPrio[i - 1]);
                    }
                    catch(Exception e)
                    {
                        e.Log();
                    }
                }
                ImGui.SameLine();
                if(ImGui.ArrowButton("##down", ImGuiDir.Down) && i < P.Config.PropertyPrio.Count - 1)
                {
                    try
                    {
                        (P.Config.PropertyPrio[i + 1], P.Config.PropertyPrio[i]) = (P.Config.PropertyPrio[i], P.Config.PropertyPrio[i + 1]);
                    }
                    catch(Exception e)
                    {
                        e.Log();
                    }
                }
                ImGui.SameLine();
                ImGui.Checkbox($"{d.Type}", ref d.Enabled);
                ImGui.PopID();
            }
            ImGui.Separator();
        })

        .Section("地图整合")
        .Widget(() =>
        {
            ImGui.Checkbox("单击地图上的城内以太水晶可快速传送", ref P.Config.UseMapTeleport);
        })

        .Section("跨大区")
        .Widget(() =>
        {
            ImGui.Checkbox($"允许前往另一个大区", ref P.Config.AllowDcTransfer);
            ImGui.Checkbox($"切换大区前离开队伍", ref P.Config.LeavePartyBeforeLogout);
            ImGui.Checkbox($"如果不在休息区，则在跨大区之前传送到以太之光", ref P.Config.TeleportToGatewayBeforeLogout);
            ImGui.Checkbox($"完成跨大区后传送到以太之光", ref P.Config.DCReturnToGateway);
            ImGui.Checkbox($"跨大区期间允许选择服务器", ref P.Config.DcvUseAlternativeWorld);
            ImGuiEx.HelpMarker("如果目标服务器不可用，但目标大区上的其他服务器可用，则会选择该服务器。正常登录后会切换服务器。");
            ImGui.Checkbox($"如果目标服务器不可用，重试跨大区", ref P.Config.EnableDvcRetry);
            ImGui.Indent();
            ImGui.SetNextItemWidth(150f);
            ImGui.InputInt("最大重试次数", ref P.Config.MaxDcvRetries.ValidateRange(1, int.MaxValue));
            ImGui.SetNextItemWidth(150f);
            ImGui.InputInt("重试间隔，秒", ref P.Config.DcvRetryInterval.ValidateRange(10, 1000));
            ImGui.Unindent();
        })

        .Section("地址簿")
        .Widget(() =>
        {
            ImGui.Checkbox($"禁用寻路到地块", ref P.Config.AddressNoPathing);
            ImGuiEx.HelpMarker($"您将被留在距离地块最近的以太水晶");
            ImGui.Checkbox($"禁止进入公寓", ref P.Config.AddressApartmentNoEntry);
            ImGuiEx.HelpMarker($"您将看到进入确认对话框");
        })

        .Section("移动")
        .Checkbox("自动移动时使用 冲刺 和 速行", () => ref P.Config.UseSprintPeloton)

        .Section("Character Select Menu")
        .Checkbox("Enable Data center and World visit from Character Select Menu", () => ref P.Config.AllowDCTravelFromCharaSelect)
        .Checkbox("Use world visit instead of DC visit to travel to same world on guest DC", () => ref P.Config.UseGuestWorldTravel)

        .Draw();
    }

    private static void DrawOverlay()
    {
        new NuiBuilder()
        .Section("常规悬浮窗设置")
        .Widget(() =>
        {
            ImGui.Checkbox("启用悬浮窗", ref P.Config.Enable);
            if(P.Config.Enable)
            {
                ImGui.Indent();
                ImGui.Checkbox($"显示城内以太水晶菜单", ref P.Config.ShowAethernet);
                ImGui.Checkbox($"显示服务器菜单", ref P.Config.ShowWorldVisit);
                ImGui.Checkbox($"显示房区按钮", ref P.Config.ShowWards);

                UtilsUI.NextSection();

                ImGui.Checkbox("固定Lifestream悬浮窗位置", ref P.Config.FixedPosition);
                if(P.Config.FixedPosition)
                {
                    ImGui.Indent();
                    ImGui.SetNextItemWidth(200f);
                    ImGuiEx.EnumCombo("水平位置", ref P.Config.PosHorizontal);
                    ImGui.SetNextItemWidth(200f);
                    ImGuiEx.EnumCombo("垂直位置", ref P.Config.PosVertical);
                    ImGui.SetNextItemWidth(200f);
                    ImGui.DragFloat2("偏移", ref P.Config.Offset);

                    ImGui.Unindent();
                }

                UtilsUI.NextSection();

                ImGui.SetNextItemWidth(100f);
                ImGui.InputInt3("按钮左/右内边距", ref P.Config.ButtonWidthArray[0]);
                ImGui.SetNextItemWidth(100f);
                ImGui.InputInt("以太水晶按钮顶部/底部填充", ref P.Config.ButtonHeightAetheryte);
                ImGui.SetNextItemWidth(100f);
                ImGui.InputInt("服务器按钮顶部/底部填充", ref P.Config.ButtonHeightWorld);
                ImGui.Unindent();
            }
        })

        .Section("副本区切换")
        .Checkbox("启用", () => ref P.Config.ShowInstanceSwitcher)
        .Checkbox("失败时重试", () => ref P.Config.InstanceSwitcherRepeat)
        .Checkbox("切换副本区前飞行时返回地面", () => ref P.Config.EnableFlydownInstance)
        .SliderInt(150f, "额外按钮高度", () => ref P.Config.InstanceButtonHeight, 0, 50)
        .Widget("重置副本区数据", (x) =>
        {
            if(ImGuiEx.Button(x, P.Config.PublicInstances.Count > 0))
            {
                P.Config.PublicInstances.Clear();
                EzConfig.Save();
            }
        })

        .Section("游戏窗口集成")
        .Checkbox($"如果打开以下游戏窗口，则隐藏 Lifestream", () => ref P.Config.HideAddon)
        .If(() => P.Config.HideAddon)
        .Widget(() =>
        {
            if(ImGui.BeginTable("HideAddonTable", 2, ImGuiTableFlags.BordersInnerH | ImGuiTableFlags.BordersOuter | ImGuiTableFlags.RowBg | ImGuiTableFlags.SizingFixedFit))
            {
                ImGui.TableSetupColumn("col1", ImGuiTableColumnFlags.WidthStretch);
                ImGui.TableSetupColumn("col2");

                ImGui.TableNextRow();
                ImGui.TableNextColumn();
                ImGuiEx.SetNextItemFullWidth();
                ImGui.InputTextWithHint("##addnew", "窗口名称... /xldata ai - 来查找", ref AddNew, 100);
                ImGui.TableNextColumn();
                if(ImGuiEx.IconButton(FontAwesomeIcon.Plus))
                {
                    P.Config.HideAddonList.Add(AddNew);
                    AddNew = "";
                }

                List<string> focused = [];
                try
                {
                    foreach(var x in RaptureAtkUnitManager.Instance()->FocusedUnitsList.Entries)
                    {
                        if(x.Value == null) continue;
                        focused.Add(x.Value->NameString);
                    }
                }
                catch(Exception e) { e.Log(); }

                if(focused != null)
                {
                    foreach(var name in focused)
                    {
                        if(name == null) continue;
                        if(P.Config.HideAddonList.Contains(name)) continue;
                        ImGui.TableNextRow();
                        ImGui.TableNextColumn();
                        ImGuiEx.TextV(EColor.Green, $"Focused: {name}");
                        ImGui.TableNextColumn();
                        ImGui.PushID(name);
                        if(ImGuiEx.IconButton(FontAwesomeIcon.Plus))
                        {
                            P.Config.HideAddonList.Add(name);
                        }
                        ImGui.PopID();
                    }
                }

                ImGui.TableNextRow();
                ImGui.TableSetBgColor(ImGuiTableBgTarget.RowBg0, 0x88888888);
                ImGui.TableSetBgColor(ImGuiTableBgTarget.RowBg1, 0x88888888);
                ImGui.TableNextColumn();
                ImGui.Dummy(new Vector2(5f));

                foreach(var s in P.Config.HideAddonList)
                {
                    ImGui.PushID(s);
                    ImGui.TableNextRow();
                    ImGui.TableNextColumn();
                    ImGuiEx.TextV(focused.Contains(s) ? EColor.Green : null, s);
                    ImGui.TableNextColumn();
                    if(ImGuiEx.IconButton(FontAwesomeIcon.Trash))
                    {
                        new TickScheduler(() => P.Config.HideAddonList.Remove(s));
                    }
                    ImGui.PopID();
                }

                ImGui.EndTable();
            }
        })
        .EndIf()
        .Draw();

        if(P.Config.Hidden.Count > 0)
        {
            new NuiBuilder()
            .Section("隐藏城内水晶")
            .Widget(() =>
            {
                uint toRem = 0;
                foreach(var x in P.Config.Hidden)
                {
                    ImGuiEx.Text($"{Svc.Data.GetExcelSheet<Aetheryte>().GetRow(x)?.AethernetName.Value?.Name.ToString() ?? x.ToString()}");
                    ImGui.SameLine();
                    if(ImGui.SmallButton($"删除##{x}"))
                    {
                        toRem = x;
                    }
                }
                if(toRem > 0)
                {
                    P.Config.Hidden.Remove(toRem);
                }
            })
            .Draw();
        }
    }

    private static void DrawExpert()
    {
        new NuiBuilder()
        .Section("专家设置")
        .Widget(() =>
        {
            ImGui.Checkbox($"减慢城内以太水晶传送速度", ref P.Config.SlowTeleport);
            ImGuiEx.HelpMarker($"将城内以太水晶传送速度减慢指定的量。");
            if(P.Config.SlowTeleport)
            {
                ImGui.Indent();
                ImGui.SetNextItemWidth(200f);
                ImGui.DragInt("传送延迟（毫秒）", ref P.Config.SlowTeleportThrottle);
                ImGui.Unindent();
            }
            ImGuiEx.CheckboxInverted($"跳过直到游戏屏幕准备好的等待", ref P.Config.WaitForScreenReady);
            ImGuiEx.HelpMarker($"启用此选项可以加快传送速度，但要小心，您可能会被卡住。");
            ImGui.Checkbox($"隐藏进度条", ref P.Config.NoProgressBar);
            ImGuiEx.HelpMarker($"隐藏进度条会让您无法阻止 Lifestream 执行其任务。");
            ImGuiEx.CheckboxInverted($"在更远的距离执行跨服命令时不要走到附近的以太之光", ref P.Config.WalkToAetheryte);
        })
        .Draw();
    }
}
