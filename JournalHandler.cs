using Celeste;
using Celeste.Mod;
using Celeste.Mod.WorldMapHelper;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;


namespace WorldMapHelper
{
    class JournalHandler
    {


        private List<String> JournalOrder;
        private string Levelset;
        private OuiJournal Journal;

        public JournalHandler(OuiJournal journal, string levelSet)
        {
            Levelset = levelSet;
            Journal = journal;
            ReadJournalOrder();
        }

        public void CreatePages()
        {

            List<List<String>> PageContents = new List<List<string>>();
            //int PageIndex = 0;
            foreach (string s in JournalOrder)
            {
                if (s.StartsWith("GROUP:") || s == "---")
                {
                    PageContents.Add(new List<string>());
                }
                PageContents.Last().Add(s);
            }

            UI.CustomJournalPage.ResetTotals(true);

            LevelSetStats levelSetStats = Celeste.SaveData.Instance.LevelSets[0];

            foreach (LevelSetStats lss in Celeste.SaveData.Instance.LevelSets)
            {
                if (lss.Name == Levelset + "/Overworld")
                    levelSetStats = lss;
            }

            UI.CustomJournalPage.AllPagesUnlocked = true;
            foreach (List<String> Page in PageContents)
            {
                Journal.Pages.Add(new UI.CustomJournalPage(Journal, levelSetStats, Page, false));

                if ((Journal.Pages.Last() as UI.CustomJournalPage).IsBlank)
                {
                    Journal.Pages.Remove(Journal.Pages.Last());
                    UI.CustomJournalPage.AllPagesUnlocked = false;
                }
                else
                {
                    (Journal.Pages.Last() as UI.CustomJournalPage).AddSubtotalRow();
                    UI.CustomJournalPage.ResetTotals(false);
                }
            }

            Journal.Pages.Add(new UI.CustomJournalPage(Journal, levelSetStats, null, true));



        }

        private void ReadJournalOrder()
        {
            JournalOrder = new List<string>();
            bool IsRightMod = false;
            foreach (ModAsset check in Everest.Content.Mods.Select(mod => mod.Map.TryGetValue("JournalOrder", out ModAsset check) ? check : null)
                .Where(check => check != null))
            {
                IsRightMod = false;
                foreach (string k in check.Source.Map.Keys)
                {
                    if (k == "Maps/" + WorldMapHelperModule.ModuleInstance.ModName)
                        IsRightMod = true;

                }
                if (IsRightMod)
                {
                    using (StreamReader r = new StreamReader(check.Stream))
                    {
                        while (!r.EndOfStream)
                        {
                            JournalOrder.Add(r.ReadLine());
                        }
                        r.Close();
                        r.Dispose();

                    }
                }
            }




        }

    }
}
