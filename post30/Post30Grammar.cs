using System;

namespace zeldagen.post30
{
    public class Post30Grammar : IMapGrammar
    {
        private readonly Random rng = new Random();

        // Note on use of Size:
        // Size = 10 averages around 40 rooms in the final dungeon
        // Size = 12 averages around 60
        public int Size { get; set; } = 8;

        public int MaxLinearSequenceLength { get; set; } = 8;

        public Map GenerateMap()
        {
            Map map = new Map();

            int tick = 0;
            bool firstLayout = true;

            while (map.Unfinished.TryDequeue(out var next))
            {
                // If the map is too big, just finish it out
                if (tick >= Size)
                {
                    if (next.Type == TemplateType.BonusGoal)
                    {
                        map.Replace(next, map.CreateRoom(ChooseRoom()))
                            .Connect(map.CreateRoom(RoomType.BonusGoal));
                        continue;
                    }

                    if (next.Type != TemplateType.SwitchSeq && next.Type != TemplateType.SwitchLockSeq)
                    {
                        // don't break switch sequences
                        map.Replace(next, map.CreateRoom(ChooseRoom()));
                        continue;
                    }
                }

                switch (next.Type)
                {
                    case TemplateType.DungeonStart:
                        var e = map.CreateTemplate(TemplateType.EntranceChain);
                        map.CreateRoom(RoomType.Entrance).Connect(e);
                        e.Connect(map.CreateRoom(RoomType.Goal));
                        break;
                    case TemplateType.EntranceChain:
                        SetUpEntrance(map, ref tick, next);
                        break;
                    case TemplateType.RoomChooser:
                        map.Replace(next, map.CreateRoom(ChooseRoom()));
                        break;
                    case TemplateType.LayoutChooser:
                        ChooseLayout(map, ref tick, firstLayout, next);
                        firstLayout = false;
                        break;
                    case TemplateType.LinearSequence:
                        if (next.State == 0 || Roll() <= 10)
                        {
                            map.Replace(next, map.CreateTemplate(TemplateType.RoomChooser));
                        }
                        else
                        {
                            var r = map.CreateTemplate(TemplateType.RoomChooser);
                            next.SwapRight(r);
                            next.Connect(r);
                            map.Unfinished.Enqueue(next); // Repeat linear sequence

                            next.State--;
                        }
                        break;
                    case TemplateType.HookSequence:
                        CreateHook(map, next);
                        break;
                    case TemplateType.BonusGoal:
                        map.Replace(
                            next,
                            map.CreateTemplate(TemplateType.LayoutChooser))
                            .Connect(map.CreateRoom(RoomType.BonusGoal));
                        break;
                    case TemplateType.LockChain:
                        AddLockChain(map, ref tick, next);
                        break;
                    case TemplateType.MultiSwitch:
                        AddSwitchedSection(map, next);
                        break;
                    case TemplateType.SwitchSeq:
                        AddSwitch(map, ref tick, next);
                        break;
                    case TemplateType.SwitchLockSeq:
                        AddSwitchLockedPath(map, ref tick, next);
                        break;
                    default:
                        throw new Exception("Unsupported room type " + next.Type);
                }
            }

            return map;
        }

        private void SetUpEntrance(Map map, ref int tick, Template next)
        {
            if (Roll() <= 15) // was 10
            {
                map.Replace(next, map.CreateTemplate(TemplateType.LayoutChooser));
            }
            else
            {
                tick++;
                // e - L - R - L - ()
                // () - E /
                Template backL = map.CreateTemplate(TemplateType.LayoutChooser);
                Template r = map.CreateTemplate(TemplateType.RoomChooser);
                Template frontL = map.CreateTemplate(TemplateType.LayoutChooser);

                //Splice the back L in at the right side of E, then build up e - L - R chain
                next.SwapRight(backL);
                r.Connect(backL);
                frontL.Connect(r);
                map.CreateRoom(RoomType.Entrance).Connect(frontL);
                next.Connect(r);

                map.Unfinished.Enqueue(next); // reuse entrance chain
            }
        }

        private void AddSwitchLockedPath(Map map, ref int tick, Template next)
        {
            if (Roll() <= 10)
            {
                var left = map.CreateTemplate(TemplateType.LayoutChooser);
                var right = map.CreateTemplate(TemplateType.LayoutChooser);

                next.SwapLeft(left);
                left.Connect(right).State = next.State;
                next.SwapRight(right);
            }
            else
            {
                tick++;
                var r = map.CreateTemplate(TemplateType.RoomChooser);
                var left = map.CreateTemplate(TemplateType.LayoutChooser);
                var right = map.CreateTemplate(TemplateType.LayoutChooser);

                next.SwapLeft(r);
                r.Connect(left);
                left.Connect(right).State = next.State;
                next.SwapRight(right);

                //create sub-switch lock branch
                r.Connect(next);
                next.Connect(map.CreateTemplate(TemplateType.BonusGoal));
                map.Unfinished.Enqueue(next); // reuse SWL template
            }
        }

        private void AddSwitch(Map map, ref int tick, Template next)
        {
            if (Roll() <= 10)
            {
                map.Replace(
                    next,
                    map.CreateTemplate(TemplateType.LayoutChooser))
                    .Connect(map.CreateRoom(RoomType.Switch, next.State));
            }
            else
            {
                tick++;
                // R - L - switch
                //   \ SW
                Template r = map.CreateTemplate(TemplateType.RoomChooser);
                Template l = map.CreateTemplate(TemplateType.LayoutChooser);

                next.SwapLeft(r);
                r.Connect(next);
                r.Connect(l);
                l.Connect(map.CreateRoom(RoomType.Switch, next.State));

                map.Unfinished.Enqueue(next); // reuse switch sequence
            }
        }

        private void AddSwitchedSection(Map map, Template next)
        {
            int state = map.Switch();

            var r = map.Replace(next, map.CreateTemplate(TemplateType.RoomChooser));

            foreach (var hall in r.Exit) hall.State = state; // seal the normal exits

            r.Connect(map.CreateTemplate(TemplateType.SwitchSeq, state));

            if (Roll() > 10)
            {
                // Add switch-lock
                var sl = map.CreateTemplate(TemplateType.SwitchLockSeq, state);
                var gb = map.CreateTemplate(TemplateType.BonusGoal);

                r.Connect(sl);
                sl.Connect(gb);
            }
        }

        private void AddLockChain(Map map, ref int tick, Template next)
        {
            if (Roll() <= 10)
            {
                // () - R - L - key
                //        \-(k)- L - ()
                var r = map.CreateTemplate(TemplateType.RoomChooser);
                var keyL = map.CreateTemplate(TemplateType.LayoutChooser);
                var lockedL = map.CreateTemplate(TemplateType.LayoutChooser);

                int key = map.Key();
                next.SwapLeft(r);
                r.Connect(keyL);
                r.Connect(lockedL).Key = key;
                keyL.Connect(map.CreateRoom(RoomType.Key, key));
                next.SwapRight(lockedL);
            }
            else
            {
                tick++;
                // () - r - K - ()
                //       \ - K - GB
                var r = map.CreateTemplate(TemplateType.RoomChooser);
                var newK = map.CreateTemplate(TemplateType.LockChain);
                var gb = map.CreateTemplate(TemplateType.BonusGoal);

                next.SwapLeft(r);
                r.Connect(next);
                r.Connect(newK);
                newK.Connect(gb);

                map.Unfinished.Enqueue(next); // Reuse key sequence
            }
        }

        private void CreateHook(Map map, Template next)
        {
            var r = map.CreateTemplate(TemplateType.RoomChooser);
            map.Replace(next, r);

            //add bonus goal
            var gb = map.CreateTemplate(TemplateType.BonusGoal);
            r.Connect(gb).Secret = (Roll() > 10);
        }

        private void ChooseLayout(Map map, ref int tick, bool firstLayout, Template next)
        {
            int roll = Roll();
            if (firstLayout)
            {
                // There are some boring layouts that can put the goal right at the entrance with no challenge.
                // Try to avoid them here
                if (roll <= 2)
                {
                    // Hook layout puts the exit right through the next room, and puts the whole dungeon behind a secret passage.
                    // put a lock on the dungeon instead
                    roll = 4;
                }
                else if (roll > 8 && roll <= 10)
                {
                    //Linear sequence is also boring - leads straight to the goal
                    // put a switch in the dungeon instead
                    roll = 7;
                }
            }

            switch (roll)
            {
                case 1:
                case 2:
                    map.Replace(next, map.CreateTemplate(TemplateType.HookSequence));
                    break;
                case 3:
                case 4:
                case 5:
                    map.Replace(next, map.CreateTemplate(TemplateType.LockChain));
                    break;
                case 6:
                case 7:
                case 8:
                    map.Replace(next, map.CreateTemplate(TemplateType.MultiSwitch));
                    break;
                case 9:
                case 10:
                    map.Replace(next, map.CreateTemplate(TemplateType.LinearSequence, MaxLinearSequenceLength));
                    break;
                case 11:
                case 12:
                case 13:
                    tick++;
                    var frontL = map.CreateTemplate(TemplateType.LayoutChooser);
                    var r = map.CreateTemplate(TemplateType.RoomChooser);
                    var backL = map.CreateTemplate(TemplateType.LayoutChooser);
                    next.SwapLeft(frontL);
                    frontL.Connect(r);
                    r.Connect(backL);
                    next.SwapRight(backL);
                    break;
                default:
                    tick++;
                    var frontR = map.CreateTemplate(TemplateType.RoomChooser);
                    var backR = map.CreateTemplate(TemplateType.RoomChooser);
                    var topL = map.CreateTemplate(TemplateType.LayoutChooser);
                    var bottomL = map.CreateTemplate(TemplateType.LayoutChooser);

                    next.SwapLeft(frontR);
                    frontR.Connect(topL, roll < 18 ? Direction.Forward : Direction.Both);
                    frontR.Connect(bottomL, roll == 16 || roll == 17 ? Direction.Back : Direction.Both);
                    topL.Connect(backR);
                    bottomL.Connect(backR);
                    next.SwapRight(backR);
                    break;
            }
        }

        private int Roll() => rng.Next(1, 21);

        private RoomType ChooseRoom() => Roll() switch
        {
            int i when i <= 4 => RoomType.Empty,
            int i when i > 4 && i <= 6 => RoomType.Trap,
            int i when i > 6 && i <= 14 => RoomType.Monster,
            int i when i > 14 && i <= 17 => RoomType.Puzzle,
            _ => RoomType.Challenge
        };
    }
}