using System;

namespace zeldagen.post30
{
    public class Post30Grammar : IMapGrammar<Template, Room>
    {
        private readonly Random rng = new Random();

        // Note on use of Size:
        // Size = 10 averages around 40 rooms in the final dungeon
        // Size = 12 averages around 60
        public int Size { get; set; } = 8;

        public int MaxLinearSequenceLength { get; set; } = 8;

        public IRoomClassifier<Room> Classifier => new Post30Classifier();

        public Map<Template, Room> GenerateMap()
        {
            // Track identifiers for key
            Skid switches = new();
            Skid keys = new();

            var map = new Map<Template, Room>((l, r) => l.Kind == r.Kind && l.Exit.Count == 1 && r.Entrance.Count == 1 ? Reduction.MergeRightToLeft : Reduction.Keep);
            map.CreateTemplate(TemplateType.DungeonStart);

            int tick = 0;
            bool firstLayout = true;

            foreach (var currentNode in map.RemainingTemplates())
            {
                // If the map is too big, just finish it out
                if (tick >= Size)
                {
                    if (currentNode.Type == TemplateType.BonusGoal)
                    {
                        currentNode.ReplaceWith(map.CreateRoom(ChooseRoom()))
                            .ConnectTo(map.CreateRoom(RoomType.BonusGoal));
                        continue;
                    }

                    if (currentNode.Type != TemplateType.SwitchSeq && currentNode.Type != TemplateType.SwitchLockSeq)
                    {
                        // don't break switch sequences
                        currentNode.ReplaceWith(map.CreateRoom(ChooseRoom()));
                        continue;
                    }
                }

                switch (currentNode.Type)
                {
                    case TemplateType.DungeonStart:
                        var e = map.CreateTemplate(TemplateType.EntranceChain);
                        map.CreateRoom(RoomType.Entrance).ConnectTo(e);
                        e.ConnectTo(map.CreateRoom(RoomType.Goal));
                        break;
                    case TemplateType.EntranceChain:
                        MapEntrance(map, ref tick, currentNode);
                        break;
                    case TemplateType.RoomChooser:
                        currentNode.ReplaceWith(map.CreateRoom(ChooseRoom()));
                        break;
                    case TemplateType.LayoutChooser:
                        ChooseLayout(map, ref tick, firstLayout, currentNode);
                        firstLayout = false;
                        break;
                    case TemplateType.LinearSequence:
                        if (currentNode.State == 0 || Roll() <= 10)
                        {
                            currentNode.ReplaceWith(map.CreateTemplate(TemplateType.RoomChooser));
                        }
                        else
                        {
                            var r = map.CreateTemplate(TemplateType.RoomChooser);
                            currentNode.SwapRight(r);
                            currentNode.ConnectTo(r);
                            map.Track(currentNode); // Repeat linear sequence

                            currentNode.State--;
                        }
                        break;
                    case TemplateType.HookSequence:
                        CreateHook(map, currentNode);
                        break;
                    case TemplateType.BonusGoal:
                        currentNode.ReplaceWith(map.CreateTemplate(TemplateType.LayoutChooser))
                            .ConnectTo(map.CreateRoom(RoomType.BonusGoal));
                        break;
                    case TemplateType.LockChain:
                        AddLockChain(map, ref tick, currentNode, keys);
                        break;
                    case TemplateType.MultiSwitch:
                        AddSwitchedSection(map, currentNode, switches);
                        break;
                    case TemplateType.SwitchSeq:
                        AddSwitch(map, ref tick, currentNode);
                        break;
                    case TemplateType.SwitchLockSeq:
                        AddSwitchLockedPath(map, ref tick, currentNode);
                        break;
                    default:
                        throw new Exception("Unsupported room type " + currentNode.Type);
                }
            }

            return map;
        }

        private void MapEntrance(Map<Template, Room> map, ref int tick, Template current)
        {
            if (Roll() <= 15) // was 10
            {
                current.ReplaceWith(map.CreateTemplate(TemplateType.LayoutChooser));
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
                current.SwapRight(backL);
                r.ConnectTo(backL);
                frontL.ConnectTo(r);
                map.CreateRoom(RoomType.Entrance).ConnectTo(frontL);
                current.ConnectTo(r);

                map.Track(current); // reuse entrance chain
            }
        }

        private void AddSwitchLockedPath(Map<Template, Room> map, ref int tick, Template current)
        {
            if (Roll() <= 10)
            {
                var left = map.CreateTemplate(TemplateType.LayoutChooser);
                var right = map.CreateTemplate(TemplateType.LayoutChooser);

                current.SwapLeft(left);
                left.ConnectTo(right).Lock = new Switch(current.State);
                current.SwapRight(right);
            }
            else
            {
                tick++;
                var r = map.CreateTemplate(TemplateType.RoomChooser);
                var left = map.CreateTemplate(TemplateType.LayoutChooser);
                var right = map.CreateTemplate(TemplateType.LayoutChooser);

                current.SwapLeft(r);
                r.ConnectTo(left);
                left.ConnectTo(right).Lock = new Switch(current.State);
                current.SwapRight(right);

                //create sub-switch lock branch
                r.ConnectTo(current);
                current.ConnectTo(map.CreateTemplate(TemplateType.BonusGoal));
                map.Track(current); // reuse SWL template
            }
        }

        private void AddSwitch(Map<Template, Room> map, ref int tick, Template current)
        {
            if (Roll() <= 10)
            {
                current.ReplaceWith(map.CreateTemplate(TemplateType.LayoutChooser))
                    .ConnectTo(map.CreateRoom(RoomType.Switch, current.State));
            }
            else
            {
                tick++;
                // R - L - switch
                //   \ SW
                Template r = map.CreateTemplate(TemplateType.RoomChooser);
                Template l = map.CreateTemplate(TemplateType.LayoutChooser);

                current.SwapLeft(r);
                r.ConnectTo(current);
                r.ConnectTo(l);
                l.ConnectTo(map.CreateRoom(RoomType.Switch, current.State));

                map.Track(current); // reuse switch sequence
            }
        }

        private void AddSwitchedSection(Map<Template, Room> map, Template current, Skid switches)
        {
            int state = switches.Next();

            var r = current.ReplaceWith(map.CreateTemplate(TemplateType.RoomChooser));

            foreach (var hall in r.Exit) hall.Lock = new Switch(state); // seal the normal exits

            r.ConnectTo(map.CreateTemplate(TemplateType.SwitchSeq, state));

            if (Roll() > 10)
            {
                // Add switch-lock
                var sl = map.CreateTemplate(TemplateType.SwitchLockSeq, state);
                var gb = map.CreateTemplate(TemplateType.BonusGoal);

                r.ConnectTo(sl);
                sl.ConnectTo(gb);
            }
        }

        private void AddLockChain(Map<Template, Room> map, ref int tick, Template current, Skid keys)
        {
            if (Roll() <= 10)
            {
                // () - R - L - key
                //        \-(k)- L - ()
                var r = map.CreateTemplate(TemplateType.RoomChooser);
                var keyL = map.CreateTemplate(TemplateType.LayoutChooser);
                var lockedL = map.CreateTemplate(TemplateType.LayoutChooser);

                int key = keys.Next();
                current.SwapLeft(r);
                r.ConnectTo(keyL);
                r.ConnectTo(lockedL).Lock = new Key(key);
                keyL.ConnectTo(map.CreateRoom(RoomType.Key, key));
                current.SwapRight(lockedL);
            }
            else
            {
                tick++;
                // () - r - K - ()
                //       \ - K - GB
                var r = map.CreateTemplate(TemplateType.RoomChooser);
                var newK = map.CreateTemplate(TemplateType.LockChain);
                var gb = map.CreateTemplate(TemplateType.BonusGoal);

                current.SwapLeft(r);
                r.ConnectTo(current);
                r.ConnectTo(newK);
                newK.ConnectTo(gb);

                map.Track(current); // Reuse key sequence
            }
        }

        private void CreateHook(Map<Template, Room> map, Template current)
        {
            current.ReplaceWith(map.CreateTemplate(TemplateType.RoomChooser))
                .ConnectTo(map.CreateTemplate(TemplateType.BonusGoal))
                .Lock = (Roll() > 10 ? new Secret() : null);
        }

        private void ChooseLayout(Map<Template, Room> map, ref int tick, bool firstLayout, Template current)
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
                    current.ReplaceWith(map.CreateTemplate(TemplateType.HookSequence));
                    break;
                case 3:
                case 4:
                case 5:
                    current.ReplaceWith(map.CreateTemplate(TemplateType.LockChain));
                    break;
                case 6:
                case 7:
                case 8:
                    current.ReplaceWith(map.CreateTemplate(TemplateType.MultiSwitch));
                    break;
                case 9:
                case 10:
                    current.ReplaceWith(map.CreateTemplate(TemplateType.LinearSequence, MaxLinearSequenceLength));
                    break;
                case 11:
                case 12:
                case 13:
                    tick++;
                    var frontL = map.CreateTemplate(TemplateType.LayoutChooser);
                    var r = map.CreateTemplate(TemplateType.RoomChooser);
                    var backL = map.CreateTemplate(TemplateType.LayoutChooser);
                    current.SwapLeft(frontL);
                    frontL.ConnectTo(r);
                    r.ConnectTo(backL);
                    current.SwapRight(backL);
                    break;
                default:
                    tick++;
                    var frontR = map.CreateTemplate(TemplateType.RoomChooser);
                    var backR = map.CreateTemplate(TemplateType.RoomChooser);
                    var topL = map.CreateTemplate(TemplateType.LayoutChooser);
                    var bottomL = map.CreateTemplate(TemplateType.LayoutChooser);

                    current.SwapLeft(frontR);
                    frontR.ConnectTo(topL, roll < 18 ? Direction.Forward : Direction.Both);
                    frontR.ConnectTo(bottomL, roll == 16 || roll == 17 ? Direction.Back : Direction.Both);
                    topL.ConnectTo(backR);
                    bottomL.ConnectTo(backR);
                    current.SwapRight(backR);
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

    public static class MapHelper
    {
        public static Template CreateTemplate(this Map<Template, Room> map, TemplateType type, int state = 0) => map.Track(new Template(type, state));

        public static Room CreateRoom(this Map<Template, Room> map, RoomType type, int keySwitch = 0) => map.Track(new Room(type, keySwitch));
    }
}