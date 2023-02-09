using Celeste;
using Celeste.Mod.WorldMapHelper;
using Microsoft.Xna.Framework;
using Monocle;
using System;
using System.Collections.Generic;
using System.Linq;
namespace WorldMapHelper.UI
{


    class CustomJournalPage : OuiJournalPage
    {
        static int RunningTotalDeaths;
        static int RunningTotalStrawberries;
        static int RunningTotalMaxStrawberries;
        static int RunningTotalExits;
        static int RunningTotalMaxExits;
        static long RunningTotalTime;
        static int RunningTotalDeathPB;
        static long RunningTotalTimePB;

        static int GrandTotalDeaths;
        static int GrandTotalStrawberries;
        static int GrandTotalMaxStrawberries;
        static int GrandTotalExits;
        static int GrandTotalMaxExits;
        static long GrandTotalTime;
        static int GrandTotalDeathPB;
        static long GrandTotalTimePB;

        static string RunningTotalGroupTitle;
        public static bool AllPagesUnlocked;

        private Table pageTable;
        private List<String> pageContents;
        private LevelSetStats levelSetStats;

        private string Levelset;
        private static List<SubtotalLine> subtotals;

        public bool IsBlank = true;
        public bool IsComplete = true;
        public static OuiJournal JournalInstance;
        public CustomJournalPage(OuiJournal journal, LevelSetStats levelSetStats, List<String> PageContents, bool FinalPage) : base(journal)
        {

            JournalInstance = journal;
            pageContents = PageContents;
            this.levelSetStats = levelSetStats;
            PageTexture = "page";

            if (FinalPage)
            {
                bool ShowGrandTotals = true;

                pageTable = new Table()
                    .AddColumn(new TextCell(Dialog.Clean("worldmaphelper_totals"), new Vector2(0f, 0.5f), 1f, Color.Black * 0.7f, 420f))
                    .AddColumn(new IconCell("clear", 150f))
                    .AddColumn(new IconCell("strawberry", 150f))
                    .AddColumn(new IconCell("skullblue", 100f))
                    .AddColumn(new IconCell("time", 220f))
                    .AddColumn(new IconCell("CollabUtils2MinDeaths/SpringCollab2020/1-Beginner", 100f))
                    .AddColumn(new IconCell("CollabUtils2/speed_berry_pbs_heading", 220f));

                foreach (SubtotalLine st in subtotals)
                {
                    if (st.Complete)
                    {
                        pageTable.AddRow()
                            .Add(new TextCell(st.GroupTitle, new Vector2(0, 0.5f), 0.6f, Color.Black * 0.7f))
                            .Add(new TextCell(st.Exits.ToString() + "/" + st.MaxExits.ToString(), new Vector2(0.5f, 0.5f), 0.6f, Color.Black * 0.7f))
                            .Add(new TextCell(st.Strawberries.ToString() + "/" + st.MaxStrawberries.ToString(), new Vector2(0.5f, 0.5f), 0.6f, Color.Black * 0.7f))
                            .Add(new TextCell(st.Deaths.ToString(), new Vector2(0.5f, 0.5f), 0.6f, Color.Black * 0.7f))
                            .Add(new TextCell(CleanTimeSpan(new TimeSpan(st.Time)), new Vector2(0.5f, 0.5f), 0.6f, Color.Black * 0.7f))
                            .Add(new TextCell(st.DeathPB.ToString(), new Vector2(0.5f, 0.5f), 0.6f, Color.Black * 0.7f))
                            .Add(new TextCell(CleanTimeSpan(new TimeSpan(st.TimePB)), new Vector2(0.5f, 0.5f), 0.6f, Color.Black * 0.7f));
                    }
                    else
                    {
                        pageTable.AddRow()
                             .Add(new TextCell(st.GroupTitle, new Vector2(0, 0.5f), 0.6f, Color.Black * 0.7f))
                             .Add(new TextCell(st.Exits.ToString(), new Vector2(0.5f, 0.5f), 0.6f, Color.Black * 0.7f))
                             .Add(new TextCell(st.Strawberries.ToString(), new Vector2(0.5f, 0.5f), 0.6f, Color.Black * 0.7f))
                             .Add(new TextCell(st.Deaths.ToString(), new Vector2(0.5f, 0.5f), 0.6f, Color.Black * 0.7f))
                             .Add(new TextCell(CleanTimeSpan(new TimeSpan(st.Time)), new Vector2(0.5f, 0.5f), 0.6f, Color.Black * 0.7f))
                             .Add(new IconCell("dot"))
                             .Add(new IconCell("dot"));
                        ShowGrandTotals = false;
                    }
                }
                pageTable.AddRow();
                if (ShowGrandTotals && AllPagesUnlocked)
                {

                    pageTable.AddRow()
                        .Add(new TextCell(Dialog.Clean("worldmaphelper_grandtotal"), new Vector2(1f, 0.5f), 0.7f, Color.Black * 0.7f))
                        .Add(new TextCell(GrandTotalExits.ToString() + "/" + GrandTotalMaxExits.ToString(), new Vector2(0.5f, 0.5f), 0.7f, Color.Black * 0.7f))
                        .Add(new TextCell(GrandTotalStrawberries.ToString() + "/" + GrandTotalMaxStrawberries.ToString(), new Vector2(0.5f, 0.5f), 0.7f, Color.Black * 0.7f))
                        .Add(new TextCell(GrandTotalDeaths.ToString(), new Vector2(0.5f, 0.5f), 0.7f, Color.Black * 0.7f))
                        .Add(new TextCell(CleanTimeSpan(new TimeSpan(GrandTotalTime)), new Vector2(0.5f, 0.5f), 0.7f, Color.Black * 0.7f))
                        .Add(new TextCell(GrandTotalDeathPB.ToString(), new Vector2(0.5f, 0.5f), 0.7f, Color.Black * 0.7f))
                        .Add(new TextCell(CleanTimeSpan(new TimeSpan(GrandTotalTimePB)), new Vector2(0.5f, 0.5f), 0.7f, Color.Black * 0.7f))
                        ;

                }
                else
                {
                    pageTable.AddRow()
                        .Add(new TextCell(Dialog.Clean("worldmaphelper_grandtotal"), new Vector2(1f, 0.5f), 0.7f, Color.Black * 0.7f))
                        .Add(new TextCell(GrandTotalExits.ToString(), new Vector2(0.5f, 0.5f), 0.7f, Color.Black * 0.7f))
                        .Add(new TextCell(GrandTotalStrawberries.ToString(), new Vector2(0.5f, 0.5f), 0.7f, Color.Black * 0.7f))
                        .Add(new TextCell(GrandTotalDeaths.ToString(), new Vector2(0.5f, 0.5f), 0.7f, Color.Black * 0.7f))
                        .Add(new TextCell(CleanTimeSpan(new TimeSpan(GrandTotalTime)), new Vector2(0.5f, 0.5f), 0.7f, Color.Black * 0.7f))
                        .Add(new IconCell("dot"))
                        .Add(new IconCell("dot"));
                }

                pageTable.AddRow();


            }
            else
            {
                RunningTotalGroupTitle = PageContents.First().Replace("GROUP:", "");
                pageTable = new Table()
                    .AddColumn(new TextCell(RunningTotalGroupTitle, new Vector2(0f, 0.5f), 1f, Color.Black * 0.7f, 420f))
                    .AddColumn(new IconCell("clear", 150f))
                    .AddColumn(new IconCell("strawberry", 150f))
                    .AddColumn(new IconCell("skullblue", 100f))
                    .AddColumn(new IconCell("time", 220f))
                    .AddColumn(new IconCell("CollabUtils2MinDeaths/SpringCollab2020/1-Beginner", 100f))
                    .AddColumn(new IconCell("CollabUtils2/speed_berry_pbs_heading", 220f))
                    ;

                Levelset = levelSetStats.Name;
                PageContents.RemoveAt(0);

                foreach (string s in PageContents)
                {
                    foreach (AreaStats a in this.levelSetStats.Areas)
                    {
                        if (a.SID == Levelset + "/" + s)
                            AddLevelRow(a);
                    }
                }

            }

        }



        private void AddLevelRow(AreaStats area)
        {
            if (WorldMapHelperModule.SaveData.IsLevelInJournal(Levelset.Replace("/Overworld", ""), area.SID.Split('/').Last()) || Celeste.SaveData.Instance.CheatMode)
            {
                IsBlank = false;

                if (WorldMapHelperModule.SaveData.ExitsIn(Levelset.Replace("/Overworld", ""), AreaData.Get(area).Name.Split('/').Last(), false) >= 1 || Celeste.SaveData.Instance.CheatMode)
                {
                    pageTable.AddRow()
                        //level name
                        .Add(new TextCell(Dialog.Clean(area.SID), new Vector2(0, 0.5f), 0.6f, Color.Black * 0.7f))
                        //exit count
                        .Add(new TextCell(WorldMapHelperModule.SaveData.ExitsIn(Levelset.Replace("/Overworld", ""), AreaData.Get(area).Name.Split('/').Last(), false).ToString()
                        + "/" + WorldMapHelperModule.SaveData.ExitsIn(Levelset.Replace("/Overworld", ""), AreaData.Get(area).Name.Split('/').Last(), true).ToString()
                        , new Vector2(0.5f, 0.5f), 0.6f, Color.Black * 0.7f))
                        //berry count
                        .Add(new TextCell(area.TotalStrawberries.ToString() + "/" + AreaData.Get(area).Mode[0].TotalStrawberries.ToString(), new Vector2(0.5f, 0.5f), 0.6f, Color.Black * 0.7f))
                        //deaths
                        .Add(new TextCell(area.TotalDeaths.ToString(), new Vector2(0.5f, 0.5f), 0.6f, Color.Black * 0.7f))
                        //time
                        .Add(new TextCell(CleanTimeSpan(new TimeSpan(area.TotalTimePlayed)), new Vector2(0.5f, 0.5f), 0.6f, Color.Black * 0.7f))
                        //fewest deaths
                        .Add(new TextCell(area.BestTotalDeaths.ToString(), new Vector2(0.5f, 0.5f), 0.6f, Color.Black * 0.7f))
                        //best time
                        .Add(new TextCell(CleanTimeSpan(new TimeSpan(area.BestTotalTime)), new Vector2(0.5f, 0.5f), 0.6f, Color.Black * 0.7f))
                        ;
                }
                else
                {
                    pageTable.AddRow()
                    //level name
                    .Add(new TextCell(Dialog.Clean(area.SID), new Vector2(0, 0.5f), 0.6f, Color.Black * 0.7f))
                    //exit count
                    .Add(new TextCell(WorldMapHelperModule.SaveData.ExitsIn(Levelset.Replace("/Overworld", ""), AreaData.Get(area).Name.Split('/').Last(), false).ToString(), new Vector2(0.5f, 0.5f), 0.6f, Color.Black * 0.7f))
                    //berry count
                    .Add(new TextCell(area.TotalStrawberries.ToString(), new Vector2(0.5f, 0.5f), 0.6f, Color.Black * 0.7f))
                    //deaths
                    .Add(new TextCell(area.TotalDeaths.ToString(), new Vector2(0.5f, 0.5f), 0.6f, Color.Black * 0.7f))
                    //time
                    .Add(new TextCell(CleanTimeSpan(new TimeSpan(area.TotalTimePlayed)), new Vector2(0.5f, 0.5f), 0.6f, Color.Black * 0.7f))
                    .Add(new IconCell("dot"))
                    .Add(new IconCell("dot"));
                    ;

                    IsComplete = false;
                }


                RunningTotalDeaths += area.TotalDeaths;
                RunningTotalStrawberries += area.TotalStrawberries;
                RunningTotalMaxStrawberries += AreaData.Get(area).Mode[0].TotalStrawberries;
                RunningTotalTime += area.TotalTimePlayed;
                RunningTotalExits += WorldMapHelperModule.SaveData.ExitsIn(Levelset.Replace("/Overworld", ""), AreaData.Get(area).Name.Split('/').Last(), false);
                RunningTotalMaxExits += WorldMapHelperModule.SaveData.ExitsIn(Levelset.Replace("/Overworld", ""), AreaData.Get(area).Name.Split('/').Last(), true);
                RunningTotalDeathPB += area.BestTotalDeaths;
                RunningTotalTimePB += area.BestTotalTime;

                GrandTotalDeaths += area.TotalDeaths;
                GrandTotalStrawberries += area.TotalStrawberries;
                GrandTotalMaxStrawberries += AreaData.Get(area).Mode[0].TotalStrawberries;
                GrandTotalTime += area.TotalTimePlayed;
                GrandTotalExits += WorldMapHelperModule.SaveData.ExitsIn(Levelset.Replace("/Overworld", ""), AreaData.Get(area).Name.Split('/').Last(), false);
                GrandTotalMaxExits += WorldMapHelperModule.SaveData.ExitsIn(Levelset.Replace("/Overworld", ""), AreaData.Get(area).Name.Split('/').Last(), true);
                GrandTotalDeathPB += area.BestTotalDeaths;
                GrandTotalTimePB += area.BestTotalTime;
            }
            else
                IsComplete = false;
        }

        public void AddSubtotalRow()
        {
            pageTable.AddRow();

            if (IsComplete)
            {
                pageTable.AddRow()
                    .Add(new TextCell(Dialog.Clean("worldmaphelper_subtotal"), new Vector2(1f, 0.5f), 0.7f, Color.Black * 0.7f))
                    .Add(new TextCell(RunningTotalExits.ToString() + "/" + RunningTotalMaxExits.ToString(), new Vector2(0.5f, 0.5f), 0.7f, Color.Black * 0.7f))
                    .Add(new TextCell(RunningTotalStrawberries.ToString() + "/" + RunningTotalMaxStrawberries.ToString(), new Vector2(0.5f, 0.5f), 0.7f, Color.Black * 0.7f))
                    .Add(new TextCell(RunningTotalDeaths.ToString(), new Vector2(0.5f, 0.5f), 0.7f, Color.Black * 0.7f))
                    .Add(new TextCell(CleanTimeSpan(new TimeSpan(RunningTotalTime)), new Vector2(0.5f, 0.5f), 0.7f, Color.Black * 0.7f))
                    .Add(new TextCell(RunningTotalDeathPB.ToString(), new Vector2(0.5f, 0.5f), 0.7f, Color.Black * 0.7f))
                    .Add(new TextCell(CleanTimeSpan(new TimeSpan(RunningTotalTimePB)), new Vector2(0.5f, 0.5f), 0.7f, Color.Black * 0.7f));
            }
            else
            {
                pageTable.AddRow()
                    .Add(new TextCell(Dialog.Clean("worldmaphelper_subtotal"), new Vector2(1f, 0.5f), 0.7f, Color.Black * 0.7f))
                    .Add(new TextCell(RunningTotalExits.ToString(), new Vector2(0.5f, 0.5f), 0.7f, Color.Black * 0.7f))
                    .Add(new TextCell(RunningTotalStrawberries.ToString(), new Vector2(0.5f, 0.5f), 0.7f, Color.Black * 0.7f))
                    .Add(new TextCell(RunningTotalDeaths.ToString(), new Vector2(0.5f, 0.5f), 0.7f, Color.Black * 0.7f))
                    .Add(new TextCell(CleanTimeSpan(new TimeSpan(RunningTotalTime)), new Vector2(0.5f, 0.5f), 0.7f, Color.Black * 0.7f))
                    .Add(new IconCell("dot"))
                    .Add(new IconCell("dot"));

            }
            pageTable.AddRow();



        }

        public static void ResetTotals(bool FullReset = false)
        {
            //either resets and archives subtotals, or hard-resets all static data.

            if (subtotals == null)
                subtotals = new List<SubtotalLine>();

            if (FullReset)
            {
                GrandTotalDeaths = 0;
                GrandTotalStrawberries = 0;
                GrandTotalMaxStrawberries = 0;
                GrandTotalTime = 0;
                GrandTotalExits = 0;
                GrandTotalMaxExits = 0;
                GrandTotalDeathPB = 0;
                GrandTotalTimePB = 0;
                RunningTotalGroupTitle = null;
                subtotals.Clear();
            }
            else
            {
                SubtotalLine newSub = new SubtotalLine();
                newSub.GroupTitle = RunningTotalGroupTitle;
                newSub.Deaths = RunningTotalDeaths;
                newSub.Strawberries = RunningTotalStrawberries;
                newSub.MaxStrawberries = RunningTotalMaxStrawberries;
                newSub.Time = RunningTotalTime;
                newSub.Exits = RunningTotalExits;
                newSub.MaxExits = RunningTotalMaxExits;
                newSub.DeathPB = RunningTotalDeathPB;
                newSub.TimePB = RunningTotalTimePB;
                newSub.Complete = (JournalInstance.Pages.Last() as CustomJournalPage).IsComplete;

                subtotals.Add(newSub);

            }
            RunningTotalDeaths = 0;
            RunningTotalStrawberries = 0;
            RunningTotalMaxStrawberries = 0;
            RunningTotalTime = 0;
            RunningTotalExits = 0;
            RunningTotalMaxExits = 0;
            RunningTotalDeathPB = 0;
            RunningTotalTimePB = 0;


        }

        private string CleanTimeSpan(TimeSpan timeSpan)
        {
            string RetVal = timeSpan.ToString();

            if (timeSpan.TotalHours < 10)
                RetVal = RetVal.Substring(1);

            if (timeSpan.TotalHours < 1)
                RetVal = RetVal.Substring(2);

            if (timeSpan.TotalMinutes < 10)
                RetVal = RetVal.Substring(1);

            //RetVal = RetVal.TrimEnd("00000");
            RetVal = RetVal.Substring(0, RetVal.Length - 4);


            return RetVal;
        }



        public override void Redraw(VirtualRenderTarget buffer)
        {
            base.Redraw(buffer);
            Draw.SpriteBatch.Begin();
            pageTable.Render(new Vector2(60f, 20f));
            Draw.SpriteBatch.End();
        }

    }

    public struct SubtotalLine
    {
        public string GroupTitle;
        public int Deaths;
        public int Strawberries;
        public int MaxStrawberries;
        public long Time;
        public int Exits;
        public int MaxExits;
        public int DeathPB;
        public long TimePB;
        public bool Complete;
    }


}
