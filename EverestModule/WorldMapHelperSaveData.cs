using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;

namespace Celeste.Mod.WorldMapHelper
{
    public class WorldMapSaveData : EverestModuleSaveData
    {
        public List<String> Exits;
        public List<String> Coords;
        public List<String> JournalEntries;



        public WorldMapSaveData()
        {
            if (Exits is null)
                Exits = new List<string>();
            if (Coords is null)
                Coords = new List<String>();
            if (JournalEntries is null)
                JournalEntries = new List<String>();
        }
        public string GetCurrentRoom(string Levelset)
        {
            foreach (String c in Coords)
            {
                if (c.Split('|')[0] == Levelset)
                {
                    return c.Split('|')[1];
                }

            }
            return "";
        }
        public void SaveCoords(string Levelset, string RoomID, Vector2 coords)
        {
            int IndexFound = -1;
            foreach (String c in Coords)
            {
                if (c.Split('|')[0] == Levelset)
                {
                    IndexFound = Coords.IndexOf(c);
                }

            }

            if (IndexFound == -1)
            {
                Coords.Add(Levelset + "|" + RoomID + "|" + coords.X.ToString() + "|" + coords.Y.ToString());
            }
            else
            {
                Coords[IndexFound] = Levelset + "|" + RoomID + "|" + coords.X.ToString() + "|" + coords.Y.ToString();
            }

        }
        public Vector2 GetCoords(string Levelset)
        {
            foreach (String c in Coords)
            {
                if (c.Split('|')[0] == Levelset)
                {
                    return new Vector2(float.Parse(c.Split('|')[2]), float.Parse(c.Split('|')[3]));
                }

            }
            return Vector2.Zero;
        }
        public bool CheckCoords(string Levelset, string RoomID)
        {
            foreach (String c in Coords)
            {
                if (c.Split('|')[0] == Levelset && c.Split('|')[1] == RoomID)
                {
                    return true;
                }

            }
            return false;

        }
        public void ClearCoords(string Levelset)
        {
            int IndexFound = -1;
            foreach (String c in Coords)
            {
                if (c.Split('|')[0] == Levelset)
                {
                    IndexFound = Coords.IndexOf(c);
                }

            }

            if (IndexFound != -1)
            {
                Coords.RemoveAt(IndexFound);
            }

        }
        public bool SaveExit(string Levelset, string MapID, string ExitName)
        {

            if (!CheckExit(Levelset, ExitName))
            {
                Exits.Add(Levelset + "|" + MapID + "|" + ExitName + "|" + "N");
                return true;
            }
            return false;

        }
        public void UnflagExit(string Levelset, string ExitName)
        {
            int IndexFound = -1;
            foreach (string e in Exits)
            {
                if (e.Split('|')[0] == Levelset && e.Split('|')[2] == ExitName)
                    IndexFound = Exits.IndexOf(e);
            }

            if (IndexFound != -1 && Exits[IndexFound].Split('|')[3] == "N")
                Exits[IndexFound] = Exits[IndexFound].Substring(0, Exits[IndexFound].Length - 1);

        }
        public bool CheckExit(string Levelset, string ExitName)
        {
            foreach (String c in Exits)
            {
                if (c.Split('|')[0] == Levelset && c.Split('|')[2] == ExitName)
                {
                    return true;
                }

            }
            return false;
        }
        public bool IsExitNew(string Levelset, string ExitName)
        {
            foreach (String c in Exits)
            {
                if (c.Split('|')[0] == Levelset && c.Split('|')[2] == ExitName)
                {
                    if (c.Split('|')[3] == "N")
                        return true;
                    else
                        return false;
                }

            }
            return false;
        }

        //2/7 goal: implement exit tracking by level

        public void AddLevelToJournal(string Levelset, string MapID, int Exits)
        {
            foreach (String j in JournalEntries)
                if (j.StartsWith(Levelset + "|" + MapID))
                {
                    if (j == Levelset + "|" + MapID )
                        return;
                    else
                        JournalEntries[JournalEntries.IndexOf(j)] = Levelset + "|" + MapID + "|" + Exits.ToString();
                        return;
                }

            JournalEntries.Add(Levelset + "|" + MapID + "|" + Exits.ToString());
        }

        public bool IsLevelInJournal(string Levelset, string MapID)
        {
            foreach (String j in JournalEntries)
                if (j.StartsWith(Levelset + "|" + MapID))
                    return true;

            return false;
        }

        public int ExitsIn(string Levelset, string MapID, bool Total)
        {
            if (Total)
            {
                foreach (string j in JournalEntries)
                    if (j.StartsWith(Levelset + "|" + MapID))
                        return int.Parse(j.Split('|')[2]);
            }
            else
            {
                int ExitCount = 0;
                foreach (string e in Exits)
                {
                    if (e.Split('|')[0] == Levelset && e.Split('|')[1] == MapID)
                        ExitCount++;
                }
                return ExitCount;

            }


            return -1;
        }

        public int UndiscoveredLevels(string Levelset)
        {
            return 0;
        }




    }




}
