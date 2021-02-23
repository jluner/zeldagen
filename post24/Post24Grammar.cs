using System;
using System.Linq;

namespace zeldagen.post24
{
    public class Post24Grammar : IMapGrammar<Template, Room>
    {
        private readonly Random rng = new();

        public IRoomClassifier<Room> Classifier => new Post24Classifier();

        public Map<Template, Room> GenerateMap()
        {
            Skid switches = new();
            Map<Template, Room> map = new(MapReducible);
            map.CreateTemplate(TemplateType.DungeonStart);

            foreach (var current in map.RemainingTemplates())
            {
                switch (current.Type)
                {
                    case TemplateType.DungeonStart:
                        MapDungeonStart(map);
                        break;
                    case TemplateType.OM_OneToManyItem:
                        MapOneToManyItems(map, current);
                        break;
                    case TemplateType.OO_OneToOneBoss:
                        MapBossSequence(map, current);
                        break;
                    case TemplateType.UI_UniqueItem:
                        MapUniqueItem(map, current);
                        break;
                    case TemplateType.MI_ManyItemSequence:
                        MapManyItemsSequence(map, current);
                        break;
                    case TemplateType.GB_BonusGoal:
                        current.ReplaceWith(Roll() switch
                        {
                            var i when i <= 5 => map.CreateTemplate(TemplateType.UI_UniqueItem, Item.HeartPiece()),
                            var i when i <= 10 => map.CreateTemplate(TemplateType.MI_ManyItemSequence, Item.Rupee()),
                            var i when i <= 15 => map.CreateTemplate(TemplateType.MI_ManyItemSequence, Item.Bombs()),
                            _ => map.CreateTemplate(TemplateType.MI_ManyItemSequence, Item.Arrows())
                        });
                        break;
                    case TemplateType.OL_OneLockSequence:
                        MapLockSequence(map, current);
                        break;
                    case TemplateType.C_Chain:
                        current.ReplaceWith(
                             map.CreateTemplate(Roll() switch
                             {
                                 var i when i <= 5 => TemplateType.H_HookSequence,
                                 var i when i <= 10 => TemplateType.MO_ManyToManyLockSequence,
                                 var i when i <= 15 => TemplateType.MM_SwitchLockChain,
                                 _ => TemplateType.MS_MultiSwitchSequence
                             }));
                        break;
                    case TemplateType.H_HookSequence:
                        current.ReplaceWith(map.CreateRoom(RoomType.Empty))
                            .ConnectTo(map.CreateTemplate(TemplateType.GB_BonusGoal));
                        break;
                    case TemplateType.S_LinearSequence:
                        MapLinearSequence(map, current);
                        break;
                    case TemplateType.ML_ManyLockSequence:
                        MapManyLockSequence(map, current);
                        break;
                    case TemplateType.MO_ManyToManyLockSequence:
                        MapManyToManyLockSequence(map, current);
                        break;
                    case TemplateType.MM_SwitchLockChain:
                        MapSwitchLockChain(switches, map, current);
                        break;
                    case TemplateType.MM2_SwitchLockChain:
                        MapExtraSwitchLocks(map, current);
                        break;
                    case TemplateType.MS_MultiSwitchSequence:
                        MapMultiSwitchSequence(switches, map, current);
                        break;
                    case TemplateType.MS2_MultiSwitchSequence:
                        MapExtraSwitches(map, current);
                        break;
                    case TemplateType.SW_SwitchSequence:
                        MapSwitchSequence(map, current);
                        break;
                    case TemplateType.SWL_SwitchLockSequence:
                        MapSwitchLockSequence(map, current);
                        break;
                    default: throw new InvalidOperationException("Unrecognized node " + current);
                }
            }

            return map;
        }

        private void MapDungeonStart(Map<Template, Room> map)
        {
            var entrance = map.CreateRoom(RoomType.Start);
            var goal = map.CreateRoom(RoomType.Goal);
            switch (Roll())
            {
                case var x when x <= 5:
                    MapStraightDungeon();
                    break;
                case var x when x <= 10:
                    MapBonusHeartDungeon();
                    break;
                case var x when x <= 15:
                    MapEasyBossKeyDungeon();
                    break;
                default:
                    MapSealedBossKeyDungeon();
                    break;
            }

            void MapStraightDungeon()
            {
                var om = map.CreateTemplate(TemplateType.OM_OneToManyItem, Item.DungeonItem());
                var oo = map.CreateTemplate(TemplateType.OO_OneToOneBoss);
                entrance.ConnectTo(om);
                om.ConnectTo(oo);
                oo.ConnectTo(goal);
            }

            void MapBonusHeartDungeon()
            {
                var c = map.CreateTemplate(TemplateType.C_Chain);
                var om = map.CreateTemplate(TemplateType.OM_OneToManyItem, Item.DungeonItem());
                var oo = map.CreateTemplate(TemplateType.OO_OneToOneBoss);
                var ol = map.CreateTemplate(TemplateType.OL_OneLockSequence, Item.NonDungeonItem());
                var ui = map.CreateTemplate(TemplateType.UI_UniqueItem, Item.HeartPiece());

                entrance.ConnectTo(c);
                c.ConnectTo(ol);
                c.ConnectTo(om);
                ol.ConnectTo(ui);
                om.ConnectTo(oo);
                oo.ConnectTo(goal);
            }

            void MapEasyBossKeyDungeon()
            {
                var itemBranch = map.CreateTemplate(TemplateType.UI_UniqueItem, Item.DungeonItem());
                var bossKeyBranch = map.CreateTemplate(TemplateType.UI_UniqueItem, Item.BossKey());
                var itemLock = map.CreateTemplate(TemplateType.OL_OneLockSequence, Item.DungeonItem());
                var bossLock = map.CreateTemplate(TemplateType.OL_OneLockSequence, Item.BossKey());
                var endBoss = map.CreateRoom(RoomType.EndBoss);

                entrance.ConnectTo(itemBranch);
                entrance.ConnectTo(bossKeyBranch);
                entrance.ConnectTo(itemLock);
                itemLock.ConnectTo(bossLock);
                bossLock.ConnectTo(endBoss);
                endBoss.ConnectTo(goal);
            }

            void MapSealedBossKeyDungeon()
            {
                var itemBranch = map.CreateTemplate(TemplateType.UI_UniqueItem, Item.DungeonItem());
                var bossKeyBranch = map.CreateTemplate(TemplateType.UI_UniqueItem, Item.BossKey());
                var itemLock = map.CreateTemplate(TemplateType.OL_OneLockSequence, Item.DungeonItem());
                var bossLock = map.CreateTemplate(TemplateType.OL_OneLockSequence, Item.BossKey());
                var endBoss = map.CreateRoom(RoomType.EndBoss);

                entrance.ConnectTo(itemBranch);
                entrance.ConnectTo(itemLock);
                entrance.ConnectTo(bossLock);
                itemLock.ConnectTo(bossKeyBranch);
                bossLock.ConnectTo(endBoss);
                endBoss.ConnectTo(goal);
            }
        }

        private static void MapOneToManyItems(Map<Template, Room> map, Template current)
        {
            var empty = map.CreateRoom(RoomType.Empty);
            var item = map.CreateTemplate(TemplateType.UI_UniqueItem, current.Item);
            var locks = map.CreateTemplate(TemplateType.ML_ManyLockSequence, current.Item);
            var exit = map.CreateTemplate(TemplateType.OL_OneLockSequence, current.Item);
            var bonus = map.CreateTemplate(TemplateType.GB_BonusGoal);

            current.SwapLeft(empty);
            empty.ConnectTo(item);
            empty.ConnectTo(locks);
            locks.ConnectTo(bonus);
            empty.ConnectTo(exit);
            current.SwapRight(exit);
        }

        private static void MapBossSequence(Map<Template, Room> map, Template current)
        {
            var empty = map.CreateRoom(RoomType.Empty);
            var bossKey = map.CreateTemplate(TemplateType.UI_UniqueItem, Item.BossKey());
            var bossHall = map.CreateTemplate(TemplateType.OL_OneLockSequence, Item.BossKey());
            var boss = map.CreateRoom(RoomType.EndBoss);

            current.SwapLeft(empty);
            empty.ConnectTo(bossKey);
            empty.ConnectTo(bossHall);
            bossHall.ConnectTo(boss);
            current.SwapRight(boss);
        }

        private void MapUniqueItem(Map<Template, Room> map, Template current)
        {
            // () <-> C/S() <-> em <-> item
            //   ^---------------------/
            var main = map.CreateTemplate(
                !map.Cap() && Roll() <= 10
                    ? TemplateType.C_Chain
                    : TemplateType.S_LinearSequence);
            var miniboss = map.CreateRoom(RoomType.MiniBoss);
            var item = map.CreateRoom(RoomType.Item, current.Item);

            current.SwapLeft(main);
            main.ConnectTo(miniboss);
            miniboss.ConnectTo(item);

            main.Entrance.First().From.ConnectTo(item, Direction.Back);
        }

        private void MapManyItemsSequence(Map<Template, Room> map, Template current)
        {
            // () <-> C/S() <-> item
            //   ^-------------/
            var main = map.CreateTemplate(
                !map.Cap() && Roll() <= 10
                    ? TemplateType.C_Chain
                    : TemplateType.S_LinearSequence);
            var item = map.CreateRoom(RoomType.Item, current.Item);
            current.SwapLeft(main);
            main.ConnectTo(item);
            main.Entrance.First().From.ConnectTo(item, Direction.Back);
        }

        private void MapManyLockSequence(Map<Template, Room> map, Template current)
        {
            if (Roll() <= 10)
            {
                current.ReplaceWith(map.CreateTemplate(TemplateType.OL_OneLockSequence, current.Item));
            }
            else
            {
                var start = map.CreateRoom(RoomType.Empty);
                var bonusGoal = map.CreateTemplate(TemplateType.GB_BonusGoal);
                var exitRoute = map.CreateTemplate(TemplateType.OL_OneLockSequence, current.Item);

                current.SwapLeft(start);
                current.SwapRight(exitRoute);

                start.ConnectTo(current);
                start.ConnectTo(exitRoute);
                current.ConnectTo(bonusGoal);

                map.Track(current); // Reuse this ML
            }
        }

        private void MapLinearSequence(Map<Template, Room> map, Template current)
        {
            switch (Roll())
            {
                case var i when i <= 5:
                    current.ReplaceWith(map.CreateRoom(RoomType.Empty));
                    break;
                case var i when i <= 10:
                    current.ReplaceWith(map.CreateRoom(RoomType.Puzzle));
                    break;
                case var i when i <= 15:
                    current.ReplaceWith(map.CreateRoom(RoomType.Enemy));
                    break;
                default:
                    //lengthen the sequence
                    var nextSeq = map.CreateTemplate(TemplateType.S_LinearSequence);
                    current.SwapRight(nextSeq);
                    current.ConnectTo(nextSeq);
                    map.Track(current);
                    break;
            }
        }

        private static void MapManyToManyLockSequence(Map<Template, Room> map, Template current)
        {
            var start = map.CreateRoom(RoomType.Empty);
            var keyPath = map.CreateTemplate(TemplateType.MI_ManyItemSequence, Item.SmallKey());
            var lockPath = map.CreateTemplate(TemplateType.OL_OneLockSequence, Item.SmallKey());
            current.SwapLeft(start);
            start.ConnectTo(keyPath);
            start.ConnectTo(lockPath);
            current.SwapRight(lockPath);
        }

        private static void MapSwitchLockChain(Skid switches, Map<Template, Room> map, Template current)
        {
            var @switch = new ToggleSwitch(switches.Next(), false);
            var start = map.CreateRoom(RoomType.Empty);
            var switchBranch = map.CreateTemplate(TemplateType.SW_SwitchSequence, @switch);
            var subBranch = map.CreateTemplate(TemplateType.MM2_SwitchLockChain, @switch.Toggle());
            var exitBranch = map.CreateTemplate(TemplateType.SWL_SwitchLockSequence, @switch);
            var bonus = map.CreateTemplate(TemplateType.GB_BonusGoal);

            current.SwapLeft(start);
            start.ConnectTo(switchBranch);
            start.ConnectTo(subBranch);
            subBranch.ConnectTo(bonus);
            start.ConnectTo(exitBranch);
            current.SwapRight(exitBranch);
        }

        private void MapLockSequence(Map<Template, Room> map, Template current)
        {
            var main = map.CreateTemplate(!map.Cap() && Roll() <= 10 ? TemplateType.C_Chain : TemplateType.S_LinearSequence);
            var finish = map.CreateRoom(RoomType.Empty);
            current.SwapLeft(main);
            current.SwapRight(finish);
            main.ConnectTo(finish).Lock = current.Item.ToLock();
        }

        private void MapSwitchSequence(Map<Template, Room> map, Template current)
        {
            // () <-> C/S() <-> switch
            //   ^-------------/
            var main = map.CreateTemplate(
                !map.Cap() && Roll() <= 10
                    ? TemplateType.C_Chain
                    : TemplateType.S_LinearSequence);
            var switchRoom = map.CreateRoom(RoomType.Item, current.Item); // This is the switch

            current.SwapLeft(main);
            main.ConnectTo(switchRoom);
            main.Entrance.First().From.ConnectTo(switchRoom, Direction.Back);
        }

        private static void MapMultiSwitchSequence(Skid switches, Map<Template, Room> map, Template current)
        {
            var @switch = new Switch(switches.Next());
            var start = map.CreateRoom(RoomType.Empty);
            var switchBranch = map.CreateTemplate(TemplateType.SW_SwitchSequence, @switch);
            var extraSwitches = map.CreateTemplate(TemplateType.MS2_MultiSwitchSequence, @switch);
            var exitBranch = map.CreateTemplate(TemplateType.SWL_SwitchLockSequence, @switch);

            current.SwapLeft(start);
            current.SwapRight(exitBranch);

            start.ConnectTo(switchBranch);
            start.ConnectTo(extraSwitches);
            start.ConnectTo(exitBranch);
        }

        private void MapSwitchLockSequence(Map<Template, Room> map, Template current)
        {
            var hall = map.CreateTemplate(!map.Cap() && Roll() <= 10 ? TemplateType.C_Chain : TemplateType.S_LinearSequence);
            var lastRoom = map.CreateRoom(RoomType.Empty);
            current.SwapLeft(hall);
            current.SwapRight(lastRoom);
            hall.ConnectTo(lastRoom).Lock = current.Item.ToLock();
        }

        private void MapExtraSwitches(Map<Template, Room> map, Template current)
        {
            var start = map.CreateRoom(RoomType.Empty);
            current.SwapLeft(start);
            start.ConnectTo(map.CreateTemplate(TemplateType.SW_SwitchSequence, current.Item));
            if (Roll() <= 10)
            {
                // Add more switches
                start.ConnectTo(current);
                map.Track(current);
            }
        }

        private void MapExtraSwitchLocks(Map<Template, Room> map, Template current)
        {
            ToggleSwitch orig = (ToggleSwitch)current.Item;
            ToggleSwitch toggled = orig.Toggle();
            var lockEntry = map.CreateTemplate(TemplateType.SWL_SwitchLockSequence, orig);
            var lockExit = map.CreateTemplate(TemplateType.SWL_SwitchLockSequence, toggled);
            var newSwitch = map.CreateTemplate(TemplateType.SW_SwitchSequence, orig);
            current.SwapLeft(lockEntry);
            current.SwapRight(lockExit);
            lockEntry.ConnectTo(newSwitch);
            lockEntry.ConnectTo(lockExit);
            if (Roll() <= 10)
            {
                var subBranch = map.CreateTemplate(TemplateType.MM2_SwitchLockChain, toggled);
                lockEntry.ConnectTo(subBranch);
                subBranch.ConnectTo(map.CreateTemplate(TemplateType.GB_BonusGoal));
            }
        }

        private static Reduction MapReducible(Room left, Room right)
        {
            // Simple: hallway shrinking
            if (left.Exit.Count == 1 && right.Entrance.Count == 1)
            {
                if (left.Kind == right.Kind) return Reduction.MergeRightToLeft;
                if (AllowEmptyMerge(left, right)) return Reduction.MergeLeftToRight;
                if (AllowEmptyMerge(right, left)) return Reduction.MergeRightToLeft;
            }
            return Reduction.Keep;

            bool AllowEmptyMerge(Room empty, Room other) => empty.Kind == RoomType.Empty && (other.Kind == RoomType.Puzzle || other.Kind == RoomType.Enemy);
        }

        private int Roll() => rng.Next(1, 21);
    }

    public static class MapHelper
    {
        public static Template CreateTemplate(this Map<Template, Room> map, TemplateType type, Item item = null)
        {
            var next = new Template(type, item);
            map.Track(next);
            return next;
        }

        public static Room CreateRoom(this Map<Template, Room> map, RoomType type, Item item = null)
        {
            var room = new Room(type, item);
            map.Track(room);
            return room;
        }

        public static bool Cap(this Map<Template, Room> map) => map.Rooms.Count > 40;
    }
}