using Dalamud.Bindings.ImGui;
using Dalamud.Interface.Colors;
using Dalamud.Interface.Components;
using Dalamud.Interface.Windowing;
using ECommons.ImGuiMethods;
using FFTriadBuddy;
using FFXIVClientStructs.FFXIV.Client.Game.GoldSaucer;
using PunishLib.ImGuiMethods;
using Saucy.CuffACur;
using Saucy.OtherGames;
using Saucy.TripleTriad;
using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Numerics;
using TriadBuddyPlugin;

namespace Saucy;

public unsafe class PluginUI : Window
{
    public PluginUI() : base("Saucy##Saucy")
    {
        Size = new Vector2(520, 420);
        SizeCondition = ImGuiCond.FirstUseEver;
    }

    public GameNpcInfo CurrentNPC
    {
        get;
        set
        {
            if (field != value)
            {
                TriadAutomater.TempCardsWonList.Clear();
                field = value;
            }
        }
    }

    public bool Enabled { get; set; } = false;

    public override void Draw()
    {
        ImGuiEx.EzTabBar("###游戏",
            ("拳击吉尔加美什", 绘制拳击标签, null, false),
            ("九宫幻卡", 绘制幻卡标签, null, false),
            ("伐木小游戏", () =>
            {
                ImGuiEx.EzTabBar("伐木小游戏",
                    ("主要", P.LimbManager.DrawSettings, null, false),
                    ("调试", P.LimbManager.DrawDebug, null, false));
            }, null, false),
            ("其他游戏", 绘制其他游戏标签, null, false),
            ("统计", 绘制统计标签, null, false),
            ("关于", () => AboutTab.Draw("Saucy"), null, false)
#if DEBUG
            , ("调试", 绘制调试标签, null, false)
#endif
            );
    }

    private void 绘制其他游戏标签()
    {
        if (ImGui.Checkbox("启用快刀一闪模块", ref C.SliceIsRightModuleEnabled))
        {
            if (C.SliceIsRightModuleEnabled)
                C.EnabledModules.Add(ModuleManager.GetModule<SliceIsRight>().InternalName);
            else
                C.EnabledModules.Remove(ModuleManager.GetModule<SliceIsRight>().InternalName);
        }

        if (ImGui.Checkbox("启用自动迷你仙人彩", ref C.EnableAutoMiniCactpot))
        {
            if (C.EnableAutoMiniCactpot)
                C.EnabledModules.Add(ModuleManager.GetModule<MiniCactpot.MiniCactpot>().InternalName);
            else
                C.EnabledModules.Remove(ModuleManager.GetModule<MiniCactpot.MiniCactpot>().InternalName);
        }

        if (ImGui.Checkbox("启用风向模块", ref C.AnyWayTheWindowBlowsModuleEnabled))
        {
            if (C.AnyWayTheWindowBlowsModuleEnabled)
                C.EnabledModules.Add(ModuleManager.GetModule<AnyWayTheWindBlows>().InternalName);
            else
                C.EnabledModules.Remove(ModuleManager.GetModule<AnyWayTheWindBlows>().InternalName);
        }
    }

    private void 绘制统计标签()
    {
        if (ImGui.BeginTabBar("统计"))
        {
            if (ImGui.BeginTabItem("总计"))
            {
                DrawStatsTab(C.Stats, out var reset);

                if (reset)
                {
                    C.Stats = new();
                    C.Save();
                }

                ImGui.EndTabItem();
            }
            if (ImGui.BeginTabItem("本次会话"))
            {
                DrawStatsTab(C.SessionStats, out var reset);
                if (reset)
                    C.SessionStats = new();
                ImGui.EndTabItem();
            }

            ImGui.EndTabBar();
        }
    }

    private void DrawStatsTab(Stats stat, out bool reset)
    {
        if (ImGui.BeginTabBar("游戏"))
        {
            if (ImGui.BeginTabItem("拳击吉尔加美什"))
            {
                DrawCuffStats(stat);
                ImGui.EndTabItem();
            }

            if (ImGui.BeginTabItem("九宫幻卡"))
            {
                DrawTTStats(stat);
                ImGui.EndTabItem();
            }

            if (ImGui.BeginTabItem("伐木小游戏"))
            {
                DrawLimbStats(stat);
                ImGui.EndTabItem();
            }

            ImGui.EndTabBar();
        }

        ImGui.PushItemWidth(ImGui.GetContentRegionAvail().X);
        reset = ImGui.Button("重置统计（按住Ctrl）", new Vector2(ImGui.GetContentRegionAvail().X, ImGui.GetContentRegionAvail().Y)) && ImGui.GetIO().KeyCtrl;
    }

    private void DrawLimbStats(Stats stat)
    {
        ImGui.BeginChild("伐木统计", new Vector2(0, ImGui.GetContentRegionAvail().Y - 30f), true);
        ImGui.Columns(3, default, false);
        ImGui.NextColumn();
        ImGuiEx.CenterColumnText(ImGuiColors.DalamudRed, "伐木小游戏", true);
        ImGuiHelpers.ScaledDummy(10f);
        ImGui.Columns(2, default, false);
        ImGui.NextColumn();
        ImGui.NextColumn();
        ImGuiEx.CenterColumnText("已玩游戏", true);
        ImGui.NextColumn();
        ImGuiEx.CenterColumnText("获得的金蝶币", true);
        ImGui.NextColumn();
        ImGuiEx.CenterColumnText($"{stat.LimbGamesPlayed:N0}");
        ImGui.NextColumn();
        ImGuiEx.CenterColumnText($"{stat.LimbMGP:N0}");

        ImGui.EndChild();
    }

    private void DrawCuffStats(Stats stat)
    {
        ImGui.BeginChild("拳击统计", new Vector2(0, ImGui.GetContentRegionAvail().Y - 30f), true);
        ImGui.Columns(3, default, false);
        ImGui.NextColumn();
        ImGuiEx.CenterColumnText(ImGuiColors.DalamudRed, "拳击吉尔加美什", true);
        ImGuiHelpers.ScaledDummy(10f);
        ImGui.NextColumn();
        ImGui.NextColumn();
        ImGui.NextColumn();
        ImGuiEx.CenterColumnText("已玩游戏", true);
        ImGui.NextColumn();
        ImGui.NextColumn();
        ImGui.NextColumn();
        ImGuiEx.CenterColumnText($"{stat.CuffGamesPlayed:N0}");
        ImGui.NextColumn();
        ImGui.NextColumn();
        ImGui.Spacing();
        ImGuiEx.CenterColumnText("青肿！", true);
        ImGui.NextColumn();
        ImGuiEx.CenterColumnText("惩罚！！", true);
        ImGui.NextColumn();
        ImGuiEx.CenterColumnText("残忍！！！！", true);
        ImGui.NextColumn();
        ImGuiEx.CenterColumnText($"{stat.CuffBruisings:N0}");
        ImGui.NextColumn();
        ImGuiEx.CenterColumnText($"{stat.CuffPunishings:N0}");
        ImGui.NextColumn();
        ImGuiEx.CenterColumnText($"{stat.CuffBrutals:N0}");
        ImGui.NextColumn();
        ImGui.NextColumn();
        ImGuiEx.CenterColumnText("获得的金蝶币", true);
        ImGui.NextColumn();
        ImGui.NextColumn();
        ImGui.NextColumn();
        ImGuiEx.CenterColumnText($"{stat.CuffMGP:N0}");

        ImGui.EndChild();
    }
        private void DrawTTStats(Stats stat)
    {
        ImGui.BeginChild("幻卡统计", new Vector2(0, ImGui.GetContentRegionAvail().Y - 30f), true);
        ImGui.Columns(3, default, false);
        ImGui.NextColumn();
        ImGuiEx.CenterColumnText(ImGuiColors.DalamudRed, "九宫幻卡", true);
        ImGuiHelpers.ScaledDummy(10f);
        ImGui.NextColumn();
        ImGui.NextColumn();
        ImGui.NextColumn();
        ImGuiEx.CenterColumnText("已玩游戏", true);
        ImGui.NextColumn();
        ImGui.NextColumn();
        ImGui.NextColumn();
        ImGuiEx.CenterColumnText($"{stat.GamesPlayedWithSaucy:N0}");
        ImGui.NextColumn();
        ImGui.NextColumn();
        ImGui.Spacing();
        ImGuiEx.CenterColumnText("胜利", true);
        ImGui.NextColumn();
        ImGuiEx.CenterColumnText("失败", true);
        ImGui.NextColumn();
        ImGuiEx.CenterColumnText("平局", true);
        ImGui.NextColumn();
        ImGuiEx.CenterColumnText($"{stat.GamesWonWithSaucy:N0}");
        ImGui.NextColumn();
        ImGuiEx.CenterColumnText($"{stat.GamesLostWithSaucy:N0}");
        ImGui.NextColumn();
        ImGuiEx.CenterColumnText($"{stat.GamesDrawnWithSaucy:N0}");
        ImGui.NextColumn();
        ImGuiEx.CenterColumnText("胜率", true);
        ImGui.NextColumn();
        ImGuiEx.CenterColumnText("获得的卡牌", true);
        ImGui.NextColumn();
        if (stat.NPCsPlayed.Count > 0)
        {
            ImGuiEx.CenterColumnText("最常挑战的NPC", true);
            ImGui.NextColumn();
        }
        else
        {
            ImGui.NextColumn();
        }

        if (stat.GamesPlayedWithSaucy > 0)
        {
            ImGuiEx.CenterColumnText($"{Math.Round((stat.GamesWonWithSaucy / (double)stat.GamesPlayedWithSaucy) * 100, 2)}%");
        }
        else
        {
            ImGuiEx.CenterColumnText("");
        }
        ImGui.NextColumn();
        ImGuiEx.CenterColumnText($"{stat.CardsDroppedWithSaucy:N0}");
        ImGui.NextColumn();

        if (stat.NPCsPlayed.Count > 0)
        {
            ImGuiEx.CenterColumnText($"{stat.NPCsPlayed.OrderByDescending(x => x.Value).First().Key}");
            ImGuiEx.CenterColumnText($"{stat.NPCsPlayed.OrderByDescending(x => x.Value).First().Value:N0} 次");
            ImGui.NextColumn();
            ImGui.NextColumn();
            ImGui.NextColumn();
        }

        ImGui.NextColumn();
        ImGuiEx.CenterColumnText("获得的金蝶币", true);
        ImGui.NextColumn();
        ImGuiEx.CenterColumnText("卡牌掉落总价值", true);
        ImGui.NextColumn();
        if (stat.CardsWon.Count > 0)
        {
            ImGuiEx.CenterColumnText("最常获得的卡牌", true);
        }
        ImGui.NextColumn();
        ImGuiEx.CenterColumnText($"{stat.MGPWon:N0} 金蝶币");
        ImGui.NextColumn();
        ImGuiEx.CenterColumnText($"{GetDroppedCardValues(stat):N0} 金蝶币");
        ImGui.NextColumn();
        if (stat.CardsWon.Count > 0)
        {
            ImGuiEx.CenterColumnText($"{TriadCardDB.Get().FindById((int)stat.CardsWon.OrderByDescending(x => x.Value).First().Key).Name.GetLocalized()}");
            ImGui.NextColumn();
            ImGui.NextColumn();
            ImGui.NextColumn();
            ImGuiEx.CenterColumnText($"获得 {stat.CardsWon.OrderByDescending(x => x.Value).First().Value:N0} 次");
        }

        ImGui.Columns(1);
        ImGui.EndChild();
    }

    private int GetDroppedCardValues(Stats stat)
    {
        var output = 0;
        foreach (var card in stat.CardsWon)
            output += GameCardDB.Get().FindById((int)card.Key).SaleValue * stat.CardsWon[card.Key];

        return output;
    }

    public void 绘制幻卡标签()
    {
        var enabled = TriadAutomater.ModuleEnabled;

        ImGui.TextWrapped(@"使用方法：向想要挑战的NPC发起幻卡对战。一旦开始挑战，点击'启用幻卡模块'。");
        ImGui.Separator();

        if (ImGui.Checkbox("启用幻卡模块", ref enabled))
        {
            TriadAutomater.ModuleEnabled = enabled;

            if (enabled)
                CufModule.ModuleEnabled = false;
        }

        var autoOpen = C.OpenAutomatically;

        if (ImGui.Checkbox("挑战NPC时自动打开Saucy", ref autoOpen))
        {
            C.OpenAutomatically = autoOpen;
            C.Save();
        }

        var selectedDeck = C.SelectedDeckIndex;

        if (Saucy.TTSolver.profileGS.GetPlayerDecks().Count() > 0)
        {
            var useAutoDeck = C.UseRecommendedDeck;
            if (ImGui.Checkbox("自动选择胜率最高的卡组", ref useAutoDeck))
            {
                C.UseRecommendedDeck = useAutoDeck;
                C.Save();
            }

            if (!C.UseRecommendedDeck)
            {
                ImGui.PushItemWidth(200);
                string preview;
                if (selectedDeck == -1 || Saucy.TTSolver.profileGS.GetPlayerDecks()[selectedDeck] is null)
                {
                    preview = "";
                }
                else
                {
                    preview = selectedDeck >= 0
                        ? Saucy.TTSolver.profileGS.GetPlayerDecks()[selectedDeck].name
                        : string.Empty;
                }

                if (ImGui.BeginCombo("选择卡组", preview))
                {
                    if (ImGui.Selectable(""))
                    {
                        C.SelectedDeckIndex = -1;
                    }

                    foreach (var deck in Saucy.TTSolver.profileGS.GetPlayerDecks())
                    {
                        if (deck is null) continue;
                        var index = deck.id;
                        if (ImGui.Selectable(deck.name, index == selectedDeck))
                        {
                            C.SelectedDeckIndex = index;
                            C.Save();
                        }
                    }

                    ImGui.EndCombo();
                }
            }
            else
            {
                ImGui.TextWrapped("请先向NPC发起挑战以加载您的卡组列表。");
            }

            if (ImGui.Checkbox("玩指定次数", ref TriadAutomater.PlayXTimes) && (TriadAutomater.NumberOfTimes <= 0 ||
                                                                           TriadAutomater.PlayUntilCardDrops ||
                                                                           TriadAutomater.PlayUntilAllCardsDropOnce))
            {
                TriadAutomater.NumberOfTimes = 1;
                TriadAutomater.PlayUntilCardDrops = false;
                TriadAutomater.PlayUntilAllCardsDropOnce = false;
            }

            if (ImGui.Checkbox("玩到有卡牌掉落为止", ref TriadAutomater.PlayUntilCardDrops) &&
                (TriadAutomater.NumberOfTimes <= 0 || TriadAutomater.PlayXTimes ||
                 TriadAutomater.PlayUntilAllCardsDropOnce))
            {
                TriadAutomater.NumberOfTimes = 1;
                TriadAutomater.PlayXTimes = false;
                TriadAutomater.PlayUntilAllCardsDropOnce = false;
            }

            if (GameNpcDB.Get().mapNpcs.TryGetValue(Saucy.TTSolver.preGameNpc?.Id ?? -1, out var npcInfo))
            {
                CurrentNPC = npcInfo;
            }
            else
            {
                CurrentNPC = null;
            }

            if (ImGui.Checkbox(
                    $"玩到NPC所有卡牌都掉落至少X次 {(CurrentNPC is null ? "" : $"({TriadNpcDB.Get().FindByID(CurrentNPC.npcId).Name.GetLocalized()})")}",
                    ref TriadAutomater.PlayUntilAllCardsDropOnce))
            {
                TriadAutomater.TempCardsWonList.Clear();
                TriadAutomater.PlayUntilCardDrops = false;
                TriadAutomater.PlayXTimes = false;
                TriadAutomater.NumberOfTimes = 1;
            }

            var onlyUnobtained = C.OnlyUnobtainedCards;

            if (TriadAutomater.PlayUntilAllCardsDropOnce)
            {
                ImGui.SameLine();
                if (ImGui.Checkbox("仅未获得的卡牌", ref onlyUnobtained))
                {
                    TriadAutomater.TempCardsWonList.Clear();
                    C.OnlyUnobtainedCards = onlyUnobtained;
                    C.Save();
                }
            }

if (TriadAutomater.PlayUntilAllCardsDropOnce && CurrentNPC != null)
{
    ImGui.Indent();
    GameCardDB.Get().Refresh();
    foreach (var card in CurrentNPC.rewardCards)
    {
        if ((C.OnlyUnobtainedCards && !GameCardDB.Get().FindById(card).IsOwned) || !C.OnlyUnobtainedCards)
        {
            TriadAutomater.TempCardsWonList.TryAdd((uint)card, 0);
            ImGui.Text(
                $"- {TriadCardDB.Get().FindById(GameCardDB.Get().FindById(card).CardId).Name.GetLocalized()} {TriadAutomater.TempCardsWonList[(uint)card]}/{TriadAutomater.NumberOfTimes}");
        }
    }

    if (C.OnlyUnobtainedCards && TriadAutomater.TempCardsWonList.Count == 0)
    {
        ImGui.PushStyleColor(ImGuiCol.Text, ImGuiColors.DalamudRed);
        ImGui.TextWrapped(
            $@"你已经获得了该NPC的所有卡牌。除非取消勾选'仅限未拥有卡牌'或选择其他NPC，否则此功能将无法工作。");
        ImGui.PopStyleColor();
    }

    ImGui.Unindent();
}

if (TriadAutomater.PlayXTimes || TriadAutomater.PlayUntilCardDrops ||
    TriadAutomater.PlayUntilAllCardsDropOnce)
{
    ImGui.PushItemWidth(150f);
    ImGui.Text("重复次数：");
    ImGui.SameLine();

    if (ImGui.InputInt("###NumberOfTimes", ref TriadAutomater.NumberOfTimes))
    {
        if (TriadAutomater.NumberOfTimes <= 0)
            TriadAutomater.NumberOfTimes = 1;
    }

    ImGui.Checkbox("完成后退出游戏", ref TriadAutomater.LogOutAfterCompletion);

    var playSound = C.PlaySound;

    ImGui.Columns(2, default, false);
    if (ImGui.Checkbox("完成时播放声音", ref playSound))
    {
        C.PlaySound = playSound;
        C.Save();
    }

    if (playSound)
    {
        ImGui.NextColumn();
        ImGui.Text("选择声音");
        if (ImGui.BeginCombo("###SelectSound", C.SelectedSound))
        {
            var path = Path.Combine(Svc.PluginInterface.AssemblyLocation.Directory.FullName, "Sounds");
            foreach (var file in new DirectoryInfo(path).GetFiles())
            {
                if (ImGui.Selectable($"{Path.GetFileNameWithoutExtension(file.FullName)}",
                        C.SelectedSound == Path.GetFileNameWithoutExtension(file.FullName)))
                {
                    C.SelectedSound = Path.GetFileNameWithoutExtension(file.FullName);
                    C.Save();
                }
            }

            ImGui.EndCombo();
        }

        if (ImGui.Button("打开声音文件夹"))
        {
            Process.Start("explorer.exe",
                @$"{Path.Combine(Svc.PluginInterface.AssemblyLocation.Directory.FullName, "Sounds")}");
        }

        ImGuiComponents.HelpMarker(
            "将MP3文件放入声音文件夹以添加自定义声音。");
    }

    ImGui.Columns(1);
}
        }
    }

    public unsafe void 绘制拳击标签()
    {
        var enabled = CufModule.ModuleEnabled;

        ImGui.TextWrapped(@"使用方法：点击'启用拳击模块'然后走到拳击宠物机器前。");
        ImGui.Separator();

        if (ImGui.Checkbox("启用拳击模块", ref enabled))
        {
            CufModule.ModuleEnabled = enabled;
            if (enabled && TriadAutomater.ModuleEnabled)
                TriadAutomater.ModuleEnabled = false;
        }

        if (ImGui.Checkbox("重复游戏次数", ref TriadAutomater.PlayXTimes) && TriadAutomater.NumberOfTimes <= 0)
        {
            TriadAutomater.NumberOfTimes = 1;
        }

        if (TriadAutomater.PlayXTimes)
        {
            ImGui.PushItemWidth(150f);
            ImGui.Text("重复次数：");
            ImGui.SameLine();

            if (ImGui.InputInt("###NumberOfTimes", ref TriadAutomater.NumberOfTimes))
            {
                if (TriadAutomater.NumberOfTimes <= 0)
                    TriadAutomater.NumberOfTimes = 1;
            }

            ImGui.Checkbox("完成后退出游戏", ref TriadAutomater.LogOutAfterCompletion);

            var playSound = C.PlaySound;

            ImGui.Columns(2, default, false);
            if (ImGui.Checkbox("完成时播放声音", ref playSound))
            {
                C.PlaySound = playSound;
                C.Save();
            }

            if (playSound)
            {
                ImGui.NextColumn();
                ImGui.Text("选择声音");
                if (ImGui.BeginCombo("###SelectSound", C.SelectedSound))
                {
                    var path = Path.Combine(Svc.PluginInterface.AssemblyLocation.Directory.FullName, "Sounds");
                    foreach (var file in new DirectoryInfo(path).GetFiles())
                    {
                        if (ImGui.Selectable($"{Path.GetFileNameWithoutExtension(file.FullName)}", C.SelectedSound == Path.GetFileNameWithoutExtension(file.FullName)))
                        {
                            C.SelectedSound = Path.GetFileNameWithoutExtension(file.FullName);
                            C.Save();
                        }
                    }

                    ImGui.EndCombo();
                }

                if (ImGui.Button("打开声音文件夹"))
                {
                    Process.Start("explorer.exe", @$"{Path.Combine(Svc.PluginInterface.AssemblyLocation.Directory.FullName, "Sounds")}");
                }
                ImGuiComponents.HelpMarker("将MP3文件放入声音文件夹以添加自定义声音。");
            }
            ImGui.Columns(1);
        }
    }

    private void 绘制调试标签()
    {
        if (GoldSaucerManager.Instance() != null && GoldSaucerManager.Instance()->CurrentGFateDirector != null)
        {
            var dir = GoldSaucerManager.Instance()->CurrentGFateDirector;
            ImGui.Text($"关卡类型: {dir->GateType}");
            ImGui.Text($"关卡位置类型: {dir->GatePositionType}");
            ImGui.Text($"标志: {dir->Flags}");
            ImGui.Text($"是否正在运行关卡: {dir->IsRunningGate()}");
            ImGui.Text($"是否接受关卡: {dir->IsAcceptingGate()}");
        }
    }
}
