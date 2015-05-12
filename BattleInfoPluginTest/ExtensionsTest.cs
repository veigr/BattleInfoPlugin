using System;
using System.Linq;
using BattleInfoPlugin;
using BattleInfoPlugin.Models;
using BattleInfoPlugin.Models.Raw;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace BattleInfoPluginTest
{
    [TestClass]
    public class ExtensionsTest
    {
        [TestMethod]
        public void SetValuesTest()
        {
            var data = CreateEmptyTestData();

            data.SetValues(
                new[] { 10, 10, 30, 30, 50, 50 },
                (s, v) => s.NowHP = v);

            data[0].NowHP.Is(10);
            data[1].NowHP.Is(10);
            data[2].NowHP.Is(30);
            data[3].NowHP.Is(30);
            data[4].NowHP.Is(50);
            data[5].NowHP.Is(50);

            data.SetValues(
                new[] {0, 0, 10, 10, 0, 50},
                (s, v) => s.NowHP -= v);

            data[0].NowHP.Is(10);
            data[1].NowHP.Is(10);
            data[2].NowHP.Is(20);
            data[3].NowHP.Is(20);
            data[4].NowHP.Is(50);
            data[5].NowHP.Is(0);
        }

        [TestMethod]
        public void GetDoubleDamageTest()
        {
            var data = new double[]
            {
                -1,
                0,
                73,
                9.1,
                4,
                0,
                15,
            };
            var damage = data.GetDamages();
            damage.ToArray().Is(new int[]
            {
                0,
                73,
                9,
                4,
                0,
                15
            });
        }

        [TestMethod]
        public void GetObjectDamageTest()
        {
            var dam = new object[]
            {
                -1,
                new[] {134},
                new[] {30},
                new[] {239},
                new[] {9.1},
                new[] {45},
                new[] {7},
                new[] {121.1, 83.1},
            };
            var df = new object[]
            {
                -1,
                new[] {11},
                new[] {5},
                new[] {8},
                new[] {5},
                new[] {12},
                new[] {2},
                new[] {9, 9},
            };
            var firend = dam.GetFriendDamages(df);
            firend.ToArray().Is(new int[]
            {
                0,
                7,
                0,
                0,
                39,
                0,
            });
            var enemies = dam.GetEnemyDamages(df);
            enemies.ToArray().Is(new int[]
            {
                0,
                239,
                204,
                0,
                134,
                45,
            });
        }

        public static MembersShipData[] CreateEmptyTestData()
        {
            return Enumerable.Repeat(0, 6)
                .Select(_ => new MembersShipData()).ToArray();
        }

        public static MembersShipData[] CreateTestData()
        {
            return new[]
            {
                new MembersShipData {NowHP = 10, MaxHP = 10},
                new MembersShipData {NowHP = 10, MaxHP = 20},
                new MembersShipData {NowHP = 30, MaxHP = 30},
                new MembersShipData {NowHP = 30, MaxHP = 40},
                new MembersShipData {NowHP = 50, MaxHP = 50},
                new MembersShipData {NowHP = 50, MaxHP = 60},
            };
        }
    }
}
