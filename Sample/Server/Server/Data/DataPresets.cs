using Google.Protobuf.Protocol;
using System;
using System.Collections.Generic;
using System.Text;

namespace Server.Data
{
    public static class DataPresets
    {
        public static Dictionary<int, SkillData> SkillDatas = new Dictionary<int, SkillData>()
        {
            {-1, Teleport },
            {1,  BasicProjectile},
            {2,  Meteo},
            {3,  IonStrike},
            {4,  HammerStrike},
            {5,  LightningField},
        };

        public static StatInfo MakeGanyuStat(int level)
        {
            StatInfo stat = new StatInfo();
            stat.Level = level;
            stat.Attack = level * 10;
            stat.Hp = stat.MaxHp = 100 * level;
            stat.Speed = 5.0f;
            stat.Radius = 0.5f;
            return stat;
        }

        public static StatInfo MakeChuChuStat(int level)
        {
            StatInfo stat = new StatInfo();
            stat.Level = level;
            stat.Attack = level * 2;
            stat.Hp = stat.MaxHp = 100 * level;
            stat.Speed = 4.0f;
            stat.Radius = 0.5f;
            return stat;
        }

        public static SkillData BasicProjectile
        {
            get
            {
                SkillData skillData = new SkillData()
                {
                    Id = 1,
                    Type = SkillType.Projectile,
                    Damage = 10,
                    CoolTimeFrame = 10,
                    LifeFrame = 20,
                    StateFrame = 6,
                    MoveSpeed = 10,
                    Range = 0.7f,
                    SpawnDelayTick = 250,
                    Name = "Projectile",
                };

                return skillData;
            }
        }

        public static SkillData Meteo
        {
            get
            {
                SkillData skillData = new SkillData()
                {
                    Id = 2,
                    Type = SkillType.Area,
                    Damage = 30,
                    CoolTimeFrame = 10,
                    LifeFrame = 100,
                    StateFrame = 10,
                    MoveSpeed = 0,
                    Range = 4.0f,
                    HitDelayFrame = 10,
                    SpawnDelayTick = 0,
                    Name = "Meteo",
                };

                return skillData;
            }
        }

        public static SkillData IonStrike
        {
            get
            {
                SkillData skillData = new SkillData()
                {
                    Id = 3,
                    Type = SkillType.Area,
                    Damage = 10,
                    CoolTimeFrame = 10,
                    LifeFrame = 100,
                    StateFrame = 10,
                    MoveSpeed = 0,
                    Range = 4.0f,
                    HitDelayFrame = 0,
                    LoopHitDelayFrame = 27,
                    SpawnDelayTick = 1000,
                    IsLoopSkill = true,
                    LoopTimeFrame = 27,
                    LoopDamage = 50,
                    Name = "IonStrike",
                };

                return skillData;
            }
        }

        public static SkillData LightningField
        {
            get
            {
                SkillData skillData = new SkillData()
                {
                    Id = 4,
                    Type = SkillType.Area,
                    Damage = 7,
                    CoolTimeFrame = 10,
                    LifeFrame = 100,
                    StateFrame = 10,
                    MoveSpeed = 0,
                    Range = 4.0f,
                    HitDelayFrame = 0,
                    LoopHitDelayFrame = 3,
                    SpawnDelayTick = 1000,
                    IsLoopSkill = true,
                    LoopTimeFrame = 15,
                    LoopDamage = 1,
                    Name = "LightningField",
                };

                return skillData;
            }
        }

        public static SkillData HammerStrike
        {
            get
            {
                SkillData skillData = new SkillData()
                {
                    Id = 3,
                    Type = SkillType.Area,
                    Damage = 40,
                    CoolTimeFrame = 10,
                    LifeFrame = 100,
                    StateFrame = 10,
                    MoveSpeed = 0,
                    Range = 4.0f,
                    HitDelayFrame = 12,
                    SpawnDelayTick = 0,
                    Name = "HammerStrike",
                };

                return skillData;
            }
        }

        public static SkillData Teleport
        {
            get
            {
                SkillData skillData = new SkillData()
                {
                    Id = -1,
                    Type = SkillType.Area,
                    StateFrame = 4,
                };

                return skillData;
            }
        }

        public static SkillData DelayAttack
        {
            get
            {
                SkillData skillData = new SkillData()
                {
                    Id = 2,
                    Type = SkillType.DelayAttack,
                    Damage = 5,
                    CoolTimeFrame = 10,
                    LifeFrame = 100,
                    StateFrame = 20,
                    MoveSpeed = 0,
                    Range = 4.0f,
                    HitDelayFrame = 7,
                    SpawnDelayTick = 0,
                };

                return skillData;
            }
        }
    }

    public class SkillData
    {
        public int Id;
        public SkillType Type;
        public bool IsLoopSkill;
        public int Damage;
        public int LoopDamage;
        public int CoolTimeFrame;
        public int LifeFrame;
        public int StateFrame;
        public int MoveSpeed;
        public float Range;
        public int SpawnDelayTick;
        public int HitDelayFrame;
        public int LoopHitDelayFrame;
        public int LoopTimeFrame;
        public string Name;
    }
}
