using BepInEx.Configuration;
using RiskOfOptions;
using RiskOfOptions.OptionConfigs;
using RiskOfOptions.Options;
using RoR2;
using RoR2.ContentManagement;
using System;
using System.Linq;

namespace BetterCommandArtifact
{
    public static class Settings
    {
        #region Main Settings
        public static ConfigEntry<bool> UseItemTiers { get; set; }
        public static ConfigDefinition UseItemTiersDef = new("Settings", "Use Item Tiers?");
        public static ConfigDescription UseItemTiersDesc = new("Control the amount of items shown in the Command Menu for each Tier \n If False, 'Default Items Shown' will be used for every Tier");

        public static ConfigEntry<bool> MultipleBossItems { get; set; }
        public static ConfigDefinition AllowMultipleBossDef = new("Settings", "Multiple Boss Items?");
        public static ConfigDescription AllowMultipleBossDesc = new("Changes whether boss items drop as Command Cubes or drop a single item");

        public static ConfigEntry<int> IncludeLockedItems { get; set; }
        public static ConfigDefinition IncludeLockedItemsDef = new("Settings", "Include Locked Items?");
        public static ConfigDescription IncludeLockedItemsDesc = new("Include locked items in the Command Menu\n(Locked due to Challenge Progression or DLC)\n" +
                                                                             "0 = Show Locked Items\n1 = Hide Locked Items\n2 = Unlock All Items");
        #endregion

        #region Tier Settings
        public static ConfigEntry<int> DefaultItemsShown { get; set; }
        public static ConfigDefinition DefaultItemsShownDef = new ("Settings", "Default");
        public static ConfigDescription DefaultItemsShownDesc = new ("Default amount of items shown in the Command Menu");
               
        public static ConfigEntry<int> CommonItemsShown { get; set; }
        public static ConfigDefinition CommonItemsShownDef = new ("Settings", "Common");
        public static ConfigDescription CommonItemsShownDesc = new ("Amount of Common items shown in the Command Menu");
               
        public static ConfigEntry<int> UncommonItemsShown { get; set; }
        public static ConfigDefinition UncommonItemsShownDef = new ("Settings", "Uncommon");
        public static ConfigDescription UncommonItemsShownDesc = new ("Amount of Uncommon items shown in the Command Menu");
               
        public static ConfigEntry<int> LegendaryItemsShown { get; set; }
        public static ConfigDefinition LegendaryItemsShownDef = new ("Settings", "Legendary");
        public static ConfigDescription LegendaryItemsShownDesc = new ("Amount of Legendary items shown in the Command Menu");
               
        public static ConfigEntry<int> CommonVoidItemsShown { get; set; }
        public static ConfigDefinition CommonVoidItemsShownDef = new("Settings", "Common Void");
        public static ConfigDescription CommonVoidItemsShownDesc = new ("Amount of Common Void items shown in the Command Menu");

        public static ConfigEntry<int> UncommonVoidItemsShown { get; set; }
        public static ConfigDefinition UncommonVoidItemsShownDef = new("Settings", "Uncommon Void");
        public static ConfigDescription UncommonVoidItemsShownDesc = new("Amount of Uncommon Void items shown in the Command Menu");

        public static ConfigEntry<int> LegendaryVoidItemsShown { get; set; }
        public static ConfigDefinition LegendaryVoidItemsShownDef = new("Settings", "Legendary Void");
        public static ConfigDescription LegendaryVoidItemsShownDesc = new("Amount of Legendary Voiditems shown in the Command Menu");

        public static ConfigEntry<int> EquipmentItemsShown { get; set; }
        public static ConfigDefinition EquipmentItemsShownDef = new ("Settings", "Equipment");
        public static ConfigDescription EquipmentItemsShownDesc = new ("Amount of Equipment items shown in the Command Menu");

        public static ConfigEntry<int> BossItemsShown { get; set; }
        public static ConfigDefinition BossItemsShownDef = new("Settings", "Boss");
        public static ConfigDescription BossItemsShownDesc = new("Amount of Boss items shown in the Command Menu");

        public static ConfigEntry<int> LunarItemsShown { get; set; }
        public static ConfigDefinition LunarItemsShownDef = new("Settings", "Lunar");
        public static ConfigDescription LunarItemsShownDesc = new("Amount of Lunar items shown in the Command Menu");

        public static ConfigEntry<int> LunarEquipmentItemsShown { get; set; }
        public static ConfigDefinition LunarEquipmentItemsShownDef = new("Settings", "Lunar Equipment");
        public static ConfigDescription LunarEquipmentItemsShownDesc = new("Amount of Lunar Equipment items shown in the Command Menu");

        static IntSliderConfig itemsShownConfig = new() 
        { 
            min = 1,
            max = 100,
            checkIfDisabled = CheckItemTiers,
        };
        #endregion


        public static void InitConfig(ConfigFile config)
        {
            var items = ContentManager.itemDefs;
            var equipment = ContentManager.equipmentDefs;
            int commonMax = items.Where(x => x.tier == ItemTier.Tier1).Count();
            int uncommonMax = items.Where(x => x.tier == ItemTier.Tier2).Count();
            int legendaryMax = items.Where(x => x.tier == ItemTier.Tier3).Count();
            int commonVoidMax = items.Where(x => x.tier == ItemTier.VoidTier1).Count();
            int uncommonVoidMax = items.Where(x => x.tier == ItemTier.VoidTier2).Count();
            int legendaryVoidMax = items.Where(x => x.tier == ItemTier.VoidTier3).Count();
            int equipmentMax = equipment.Where(x => !x.isLunar).Count();
            int bossMax = items.Where(x => x.tier == ItemTier.Boss).Count();
            int lunarMax = items.Where(x => x.tier == ItemTier.Lunar).Count();
            int lunarEquipmentMax = equipment.Where(x => x.isLunar).Count();

            MultipleBossItems = config.Bind(AllowMultipleBossDef, true, AllowMultipleBossDesc);
            ModSettingsManager.AddOption(new CheckBoxOption(MultipleBossItems));

            IncludeLockedItems = config.Bind(IncludeLockedItemsDef, 1, IncludeLockedItemsDesc);
            ModSettingsManager.AddOption(new IntSliderOption(IncludeLockedItems, new IntSliderConfig() { min = 0, max = 2 }));

            UseItemTiers = config.Bind(UseItemTiersDef, true, UseItemTiersDesc);
            ModSettingsManager.AddOption(new CheckBoxOption(UseItemTiers));

            DefaultItemsShown = config.Bind(DefaultItemsShownDef, 5, DefaultItemsShownDesc);
            ModSettingsManager.AddOption(new IntSliderOption(DefaultItemsShown, new IntSliderConfig { min = 1, max = 100, checkIfDisabled = CheckItemTiers }));

            CommonItemsShown = config.Bind(CommonItemsShownDef, 5, CommonItemsShownDesc);
            ModSettingsManager.AddOption(new IntSliderOption(CommonItemsShown, new IntSliderConfig { min = 1, max = commonMax, checkIfDisabled = CheckItemTiersInverted }));

            UncommonItemsShown = config.Bind(UncommonItemsShownDef, 5, UncommonItemsShownDesc);
            ModSettingsManager.AddOption(new IntSliderOption(UncommonItemsShown, new IntSliderConfig { min = 1, max = uncommonMax, checkIfDisabled = CheckItemTiersInverted }));

            LegendaryItemsShown = config.Bind(LegendaryItemsShownDef, 5, LegendaryItemsShownDesc);
            ModSettingsManager.AddOption(new IntSliderOption(LegendaryItemsShown, new IntSliderConfig { min = 1, max = legendaryMax, checkIfDisabled = CheckItemTiersInverted }));

            CommonVoidItemsShown = config.Bind(CommonVoidItemsShownDef, 3, CommonVoidItemsShownDesc);
            ModSettingsManager.AddOption(new IntSliderOption(CommonVoidItemsShown, new IntSliderConfig { min = 1, max = commonVoidMax, checkIfDisabled = CheckItemTiersInverted }));

            UncommonVoidItemsShown = config.Bind(UncommonVoidItemsShownDef, 3, UncommonVoidItemsShownDesc);
            ModSettingsManager.AddOption(new IntSliderOption(UncommonVoidItemsShown, new IntSliderConfig { min = 1, max = uncommonVoidMax, checkIfDisabled = CheckItemTiersInverted }));

            LegendaryVoidItemsShown = config.Bind(LegendaryVoidItemsShownDef, 1, LegendaryVoidItemsShownDesc);
            ModSettingsManager.AddOption(new IntSliderOption(LegendaryVoidItemsShown, new IntSliderConfig { min = 1, max = legendaryVoidMax, checkIfDisabled = CheckItemTiersInverted }));

            EquipmentItemsShown = config.Bind(EquipmentItemsShownDef, 6, EquipmentItemsShownDesc);
            ModSettingsManager.AddOption(new IntSliderOption(EquipmentItemsShown, new IntSliderConfig { min = 1, max = equipmentMax, checkIfDisabled = CheckItemTiersInverted }));

            BossItemsShown = config.Bind(BossItemsShownDef, 5, BossItemsShownDesc);
            ModSettingsManager.AddOption(new IntSliderOption(BossItemsShown, new IntSliderConfig { min = 1, max = bossMax, checkIfDisabled = CheckItemTiersInverted }));

            LunarItemsShown = config.Bind(LunarItemsShownDef, 5, LunarItemsShownDesc);
            ModSettingsManager.AddOption(new IntSliderOption(LunarItemsShown, new IntSliderConfig { min = 1, max = lunarMax, checkIfDisabled = CheckItemTiersInverted }));

            LunarEquipmentItemsShown = config.Bind(LunarEquipmentItemsShownDef, 2, LunarEquipmentItemsShownDesc);
            ModSettingsManager.AddOption(new IntSliderOption(LunarEquipmentItemsShown, new IntSliderConfig { min = 1, max = lunarEquipmentMax, checkIfDisabled = CheckItemTiersInverted }));
        }

        static bool CheckItemTiers() { return UseItemTiers.Value; }
        static bool CheckItemTiersInverted() { return !UseItemTiers.Value; }
    }
}
