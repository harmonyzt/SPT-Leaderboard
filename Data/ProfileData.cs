using System;
using System.Collections.Generic;
using Newtonsoft.Json;

namespace SPTLeaderboard.Data
{
    public class ProfileData
    {
        [JsonProperty("_id")] public string _ID { get; set; }

        [JsonProperty("aid")] public string aID { get; set; }

        [JsonProperty("savage")] public string Savage { get; set; }

        [JsonProperty("Info")] public Info Info { get; set; }
        
        [JsonProperty("Inventory")] public Inventory Inventory { get; set; }

        // [JsonProperty("Health")] public Health Health { get; set; }

        // [JsonProperty("Skills")] public Skills Skills { get; set; }
        
        // [JsonProperty("Hideout")] public Hideout Hideout { get; set; }

        // [JsonProperty("RagfairInfo")] public RagfairInfo RagfairInfo { get; set; }

        // [JsonProperty("Stats")] public Stats Stats { get; set; }
    }

    public class TraderData
    {
        [JsonProperty("id")] public string ID { get; set; }
        
        [JsonProperty("unlocked")] public bool Unlocked { get; set; }

        [JsonProperty("loyaltyLevel")] public int LoyaltyLevel { get; set; }

        [JsonProperty("salesSum")] public long SalesSum { get; set; }

        [JsonProperty("standing")] public double Standing { get; set; }

        [JsonProperty("disabled")] public bool Disabled { get; set; }
        
        [JsonProperty("notFound", NullValueHandling = NullValueHandling.Ignore)] public bool? NotFound { get; set; }
    }
    

    public class BodyParts
    {
        [JsonProperty("Chest")] public Chest Chest { get; set; }

        [JsonProperty("Head")] public Head Head { get; set; }

        [JsonProperty("LeftArm")] public LeftArm LeftArm { get; set; }

        [JsonProperty("LeftLeg")] public LeftLeg LeftLeg { get; set; }

        [JsonProperty("RightArm")] public RightArm RightArm { get; set; }

        [JsonProperty("RightLeg")] public RightLeg RightLeg { get; set; }

        [JsonProperty("Stomach")] public Stomach Stomach { get; set; }
    }

    public class Chest
    {
        [JsonProperty("Health")] public Health Health { get; set; }

        [JsonProperty("Effects")] public Effects Effects { get; set; }

        [JsonProperty("Amount")] public double Amount { get; set; }

        [JsonProperty("Type")] public string Type { get; set; }

        [JsonProperty("SourceId")] public object SourceId { get; set; }

        [JsonProperty("OverDamageFrom")] public object OverDamageFrom { get; set; }

        [JsonProperty("Blunt")] public bool Blunt { get; set; }

        [JsonProperty("ImpactsCount")] public int ImpactsCount { get; set; }
    }

    public class Common
    {
        [JsonProperty("Id")] public string Id { get; set; }

        [JsonProperty("Progress")] public double Progress { get; set; }

        [JsonProperty("PointsEarnedDuringSession")]
        public double PointsEarnedDuringSession { get; set; }

        [JsonProperty("LastAccess")] public int LastAccess { get; set; }
    }

    public class Customization
    {
        [JsonProperty("Body")] public string Body { get; set; }

        [JsonProperty("Feet")] public string Feet { get; set; }

        [JsonProperty("Hands")] public string Hands { get; set; }

        [JsonProperty("Head")] public string Head { get; set; }

        [JsonProperty("DogTag")] public string DogTag { get; set; }

        [JsonProperty("Wall")] public string Wall { get; set; }

        [JsonProperty("Floor")] public string Floor { get; set; }

        [JsonProperty("Light")] public string Light { get; set; }

        [JsonProperty("Ceiling")] public string Ceiling { get; set; }

        [JsonProperty("ShootingRangeMark")] public string ShootingRangeMark { get; set; }
    }

    public class DamageHistory
    {
        [JsonProperty("LethalDamagePart")] public string LethalDamagePart { get; set; }

        [JsonProperty("LethalDamage")] public object LethalDamage { get; set; }

        [JsonProperty("BodyParts")] public BodyParts BodyParts { get; set; }
    }

    public class DeathCause
    {
        [JsonProperty("DamageType")] public string DamageType { get; set; }

        [JsonProperty("Side")] public string Side { get; set; }

        [JsonProperty("Role")] public string Role { get; set; }

        [JsonProperty("WeaponId")] public string WeaponId { get; set; }
    }

    public class Dogtag
    {
        [JsonProperty("AccountId")] public string AccountId { get; set; }

        [JsonProperty("ProfileId")] public string ProfileId { get; set; }

        [JsonProperty("Nickname")] public string Nickname { get; set; }

        [JsonProperty("Side")] public int Side { get; set; }

        [JsonProperty("Level")] public int Level { get; set; }

        [JsonProperty("Time")] public DateTime Time { get; set; }

        [JsonProperty("Status")] public string Status { get; set; }

        [JsonProperty("KillerAccountId")] public string KillerAccountId { get; set; }

        [JsonProperty("KillerProfileId")] public string KillerProfileId { get; set; }

        [JsonProperty("KillerName")] public string KillerName { get; set; }

        [JsonProperty("WeaponName")] public string WeaponName { get; set; }

        [JsonProperty("CarriedByGroupMember")] public bool CarriedByGroupMember { get; set; }
    }

    public class Effects;

    public class Eft
    {
        [JsonProperty("SessionCounters")] public SessionCounters SessionCounters { get; set; }

        [JsonProperty("OverallCounters")] public OverallCounters OverallCounters { get; set; }

        [JsonProperty("SessionExperienceMult")]
        public double SessionExperienceMult { get; set; }

        [JsonProperty("ExperienceBonusMult")] public double ExperienceBonusMult { get; set; }

        [JsonProperty("TotalSessionExperience")]
        public int TotalSessionExperience { get; set; }

        [JsonProperty("LastSessionDate")] public int LastSessionDate { get; set; }

        [JsonProperty("DroppedItems")] public List<object> DroppedItems { get; set; }

        [JsonProperty("FoundInRaidItems")] public List<object> FoundInRaidItems { get; set; }

        [JsonProperty("Victims")] public List<Victim> Victims { get; set; }

        [JsonProperty("CarriedQuestItems")] public List<object> CarriedQuestItems { get; set; }

        [JsonProperty("DamageHistory")] public DamageHistory DamageHistory { get; set; }

        [JsonProperty("DeathCause")] public DeathCause DeathCause { get; set; }

        [JsonProperty("TotalInGameTime")] public int TotalInGameTime { get; set; }

        [JsonProperty("SurvivorClass")] public string SurvivorClass { get; set; }
    }

    public class Energy
    {
        [JsonProperty("Current")] public double Current { get; set; }

        [JsonProperty("Minimum")] public int Minimum { get; set; }

        [JsonProperty("Maximum")] public int Maximum { get; set; }

        [JsonProperty("OverDamageReceivedMultiplier")]
        public int OverDamageReceivedMultiplier { get; set; }

        [JsonProperty("EnvironmentDamageMultiplier")]
        public int EnvironmentDamageMultiplier { get; set; }
    }

    public class FaceShield
    {
        [JsonProperty("HitSeed")] public int HitSeed { get; set; }

        [JsonProperty("Hits")] public int Hits { get; set; }
    }

    public class FastPanel;

    public class Foldable
    {
        [JsonProperty("Folded")] public bool Folded { get; set; }
    }

    public class Head
    {
        [JsonProperty("Health")] public Health Health { get; set; }

        [JsonProperty("Effects")] public Effects Effects { get; set; }

        [JsonProperty("Amount")] public double Amount { get; set; }

        [JsonProperty("Type")] public string Type { get; set; }

        [JsonProperty("SourceId")] public object SourceId { get; set; }

        [JsonProperty("OverDamageFrom")] public object OverDamageFrom { get; set; }

        [JsonProperty("Blunt")] public bool Blunt { get; set; }

        [JsonProperty("ImpactsCount")] public int ImpactsCount { get; set; }
    }

    public class Health
    {
        [JsonProperty("BodyParts")] public BodyParts BodyParts { get; set; }

        [JsonProperty("Energy")] public Energy Energy { get; set; }

        [JsonProperty("Hydration")] public Hydration Hydration { get; set; }

        [JsonProperty("Temperature")] public Temperature Temperature { get; set; }

        [JsonProperty("Poison")] public Poison Poison { get; set; }

        [JsonProperty("UpdateTime")] public object UpdateTime { get; set; }

        [JsonProperty("Current")] public double Current { get; set; }

        [JsonProperty("Minimum")] public int Minimum { get; set; }

        [JsonProperty("Maximum")] public int Maximum { get; set; }

        [JsonProperty("OverDamageReceivedMultiplier")]
        public int OverDamageReceivedMultiplier { get; set; }

        [JsonProperty("EnvironmentDamageMultiplier")]
        public int EnvironmentDamageMultiplier { get; set; }
    }

    public class Hideout
    {
        [JsonProperty("Production")] public Production Production { get; set; }

        [JsonProperty("Seed")] public string Seed { get; set; }

        [JsonProperty("Customization")] public Customization Customization { get; set; }

        [JsonProperty("MannequinPoses")] public MannequinPoses MannequinPoses { get; set; }
    }

    public class HideoutAreaStashes
    {
        [JsonProperty("PlaceOfFame")] public string PlaceOfFame { get; set; }

        [JsonProperty("WeaponStand")] public string WeaponStand { get; set; }

        [JsonProperty("WeaponStandSecondary")] public string WeaponStandSecondary { get; set; }

        [JsonProperty("CircleOfCultists")] public string CircleOfCultists { get; set; }
    }

    public class Hydration
    {
        [JsonProperty("Current")] public double Current { get; set; }

        [JsonProperty("Minimum")] public int Minimum { get; set; }

        [JsonProperty("Maximum")] public int Maximum { get; set; }

        [JsonProperty("OverDamageReceivedMultiplier")]
        public int OverDamageReceivedMultiplier { get; set; }

        [JsonProperty("EnvironmentDamageMultiplier")]
        public int EnvironmentDamageMultiplier { get; set; }
    }

    public class Info
    {
        [JsonProperty("Nickname")] public string Nickname { get; set; }

        [JsonProperty("MainProfileNickname")] public string MainProfileNickname { get; set; }

        [JsonProperty("Side")] public string Side { get; set; }

        [JsonProperty("PrestigeLevel")] public int PrestigeLevel { get; set; }

        [JsonProperty("RegistrationDate")] public int RegistrationDate { get; set; }

        [JsonProperty("SavageLockTime")] public double SavageLockTime { get; set; }

        [JsonProperty("GroupId")] public object GroupId { get; set; }

        [JsonProperty("TeamId")] public object TeamId { get; set; }

        [JsonProperty("EntryPoint")] public string EntryPoint { get; set; }

        [JsonProperty("NicknameChangeDate")] public int NicknameChangeDate { get; set; }

        [JsonProperty("GameVersion")] public string GameVersion { get; set; }

        [JsonProperty("Type")] public string Type { get; set; }

        [JsonProperty("HasCoopExtension")] public bool HasCoopExtension { get; set; }

        [JsonProperty("HasPveGame")] public bool HasPveGame { get; set; }

        [JsonProperty("Voice")] public string Voice { get; set; }

        [JsonProperty("Experience")] public int Experience { get; set; }

        [JsonProperty("Level")] public int Level { get; set; }
    }

    public class InsuredItem
    {
        [JsonProperty("tid")] public string tid { get; set; }

        [JsonProperty("itemId")] public string itemId { get; set; }
    }

    public class Inventory
    {
        [JsonProperty("items")] public List<InventoryItem> Items { get; set; }
    }

    public class InventoryItem
    {
        [JsonProperty("_id")] public string ID { get; set; }

        [JsonProperty("_tpl")] public string Tpl { get; set; }
    }

    public class Item2
    {
        [JsonProperty("Key")] public List<string> Key { get; set; }

        [JsonProperty("Value")] public int Value { get; set; }
    }

    public class LeftArm
    {
        [JsonProperty("Health")] public Health Health { get; set; }

        [JsonProperty("Effects")] public Effects Effects { get; set; }

        [JsonProperty("Amount")] public double Amount { get; set; }

        [JsonProperty("Type")] public string Type { get; set; }

        [JsonProperty("SourceId")] public object SourceId { get; set; }

        [JsonProperty("OverDamageFrom")] public object OverDamageFrom { get; set; }

        [JsonProperty("Blunt")] public bool Blunt { get; set; }

        [JsonProperty("ImpactsCount")] public int ImpactsCount { get; set; }
    }

    public class LeftLeg
    {
        [JsonProperty("Health")] public Health Health { get; set; }

        [JsonProperty("Effects")] public Effects Effects { get; set; }

        [JsonProperty("Amount")] public double Amount { get; set; }

        [JsonProperty("Type")] public string Type { get; set; }

        [JsonProperty("SourceId")] public object SourceId { get; set; }

        [JsonProperty("OverDamageFrom")] public object OverDamageFrom { get; set; }

        [JsonProperty("Blunt")] public bool Blunt { get; set; }

        [JsonProperty("ImpactsCount")] public int ImpactsCount { get; set; }
    }

    public class Location
    {
        [JsonProperty("x")] public int x { get; set; }

        [JsonProperty("y")] public int y { get; set; }

        [JsonProperty("r")] public string r { get; set; }
    }

    public class MannequinPoses
    {
    }

    public class Mastering
    {
        [JsonProperty("Id")] public string Id { get; set; }

        [JsonProperty("Progress")] public int Progress { get; set; }
    }

    public class MoneyTransferLimitData
    {
        [JsonProperty("nextResetTime")] public int nextResetTime { get; set; }

        [JsonProperty("remainingLimit")] public int remainingLimit { get; set; }

        [JsonProperty("totalLimit")] public int totalLimit { get; set; }

        [JsonProperty("resetInterval")] public int resetInterval { get; set; }
    }

    public class OverallCounters
    {
        [JsonProperty("Items")] public List<InventoryItem> Items { get; set; }
    }

    public class Poison
    {
        [JsonProperty("Current")] public int Current { get; set; }

        [JsonProperty("Minimum")] public int Minimum { get; set; }

        [JsonProperty("Maximum")] public int Maximum { get; set; }

        [JsonProperty("OverDamageReceivedMultiplier")]
        public int OverDamageReceivedMultiplier { get; set; }

        [JsonProperty("EnvironmentDamageMultiplier")]
        public int EnvironmentDamageMultiplier { get; set; }
    }

    public class Prestige
    {
    }

    public class Production
    {
    }

    public class RagfairInfo
    {
        [JsonProperty("rating")] public double rating { get; set; }

        [JsonProperty("isRatingGrowing")] public bool isRatingGrowing { get; set; }
    }

    public class Repairable
    {
        [JsonProperty("MaxDurability")] public int MaxDurability { get; set; }

        [JsonProperty("Durability")] public double Durability { get; set; }
    }

    public class Resource
    {
        [JsonProperty("Value")] public int Value { get; set; }
    }

    public class RightArm
    {
        [JsonProperty("Health")] public Health Health { get; set; }

        [JsonProperty("Effects")] public Effects Effects { get; set; }

        [JsonProperty("Amount")] public double Amount { get; set; }

        [JsonProperty("Type")] public string Type { get; set; }

        [JsonProperty("SourceId")] public object SourceId { get; set; }

        [JsonProperty("OverDamageFrom")] public object OverDamageFrom { get; set; }

        [JsonProperty("Blunt")] public bool Blunt { get; set; }

        [JsonProperty("ImpactsCount")] public int ImpactsCount { get; set; }
    }

    public class RightLeg
    {
        [JsonProperty("Health")] public Health Health { get; set; }

        [JsonProperty("Effects")] public Effects Effects { get; set; }

        [JsonProperty("Amount")] public double Amount { get; set; }

        [JsonProperty("Type")] public string Type { get; set; }

        [JsonProperty("SourceId")] public object SourceId { get; set; }

        [JsonProperty("OverDamageFrom")] public object OverDamageFrom { get; set; }

        [JsonProperty("Blunt")] public bool Blunt { get; set; }

        [JsonProperty("ImpactsCount")] public int ImpactsCount { get; set; }
    }

    public class SessionCounters
    {
        [JsonProperty("Items")] public List<InventoryItem> Items { get; set; }
    }

    public class Settings
    {
        [JsonProperty("Role")] public string Role { get; set; }

        [JsonProperty("BotDifficulty")] public string BotDifficulty { get; set; }

        [JsonProperty("Experience")] public int Experience { get; set; }

        [JsonProperty("StandingForKill")] public int StandingForKill { get; set; }

        [JsonProperty("AggressorBonus")] public int AggressorBonus { get; set; }

        [JsonProperty("UseSimpleAnimator")] public bool UseSimpleAnimator { get; set; }
    }

    public class Sight
    {
        [JsonProperty("ScopesCurrentCalibPointIndexes")]
        public List<int> ScopesCurrentCalibPointIndexes { get; set; }

        [JsonProperty("ScopesSelectedModes")] public List<int> ScopesSelectedModes { get; set; }

        [JsonProperty("SelectedScope")] public int SelectedScope { get; set; }

        [JsonProperty("ScopeZoomValue")] public int ScopeZoomValue { get; set; }
    }

    public class Skills
    {
        [JsonProperty("Common")] public List<Common> Common { get; set; }

        [JsonProperty("Mastering")] public List<Mastering> Mastering { get; set; }
    }

    public class Slot
    {
        [JsonProperty("item")] public object item { get; set; }
    }

    public class Stats
    {
        [JsonProperty("Eft")] public Eft Eft { get; set; }
    }

    public class Stomach
    {
        [JsonProperty("Health")] public Health Health { get; set; }

        [JsonProperty("Effects")] public Effects Effects { get; set; }

        [JsonProperty("Amount")] public double Amount { get; set; }

        [JsonProperty("Type")] public string Type { get; set; }

        [JsonProperty("SourceId")] public object SourceId { get; set; }

        [JsonProperty("OverDamageFrom")] public object OverDamageFrom { get; set; }

        [JsonProperty("Blunt")] public bool Blunt { get; set; }

        [JsonProperty("ImpactsCount")] public int ImpactsCount { get; set; }
    }

    public class Temperature
    {
        [JsonProperty("Current")] public int Current { get; set; }

        [JsonProperty("Minimum")] public int Minimum { get; set; }

        [JsonProperty("Maximum")] public int Maximum { get; set; }

        [JsonProperty("OverDamageReceivedMultiplier")]
        public int OverDamageReceivedMultiplier { get; set; }

        [JsonProperty("EnvironmentDamageMultiplier")]
        public int EnvironmentDamageMultiplier { get; set; }
    }

    public class Togglable
    {
        [JsonProperty("On")] public bool On { get; set; }
    }

    public class UnlockedInfo
    {
        [JsonProperty("unlockedProductionRecipe")]
        public List<object> unlockedProductionRecipe { get; set; }
    }

    public class Upd
    {
        [JsonProperty("Repairable")] public Repairable Repairable { get; set; }

        [JsonProperty("Foldable")] public Foldable Foldable { get; set; }

        [JsonProperty("Resource")] public Resource Resource { get; set; }

        [JsonProperty("Dogtag")] public Dogtag Dogtag { get; set; }

        [JsonProperty("Sight")] public Sight Sight { get; set; }

        [JsonProperty("Togglable")] public Togglable Togglable { get; set; }

        [JsonProperty("FaceShield")] public FaceShield FaceShield { get; set; }
    }

    public class Victim
    {
        [JsonProperty("AccountId")] public string AccountId { get; set; }

        [JsonProperty("ProfileId")] public string ProfileId { get; set; }

        [JsonProperty("Name")] public string Name { get; set; }

        [JsonProperty("Side")] public string Side { get; set; }

        [JsonProperty("Time")] public string Time { get; set; }

        [JsonProperty("Level")] public int Level { get; set; }

        [JsonProperty("PrestigeLevel")] public int PrestigeLevel { get; set; }

        [JsonProperty("BodyPart")] public string BodyPart { get; set; }

        [JsonProperty("Weapon")] public string Weapon { get; set; }

        [JsonProperty("Distance")] public double Distance { get; set; }

        [JsonProperty("Role")] public string Role { get; set; }

        [JsonProperty("Location")] public string Location { get; set; }

        [JsonProperty("GInterface187.ProfileId")]
        public string GInterface187ProfileId { get; set; }

        [JsonProperty("GInterface187.Nickname")]
        public string GInterface187Nickname { get; set; }

        [JsonProperty("GInterface187.Side")] public string GInterface187Side { get; set; }

        [JsonProperty("GInterface187.PrestigeLevel")]
        public int GInterface187PrestigeLevel { get; set; }
    }
}